using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kite2.ScanProtocol;
using NAPS2.Images;
using NAPS2.Images.Gdi;
using NAPS2.Scan;
using NAPS2.Scan.Exceptions;
using NLog;

namespace Kite2.Scan
{
    // Движок — NAPS2.Sdk (LGPL): TWAIN обоих диспетчеров и WIA под одним API.
    // Работает в отдельном 32-разрядном процессе: драйверу TWAIN нужен 32-битный
    // вызывающий процесс, а само приложение x64. Внутри остаётся только список
    // устройств, порядок попыток обхода капризных драйверов и паузы тишины.
    internal static class CaptureEngine
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly TimeSpan FirstPageWait = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan PageGapWait = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan ListingWait = TimeSpan.FromSeconds(10);
        // Стенка на всю попытку: таймер тишины доходит до итератора только между
        // страницами, а молчаливый драйвер может зависнуть внутри вызова TWAIN —
        // без стенки попытка висела бы часами (15 минут на Kyocera TWAIN(2)).
        private static readonly TimeSpan AttemptWall = TimeSpan.FromSeconds(120);

        public static async Task<List<string>> GetSources()
        {
            return (await BuildCatalog(-1)).Select(source => source.Name).ToList();
        }

        public static async Task<List<string>> Capture(int sourceIndex, int dpi, int colour, string directory)
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

                var chapter = await TryCapture(source, attempt, dpi, colour, directory, pages.Count);

                if (chapter.Failure != null)
                    Log.Warn("Попытка не сработала: {0}", chapter.Failure);
                else if (chapter.Abandoned)
                    Log.Warn("Попытка не отвечала {0} с и брошена", (int)AttemptWall.TotalSeconds);

                // Страницы важнее гладкой серии: частичный захват уже полезен,
                // а пустой даёт шанс следующей попытке.
                if (chapter.Pages.Count > 0)
                {
                    pages.AddRange(chapter.Pages);
                    return pages;
                }

