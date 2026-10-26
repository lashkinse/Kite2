using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAPS2.Images;
using NAPS2.Images.Gdi;
using NAPS2.Scan;
using NAPS2.Scan.Exceptions;
using NLog;
namespace Kite2.Scan
{
    // Движок — NAPS2.Sdk (LGPL): WIA под одним API. Работает в отдельном
    // процессе: у части старых сканеров WIA-драйверы живут по x86, да и сбой
    // драйвера гасит помощника, а не окно программы. Внутри — только список
    // устройств, порядок попыток обхода капризных драйверов и паузы тишины.
    internal static class CaptureEngine
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // Тишина ждётся терпеливо: разогрев Kyocera и приём страницы тянутся
        // десятки секунд, а короткая отсечка бросала попытку в момент разгрузки.
        // 2,5 минуты: и драйверу простор, и пользователю не вечность.
        private static readonly TimeSpan FirstPageWait = TimeSpan.FromSeconds(150);
        private static readonly TimeSpan PageGapWait = TimeSpan.FromSeconds(100);
        private static readonly TimeSpan ListingWait = TimeSpan.FromSeconds(10);
        // Стенка на всю попытку: таймер тишины доходит до итератора только между
        // страницами, а молчаливый драйвер может зависнуть внутри вызова —
        // без стенки попытка висела бы часами.
        private static readonly TimeSpan AttemptWall = TimeSpan.FromSeconds(180);
        // Родное окно драйвера требует участия человека: ждали бы неизвестно
        // сколько, поэтому стенка щедрая, но не бесконечная.
        private static readonly TimeSpan NativeUiWall = TimeSpan.FromMinutes(6);

        public static async Task<List<string>> GetSources()
        {
            return (await BuildCatalog(-1)).Select(source => source.Name).ToList();
        }

        public static async Task<List<string>> Capture(int sourceIndex, string directory)
        {
            List<CatalogSource> sources = await BuildCatalog(sourceIndex);

            if (sources.Count <= sourceIndex)
                throw new InvalidOperationException(
                    "Выбранный источник не найден. Обновите список источников и повторите попытку.");

            CatalogSource source = sources[sourceIndex];

            Log.Info("Источник {0}: {1} ({2})", sourceIndex, source.Name, source.Device.Driver);

            var pages = new List<string>();
            string lastFailure = null;

            foreach (ScanAttempt attempt in AttemptsFor(source))
            {
                Log.Info("Попытка: {0}", attempt.Title);

                var chapter = await TryCapture(attempt, directory, pages.Count);

                if (chapter.Failure != null)
                    Log.Warn("Попытка не сработала: {0}", chapter.Failure);
                else if (chapter.Abandoned)
                    Log.Warn(
                        "Попытка не отвечала {0} с и брошена",
                        (int)(attempt.Wall ?? AttemptWall).TotalSeconds);

                // Страницы важнее гладкой серии: частичный захват уже полезен,
                // а пустой даёт шанс следующей попытке.
                if (chapter.Pages.Count > 0)
                {
                    pages.AddRange(chapter.Pages);
                    return pages;
                }

                // Родное окно показывалось человеку и закрылось без страниц:
                // сканирование было отменено в нём. Тихие программные попытки
                // после этого только просканируют стекло ещё раз без результата.
                // Если помощник назвал причину сам (например, драйвер сорвал
                // передачу), его слово и есть ответ — своё не выдумываем.
                if (attempt.NativeUi && !chapter.Abandoned && chapter.Pages.Count == 0
                    && chapter.Failure == null)
                {
                    throw new InvalidOperationException(
                        "Окно драйвера закрылось без страницы: сканирование в нём отменено. " +
                        "Нажмите «Сканировать» ещё раз и выполните скан в окне драйвера.");
                }

                lastFailure = chapter.Failure ?? (chapter.Abandoned
                    ? string.Format(
                        CultureInfo.CurrentCulture,
                        "Попытка «{0}» не отвечала {1} с и была пропущена; ожидается следующая.",
                        attempt.Title, (int)(attempt.Wall ?? AttemptWall).TotalSeconds)
                    : null);
            }

            throw new InvalidOperationException(lastFailure ?? "Сканер не отдал ни одной страницы.");
        }

