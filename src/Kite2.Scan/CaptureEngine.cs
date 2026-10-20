using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        // без стенки попытка висела бы часами (Kyocera TWAIN(2) молчала 15 минут
        // в twain_32 и без результата на новом диспетчере).
        private static readonly TimeSpan AttemptWall = TimeSpan.FromSeconds(75);
        // Родное окно драйвера требует участия человека: ждали бы неизвестно
        // сколько, поэтому стенка щедрая, но не бесконечная.
        private static readonly TimeSpan NativeUiWall = TimeSpan.FromMinutes(6);

        public static async Task<List<string>> GetSources()
        {
            return (await BuildCatalog(-1)).Select(source => source.Name).ToList();
        }

        public static async Task<List<string>> Capture(int sourceIndex, int dpi, int colour, string directory)
        {
            List<CatalogSource> sources = await BuildCatalog(-1);

            if (sources.Count <= sourceIndex)
                throw new InvalidOperationException(
                    "Выбранный источник не найден. Обновите список источников и повторите попытку.");

            CatalogSource source = sources[sourceIndex];

            Log.Info("Источник {0}: {1} ({2})", sourceIndex, source.Name, source.Device.Driver);

            var pages = new List<string>();
            string lastFailure = null;

            foreach (ScanAttempt attempt in AttemptsFor(source, sources))
            {
                Log.Info("Попытка: {0}", attempt.Title);

                var chapter = await TryCapture(attempt, dpi, colour, directory, pages.Count);

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

                lastFailure = chapter.Failure ?? (chapter.Abandoned
                    ? string.Format(
                        CultureInfo.CurrentCulture,
                        "Попытка «{0}» не отвечала {1} с и была пропущена; ожидается следующая.",
                        attempt.Title, (int)(attempt.Wall ?? AttemptWall).TotalSeconds)
                    : null);
            }

            throw new InvalidOperationException(lastFailure ?? "Сканер не отдал ни одной страницы.");
        }

        // Одна TWAIN-попытка в дочернем процессе: Kyocera TWAIN(2) виснет на
        // повторном открытии диспетчера в том же процессе, а свежий процесс
        // диспетчер для себя открывает с чистого листа. Страницы пишутся прямо
        // в каталог серии — их жизнь не зависит от жизни процессов.
        internal static async Task<AttemptChapter> TryTwainChild(
            int dsm, int mode, int dpi, int colour, string directory, string name, int pagesBefore)
        {
            TwainDsm dispatcher = ParseDsm(dsm);
            TwainTransferMode transfer = ParseMode(mode);

            Log.Info(
                "Попытка в дочернем процессе: TWAIN ({0}, передача {1}), {2}",
                dispatcher == TwainDsm.New ? "новый диспетчер" : "старый диспетчер",
                transfer == TwainTransferMode.Memory ? "памятью" : "целиком",
                name);

            var attempt = new ScanAttempt
            {
                Driver = Driver.Twain,
                Device = new ScanDevice(Driver.Twain, name, name),
                Dsm = dispatcher,
                Mode = transfer,
                Title = "TWAIN (дочерний процесс)"
            };

            var chapter = new AttemptChapter();
            var controller = NewController();
            var consume = ConsumePages(attempt, dpi, colour, directory, pagesBefore, chapter, controller);

            // Стенка у дочернего не своя: следит родитель, а сюда доходит только
            // таймер тишины из ConsumePages. Ждём до конца, не отрезая.
            await consume;

            return chapter;
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
            public ScanDevice Device;
            public TwainDsm Dsm;
            public TwainTransferMode Mode;
            public WiaApiVersion WiaVersion;
            public PaperSource PaperSource = PaperSource.Auto;
            // Родное окно драйвера вместо программных значений: молчаливые
            // приложения-драйверы Kyocera часто говорят только собственному окну.
            public bool NativeUi;
            public string Title;

            // Стенка попытки; null — общий AttemptWall.
            public TimeSpan? Wall;
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

        // Лестница попыток уходит от старой иерархии «весь TWAIN, потом WIA»:
        // у Kyocera молчит TWAIN, а родное окно WIA-драйвера работает там, где
        // программная передача падает. Лестница перемешана так, чтобы самый
        // вероятный успех не ждал за самыми долгими попытками.
        private static List<ScanAttempt> AttemptsFor(CatalogSource source, List<CatalogSource> sources)
        {
            var attempts = new List<ScanAttempt>();

            if (source.Device.Driver == Driver.Twain)
            {
                TwainDsm first = source.TwainDsm == TwainDsm.New ? TwainDsm.New : TwainDsm.Old;
                TwainDsm second = source.TwainDsm == TwainDsm.New ? TwainDsm.Old : TwainDsm.New;

                // WIA-запасной ищется по одному устройству: названия в TWAIN и WIA
                // несхожи («Kyocera TWAIN(2)» и «Kyocera FS-1120MFP WIA Driver»),
                // но на машине с одним сканером WIA-дубль — он всегда последний в
                // списке, и не совпадать по имени дешевле и надёжнее.
                CatalogSource wia = sources.FirstOrDefault(s => s.Device.Driver == Driver.Wia);

                attempts.Add(TwainAttempt(source.Device, first, TwainTransferMode.Memory));

                if (wia != null)
                    attempts.Add(WiaAttempt(wia, PaperSource.Auto, "WIA (та же модель)"));

                if (wia != null)
                    attempts.Add(WiaAttempt(wia, PaperSource.Feeder, "WIA (та же модель, автоподатчик)"));

                attempts.Add(TwainAttempt(source.Device, first, TwainTransferMode.Native));

                if (wia != null)
                    attempts.Add(new ScanAttempt
                    {
                        Driver = Driver.Wia,
                        Device = wia.Device,
                        WiaVersion = wia.WiaVersion,
                        NativeUi = true,
                        Wall = NativeUiWall,
                        Title = "WIA (родное окно драйвера)"
                    });

                attempts.Add(TwainAttempt(source.Device, second, TwainTransferMode.Memory));
                attempts.Add(TwainAttempt(source.Device, second, TwainTransferMode.Native));
            }
            else
            {
                CatalogSource self = sources.First(s => s.Device.Driver == Driver.Wia && s.Name == source.Name);

                attempts.Add(WiaAttempt(self, PaperSource.Auto, "WIA"));
                attempts.Add(WiaAttempt(self, PaperSource.Feeder, "WIA (автоподатчик)"));

                attempts.Add(new ScanAttempt
                {
                    Driver = Driver.Wia,
                    Device = self.Device,
                    WiaVersion = self.WiaVersion,
                    NativeUi = true,
                    Wall = NativeUiWall,
                    Title = "WIA (родное окно драйвера)"
                });
            }

            return attempts;
        }

        private static ScanAttempt TwainAttempt(ScanDevice device, TwainDsm dsm, TwainTransferMode mode)
        {
            string dispatcher = dsm == TwainDsm.New ? "новый диспетчер" : "старый диспетчер";
            string transfer = mode == TwainTransferMode.Memory ? "передача памятью" : "передача целиком";

            return new ScanAttempt
            {
                Driver = Driver.Twain,
                Device = device,
                Dsm = dsm,
                Mode = mode,
                Title = "TWAIN (" + dispatcher + ", " + transfer + ")"
            };
        }

        private static ScanAttempt WiaAttempt(CatalogSource source, PaperSource paperSource, string title) =>
            new ScanAttempt
            {
                Driver = Driver.Wia,
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
            ScanAttempt attempt, int dpi, int colour, string directory, int pagesBefore)
        {
            var chapter = new AttemptChapter();
            TimeSpan wall = attempt.Wall ?? AttemptWall;

            using (var child = attempt.Driver == Driver.Twain ? TwainChild.Start(attempt, dpi, colour, directory, pagesBefore) : null)
            {
                var consume = child != null
                    ? ConsumeTwainChild(child, attempt, directory, pagesBefore, chapter)
                    : ConsumePages(
                        attempt, dpi, colour, directory, pagesBefore, chapter, NewController());

                // Стенка на попытку, но не жёсткая: если страницы идут — стенка
                // перезапускается по каждой пришедшей. Бросаем только когда вызов
                // TWAIN завис и за всё время не отдал ничего нового.
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

                    // Дочерний TWAIN-процесс остаётся с зависшим драйвером: гасим,
                    // до страниц, которые он успел записать, добирается ниже.
                    child?.Dispose();

                    break;
                }
            }

            // Дочерний процесс, живший на стенку тишины, отдал бы страницы файлом
            // ответа, но убитый — только тем, что писал в каталог серии. Всё
            // написанное в каталоге этой серии берём оттуда, если ответ пуст.
            if (chapter.Pages.Count == 0 && attempt.Driver == Driver.Twain)
                FillPagesFromDirectory(chapter, directory);

            return chapter;
        }

        private static void FillPagesFromDirectory(AttemptChapter chapter, string directory)
        {
            // Серия очищает каталог прежде захвата, поэтому scan*.bmp здесь —
            // только страницы этой серии.
            AppendSortedScans(chapter, Directory.GetFiles(directory, "scan*.bmp"));
        }

        // TWAIN всегда издёргивается в дочернем процессе: зависший вызов тянет
        // за собой весь процесс, и осиротевший ребёнок гасится сам, увидев
        // смерть родителя.
        private sealed class TwainChild : IDisposable
        {
            public Process Process;

            public string ResponsePath;

            public static TwainChild Start(
                ScanAttempt attempt, int dpi, int colour, string directory, int pagesBefore)
            {
                Process self = Process.GetCurrentProcess();

                string responsePath = Path.Combine(
                    Path.GetTempPath(),
                    "kite2-twain-" + Guid.NewGuid().ToString("N") + ".txt");

                string arguments = string.Format(
                    CultureInfo.InvariantCulture,
                    "twain {0} {1} {2} {3} {4} \"{5}\" \"{6}\" {7} \"{8}\"",
                    (int)attempt.Dsm,
                    (int)attempt.Mode,
                    dpi,
                    colour,
                    pagesBefore,
                    directory,
                    attempt.Device.Name,
                    self.Id,
                    responsePath);

                var child = new TwainChild
                {
                    Process = Process.Start(new ProcessStartInfo(self.MainModule.FileName, arguments)
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }),
                    ResponsePath = responsePath
                };

                Log.Info(
                    "Попытка TWAIN идёт в отдельном процессе: диспетчер={0}, режим={1}",
                    attempt.Dsm, attempt.Mode);

                return child;
            }

            public void Dispose()
            {
                try
                {
                    if (Process != null && !Process.HasExited)
                        Process.Kill();
                }
                catch (Exception)
                {
                    // Сам ушёл — и слава богу.
                }
            }
        }

        private static async Task ConsumeTwainChild(
            TwainChild child, ScanAttempt attempt,
            string directory, int pagesBefore, AttemptChapter chapter)
        {
            await Task.Run(() => child.Process.WaitForExit());

            if (chapter.Abandoned)
                return;

            // Список страниц приходит файлом ответа; имена файлов в каталоге
            // те же, что пишутся обычным захватом.
            if (File.Exists(child.ResponsePath))
            {
                ScanResult result = ScanExchange.Read(child.ResponsePath);

                if (result.Succeeded)
                {
                    AppendSortedScans(chapter, result.Items.ToArray());
                    return;
                }

                chapter.Failure = result.Error;
            }

            // Может случиться и так, что ребёнок умер насильственно, а файл
            // ответа не написан: страницы на диске всё ещё существуют.
            chapter.Failure ??= "Дочерний процесс сканирования завершился без ответа.";
        }

        // Список файлов нужен серии по порядку номеров: страницы пронумеруются
        // scan001... scanNNN, и лексический порядок нулей означает хронологию.
        private static void AppendSortedScans(AttemptChapter chapter, string[] files)
        {
            chapter.Pages.AddRange(files
                .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
                .ToArray());
        }

        private static async Task ConsumePages(
            ScanAttempt attempt, int dpi, int colour,
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
                        // NAPS2 требует точного устройства: имени для этого мало.
                        Device = attempt.Device,
                        // Наличие податчика отдаём решать драйверу.
                        PaperSource = attempt.PaperSource,
                        Dpi = dpi,
                        BitDepth = colour == ScanExchange.BlackAndWhite ? BitDepth.BlackAndWhite : BitDepth.Color,
                        // Размер задаётся всегда: без него NAPS2 вписывает Letter
                        // и обрезает низ А4.
                        PageSize = PageSize.A4,
                        // Родное окно WIA-драйверу: молчаливый драйвер Kyocera
                        // программную передачу отдаёт E_FAIL.
                        UseNativeUI = attempt.NativeUi
                    };

                    if (attempt.Driver == Driver.Twain)
                        options.TwainOptions = new TwainOptions { Dsm = attempt.Dsm, TransferMode = attempt.Mode };
                    else
                        options.WiaOptions = new WiaOptions { WiaApiVersion = attempt.WiaVersion };

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

        private static TwainDsm ParseDsm(int value)
        {
            if (value == (int)TwainDsm.New)
                return TwainDsm.New;

            return TwainDsm.Old;
        }

        private static TwainTransferMode ParseMode(int value)
        {
            if (value == (int)TwainTransferMode.Native)
                return TwainTransferMode.Native;

            return TwainTransferMode.Memory;
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