                lastFailure = chapter.Failure ?? (chapter.Abandoned
                    ? string.Format(
                        CultureInfo.CurrentCulture,
                        "Попытка «{0}» не отвечала {1} с и была пропущена; ожидается следующая.",
                        attempt.Title, (int)AttemptWall.TotalSeconds)
                    : null);
            }

            throw new InvalidOperationException(lastFailure ?? "Сканер не отдал ни одной страницы.");
        }

        // Источник адресуется индексом, поэтому порядок устройств обязан совпадать
        // в «sources» и в «acquire»: сначала TWAIN обоих диспетчеров, затем WIA.
        private sealed class CatalogSource
        {
            public int Index;
            public string Name;
            public ScanDevice Device;
            public WiaApiVersion WiaVersion;
            // Диспетчер, чей опрос нашёл устройство: ему первая попытка и доверяем.
            public TwainDsm TwainDsm = TwainDsm.Old;
        }

        private sealed class ScanAttempt
        {
            public Driver Driver;
            public TwainDsm Dsm;
            public TwainTransferMode Mode;
            public string Title;
        }

        // Одна стройка каталога для «sources» и «acquire»: индексация устройств
        // обязана совпадать. neededIndex правит ранний выход: acquire не обязан
        // ждать опрос молчаливого старого диспетчера ради источников дальше.
        private static async Task<List<CatalogSource>> BuildCatalog(int neededIndex)
        {
            var sources = new List<CatalogSource>();
            var controller = NewController();
            int index = 0;

            foreach (var options in CatalogOptions())
            {
                // Каждая TWAIN-версия счастливо увидит свои устройства; если новый
                // диспетчер уже что-то нашёл, старая — это чистая трата времени:
                // молчаливый twain_32 отделялся на 10 с (Kyocera TWAIN(2)).
                if (options.Driver == Driver.Twain && options.TwainOptions.Dsm == TwainDsm.Old
                    && sources.Any(s => s.Device.Driver == Driver.Twain))
                    continue;

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
            yield return new ScanOptions { Driver = Driver.Twain, TwainOptions = { Dsm = TwainDsm.New } };
            yield return new ScanOptions { Driver = Driver.Twain, TwainOptions = { Dsm = TwainDsm.Old } };
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
                        WiaVersion = options.Driver == Driver.Wia ? options.WiaOptions.WiaApiVersion : default,
                        TwainDsm = options.Driver == Driver.Twain ? options.TwainOptions.Dsm : TwainDsm.Old
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
            options.Driver == Driver.Twain
                ? "TWAIN (" + options.TwainOptions.Dsm + ")"
                : "WIA (" + options.WiaOptions.WiaApiVersion + ")";

        private static async Task<List<ScanDevice>> ListDevices(ScanController controller, ScanOptions options)
        {
            var listing = controller.GetDeviceList(options);
            var done = await Task.WhenAny(listing, Task.Delay(ListingWait));

            if (done != listing)
                throw new TimeoutException("Перечисление не ответило за 10 с");

            return await listing;
        }

        // Первой идёт попытка на том диспетчере, чей опрос нашёл устройство;
        // остальные — запасные. Kyocera TWAIN(2) нашлась новым диспетчером, но
        // зависает внутри старого twain_32 на полдня — попытка там более не первая.
        private static List<ScanAttempt> AttemptsFor(CatalogSource source)
        {
            var attempts = new List<ScanAttempt>();

            if (source.Device.Driver == Driver.Twain)
            {
                if (source.TwainDsm == TwainDsm.New)
                {
                    attempts.Add(new ScanAttempt { Driver = Driver.Twain, Dsm = TwainDsm.New, Mode = TwainTransferMode.Memory, Title = "TWAIN (новый диспетчер, передача памятью)" });
                    attempts.Add(new ScanAttempt { Driver = Driver.Twain, Dsm = TwainDsm.New, Mode = TwainTransferMode.Native, Title = "TWAIN (новый диспетчер, передача целиком)" });
                    attempts.Add(new ScanAttempt { Driver = Driver.Twain, Dsm = TwainDsm.Old, Mode = TwainTransferMode.Memory, Title = "TWAIN (старый диспетчер, передача памятью)" });
                    attempts.Add(new ScanAttempt { Driver = Driver.Twain, Dsm = TwainDsm.Old, Mode = TwainTransferMode.Native, Title = "TWAIN (старый диспетчер, передача целиком)" });
                }
                else
                {
                    attempts.Add(new ScanAttempt { Driver = Driver.Twain, Dsm = TwainDsm.Old, Mode = TwainTransferMode.Memory, Title = "TWAIN (старый диспетчер, передача памятью)" });
                    attempts.Add(new ScanAttempt { Driver = Driver.Twain, Dsm = TwainDsm.New, Mode = TwainTransferMode.Memory, Title = "TWAIN (новый диспетчер, передача памятью)" });
                    attempts.Add(new ScanAttempt { Driver = Driver.Twain, Dsm = TwainDsm.New, Mode = TwainTransferMode.Native, Title = "TWAIN (новый диспетчер, передача целиком)" });
                    attempts.Add(new ScanAttempt { Driver = Driver.Twain, Dsm = TwainDsm.Old, Mode = TwainTransferMode.Native, Title = "TWAIN (старый диспетчер, передача целиком)" });
                }
            }
            else
            {
                attempts.Add(new ScanAttempt { Driver = Driver.Wia, Title = "WIA" });
            }

            return attempts;
        }

        private sealed class AttemptChapter
        {
            public readonly List<string> Pages = new List<string>();

            // null — попытка тихо закончилась без страниц (пустой лоток и т.п.).
            public string Failure;

            // true — попытка брошена по стенке тишины: страницы могли прийти от
            // зависшего вызова позже, и их приём нужен только до брошенной отметки.
            public bool Abandoned;
        }

        private static async Task<AttemptChapter> TryCapture(
            CatalogSource source, ScanAttempt attempt, int dpi, int colour, string directory, int pagesBefore)
        {
            var chapter = new AttemptChapter();
            var controller = NewController();

            var consume = ConsumePages(
                source, attempt, dpi, colour, directory, pagesBefore, chapter, controller);

            // Стенка на попытку, но не жёсткая: если страницы идут — стенка
            // перезапускается по каждой пришедшей. Бросаем только когда вызов
            // TWAIN завис и за всё время не отдал ничего нового.
            int lastCount = 0;

            while (true)
            {
                Task done = await Task.WhenAny(consume, Task.Delay(AttemptWall));

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
                    (int)AttemptWall.TotalSeconds);
                break;
            }

            return chapter;
        }

        private static async Task ConsumePages(
            CatalogSource source, ScanAttempt attempt, int dpi, int colour,
            string directory, int pagesBefore, AttemptChapter chapter, ScanController controller)
        {
            using (var quiet = new CancellationTokenSource())
            {
                // CancelAfter перезапускает таймер целиком, поэтому одна и та же
                // метка отсчитывает тишину и до первой страницы, и между страницами.
                quiet.CancelAfter(FirstPageWait);

                try
                {
                    var options = new ScanOptions
                    {
                        Driver = attempt.Driver,
                        // NAPS2 требует точного устройства: имени для этого мало.
                        Device = source.Device,
                        // Наличие податчика отдаём решать драйверу.
                        PaperSource = PaperSource.Auto,
                        Dpi = dpi,
                        BitDepth = colour == ScanExchange.BlackAndWhite ? BitDepth.BlackAndWhite : BitDepth.Color,
                        // Размер задаётся всегда: без него NAPS2 вписывает Letter
                        // и обрезает низ А4.
                        PageSize = PageSize.A4,
                        TwainOptions = { Dsm = attempt.Dsm, TransferMode = attempt.Mode }
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

                        quiet.CancelAfter(PageGapWait);
                    }
                }
                catch (OperationCanceledException)
                {
                    Log.Warn("Попытка прервана по тишине, страниц {0}", chapter.Pages.Count);

                    if (chapter.Pages.Count == 0)
                        chapter.Failure = string.Format(
                            CultureInfo.CurrentCulture,
                            "Сканер молчит уже {0} с и не отдал ни одной страницы. " +
                            "Проверьте, включён ли он и заправлена ли бумага в лоток.",
                            (int)FirstPageWait.TotalSeconds);
                }
                catch (Exception ex)
                {
                    Log.Error("Попытка упала: {0}", ex);
                    chapter.Failure = GetFailureMessage(ex, chapter.Pages.Count > 0);
                }
            }
        }

        private static ScanController NewController() =>
            new ScanController(new ScanningContext(new GdiImageContext()));

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