        // Источник адресуется индексом, поэтому порядок устройств обязан совпадать
        // в «sources» и в «acquire».
        private sealed class CatalogSource
        {
            public int Index;
            public string Name;
            public ScanDevice Device;
            public WiaApiVersion WiaVersion;
        }

        private sealed class ScanAttempt
        {
            public ScanDevice Device;
            public WiaApiVersion WiaVersion;
            public PaperSource PaperSource = PaperSource.Auto;
            // Родное окно драйвера вместо программных значений: молчаливые
            // приложения-драйверы Kyocera часто говорят только собственному окну.
            public bool NativeUi;
            public string Title;

            // Стенка попытки; null — общий AttemptWall.
            public TimeSpan? Wall;
        }

        private static async Task<List<CatalogSource>> BuildCatalog(int neededIndex)
        {
            var sources = new List<CatalogSource>();
            var controller = NewController();
            int index = 0;

            foreach (var options in CatalogOptions())
            {
                await CollectDevices(sources, controller, options, index);
                index = sources.Count;

                if (neededIndex >= 0 && neededIndex < sources.Count)
                    return sources;
            }

            Log.Info("Каталог собран: источников {0}", sources.Count);

            return sources;
        }

        // Один ScanOptions-экземпляр не переиспользуется между опросами.
        private static IEnumerable<ScanOptions> CatalogOptions()
        {
            // Захват полностью перешёл на WIA: TWAIN вырезан (решение
            // пользователя). Вариантов WIA-интерфейса осталось три ради
            // совместимости: часть драйверов отвечает только поверх 1.0,
            // часть молчит в нём и говорит по 2.0.
            yield return new ScanOptions { Driver = Driver.Wia, WiaOptions = { WiaApiVersion = WiaApiVersion.Default } };
            yield return new ScanOptions { Driver = Driver.Wia, WiaOptions = { WiaApiVersion = WiaApiVersion.Wia10 } };
            yield return new ScanOptions { Driver = Driver.Wia, WiaOptions = { WiaApiVersion = WiaApiVersion.Wia20 } };
        }

        private static async Task CollectDevices(
            List<CatalogSource> sources, ScanController controller, ScanOptions options, int startIndex)
        {
            string label = OptionsLabel(options);

            try
            {
                Log.Info("Опрос {0}...", label);

                var devices = await ListDevices(controller, options);

                foreach (ScanDevice device in devices.Where(device => sources.All(known => known.Name != device.Name)))
                {
                    sources.Add(new CatalogSource
                    {
                        Index = sources.Count,
                        Name = device.Name,
                        Device = device,
                        WiaVersion = options.WiaOptions.WiaApiVersion
                    });
                }

                Log.Info("Список {0}: устройств {1}", label, devices.Count);
            }
            catch (Exception ex)
            {
                // Диспетчера может не быть вовсе: пустой список — оговорка каталога,
                // ошибка только если не осталось ни одного способа найти сканер.
                Log.Warn("Список {0} не получен: {1}", label, ex.Message);
            }
        }

        private static string OptionsLabel(ScanOptions options) =>
            "WIA (" + options.WiaOptions.WiaApiVersion + ")";

        private static async Task<List<ScanDevice>> ListDevices(ScanController controller, ScanOptions options)
        {
            var listing = controller.GetDeviceList(options);
            var done = await Task.WhenAny(listing, Task.Delay(ListingWait));

            if (done != listing)
                throw new TimeoutException("Перечисление не ответило за 10 с");

            return await listing;
        }

        // Лестница попыток: только источник, который выбрал человек. Скан
        // выполняется напрямую, без окна драйвера (просьба пользователя:
        // «заставить окно не появляться, а напрямую сканировать»). Родное
        // окно — лишь последний резерв: оно добирается только в том случае,
        // если все прямые попытки кончились, не сканировав стекло (иначе
        // сработало бы правило остановки на пустой передаче — окно лишь
        // заставило бы сканер трудиться заново).
        private static List<ScanAttempt> AttemptsFor(CatalogSource source)
        {
            var attempts = new List<ScanAttempt>
            {
                WiaAttempt(source, PaperSource.Auto, "WIA"),
                WiaAttempt(source, PaperSource.Feeder, "WIA (автоподатчик)")
            };

            attempts.Add(new ScanAttempt
            {
                Device = source.Device,
                WiaVersion = source.WiaVersion,
                NativeUi = true,
                Wall = NativeUiWall,
                Title = "WIA (родное окно драйвера)"
            });

            return attempts;
        }

        private static ScanAttempt WiaAttempt(CatalogSource source, PaperSource paperSource, string title) =>
            new ScanAttempt
            {
                Device = source.Device,
                WiaVersion = source.WiaVersion,
                PaperSource = paperSource,
                Title = title
            };

        internal sealed class AttemptChapter
        {
            public readonly List<string> Pages = new List<string>();

            // null — попытка тихо закончилась без страниц (пустой лоток и т.п.).
            public string Failure;

            // true — попытка брошена по стенке тишины: страницы могли прийти от
            // зависшего вызова позже, и их приём нужен только до брошенной отметки.
            public bool Abandoned;
        }

        private static async Task<AttemptChapter> TryCapture(
            ScanAttempt attempt, string directory, int pagesBefore)
        {
            var chapter = new AttemptChapter();
            TimeSpan wall = attempt.Wall ?? AttemptWall;

            var consume = ConsumePages(attempt, directory, pagesBefore, chapter, NewController());

            // Стенка на попытку, но не жёсткая: если страницы идут — стенка
            // перезапускается по каждой пришедшей. Бросаем только когда вызов
            // зависит и за всё время не отдал ничего нового.
            int lastCount = 0;

            while (true)
            {
                Task done = await Task.WhenAny(consume, Task.Delay(wall));

                if (done == consume)
                    break;

                if (chapter.Pages.Count != lastCount)
                {
                    // Страница прокралась ровно в момент стенки: время пошло заново.
                    lastCount = chapter.Pages.Count;
                    continue;
                }

                chapter.Abandoned = true;
                chapter.Failure ??= string.Format(
                    CultureInfo.CurrentCulture,
                    "Попытка молчала больше {0} с: драйвер не отвечает.",
                    (int)wall.TotalSeconds);

                break;
            }

            return chapter;
        }

        private static async Task ConsumePages(
            ScanAttempt attempt,
            string directory, int pagesBefore, AttemptChapter chapter, ScanController controller)
        {
            using (var quiet = new CancellationTokenSource())
            using (var visible = new CancellationTokenSource())
            {
                // В родном окне драйвера страниц не дождаться и от машинного
                // вызова: человек должен сам щёлкнуть «Скан» в диалоге.
                TimeSpan firstPageWait = attempt.NativeUi ? NativeUiWall : FirstPageWait;
                TimeSpan pageGapWait = attempt.NativeUi ? NativeUiWall : PageGapWait;

                // CancelAfter перезапускает таймер целиком, поэтому одна и та же
                // метка отсчитывает тишину и до первой страницы, и между страницами.
                quiet.CancelAfter(firstPageWait);

                // Пока открыто родное окно, его держат наверху: иначе человек
                // мог бы его не найти и закрыть, отменив скан.
                Task raising = attempt.NativeUi
                    ? Task.Run(() => RaiseDriverWindowLoop(visible.Token))
                    : Task.CompletedTask;

                try
                {
                    var options = new ScanOptions
                    {
                        // NAPS2 требует точного устройства: имени для этого мало.
                        Device = attempt.Device,
                        // Наличие податчика отдаём решать драйверу.
                        PaperSource = attempt.PaperSource,
                        // Ни dpi, ни цветности: их человек задаст в окне драйвера.
                        // Размер задаётся всегда: без него NAPS2 вписывает Letter
                        // и обрезает низ А4.
                        PageSize = PageSize.A4,
                        // Родное окно WIA-драйверу: молчаливый драйвер Kyocera
                        // программную передачу отдаёт E_FAIL.
                        UseNativeUI = attempt.NativeUi,
                        WiaOptions = new WiaOptions { WiaApiVersion = attempt.WiaVersion }
                    };

                    await foreach (var page in controller.Scan(options, quiet.Token))
                    {
                        // Зависший вызов мог проснуться после броска попытки:
                        // страница уже не нужна.
                        if (chapter.Abandoned)
                            break;

                        string path = Path.Combine(
                            directory,
                            "scan" + (pagesBefore + chapter.Pages.Count + 1).ToString("D3") + ".bmp");

                        using (page)
                            page.Save(path);

                        chapter.Pages.Add(path);
                        Log.Info("Страница {0}: {1} ({2})", pagesBefore + chapter.Pages.Count, path, attempt.Title);

                        quiet.CancelAfter(pageGapWait);
                    }
                }
                catch (OperationCanceledException)
                {
                    Log.Warn("Попытка прервана по тишине, страниц {0}", chapter.Pages.Count);

                    if (chapter.Pages.Count == 0)
                    {
                        chapter.Failure = attempt.NativeUi
                            ? string.Format(
                                CultureInfo.CurrentCulture,
                                "За {0} мин в окне драйвера страница не была просканирована.",
                                (int)firstPageWait.TotalMinutes)
                            : string.Format(
                                CultureInfo.CurrentCulture,
                                "Сканер молчит уже {0} с и не отдал ни одной страницы. " +
                                "Проверьте, включён ли он и заправлена ли бумага в лоток.",
                                (int)FirstPageWait.TotalSeconds);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Попытка упала: {0}", ex);
                    chapter.Failure = GetFailureMessage(ex, chapter.Pages.Count > 0);
                }
                finally
                {
                    visible.Cancel();
                }

                // Подниматель окна не важнее скана: его сбой игнорируем.
                try
                {
                    await raising;
                }
                catch
                {
                }
            }
        }

        // Пока открыто родное окно драйвера, каждые полсекунды поднимаем его
        // наверх: без этого окно прячется за окном программы, и человек
        // «закрывает окно без страницы», мог и не найти его.
        private static async Task RaiseDriverWindowLoop(CancellationToken cancellation)
        {
            int self = Process.GetCurrentProcess().Id;

            while (!cancellation.IsCancellationRequested)
            {
                WindowRaiser.RaiseOwnedWindows(self);

                try
                {
                    await Task.Delay(500, cancellation);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }
        }

        private static ScanController NewController()
        {
            // Диагностика NAPS2 иначе никуда не пишется: именно там видна причина
            // молчаливой передачи.
            var context = new ScanningContext(new GdiImageContext()) { Logger = Naps2Logging.Instance };

            return new ScanController(context);
        }

        private static string GetFailureMessage(Exception error, bool hasPages)
        {
            // Драйвер уже показал человеку свою ошибку (Kyocera приходит так
            // на пустом лотке) — сверху останется общий текст про отсутствие страниц.
            if (error is AlreadyHandledDriverException)
                return null;

            if (error is DeviceFeederEmptyException)
                return hasPages ? null : "В лотке сканера нет бумаги.";

            if (error is DeviceOfflineException)
                return "Сканер не отвечает. Проверьте питание и подключение.";

            if (error is DeviceBusyException)
                return "Сканер занят. Дождитесь окончания предыдущей работы.";

            if (error is DeviceWarmingUpException)
                return "Сканер разогревается. Повторите попытку через минуту.";

            if (error is DeviceCoverOpenException)
                return "Открыта крышка или лоток сканера.";

            if (error is DevicePaperJamException)
                return "Замятие бумаги в сканере. Откройте крышку и удалите лист.";

            if (error is DeviceNotFoundException)
                return "Сканер не найден. Проверьте подключение и обновите список источников.";

            if (error is DeviceCommunicationException)
                return "Связь со сканером прервалась. Проверьте подключение и повторите.";

            if (error is NoFeederSupportException)
                return "У сканера нет автоподатчика. Положите лист на стекло планшета.";

            if (error is DeviceException)
                return "Сканер сообщил об ошибке: " + error.Message;

            return error.Message;
        }
    }
}
