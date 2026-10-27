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
    // Захват через NAPS2.Sdk в отдельном процессе: сбой драйвера гасит
    // помощника, а не окно программы. Единственный способ захвата — родное
    // окно драйвера: параметры задаёт там человек, страницы идут потоком.
    internal static class CaptureEngine
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // Стенка тишины: драйвер может зависнуть внутри вызова — следит за
        // страницами и завершением задачи, а таймер тишины перезапускается.
        private static readonly TimeSpan QuietWall = TimeSpan.FromMinutes(6);
        private static readonly TimeSpan ListingWait = TimeSpan.FromSeconds(10);

        // Дескриптор окна программы — владелец диалога драйвера. Задаётся
        // Program-ом из окружения, а не аргументом: дескриптор — не протокол.
        internal static long DialogParentValue;

        // Штатный исход: человек отменил скан в окне драйвера — не сбой.
        internal sealed class ScanCancelledException : InvalidOperationException
        {
        }

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
            Log.Info("Захват: WIA (родное окно драйвера)");

            CaptureResult result = await RunCapture(source, directory);

            if (result.Pages.Count > 0)
                return result.Pages;

            if (result.Failure == ScanExchange.CancelledInDialog)
                throw new ScanCancelledException();

            if (result.Failure != null)
                throw new InvalidOperationException(result.Failure);

            throw new InvalidOperationException(
                "Окно драйвера закрылось без страницы: сканирование в нём отменено. " +
                "Нажмите «Сканировать» ещё раз и выполните скан в окне драйвера.");
        }

        private sealed class CatalogSource
        {
            public string Name;
            public ScanDevice Device;
            public WiaApiVersion WiaVersion;
        }

        private sealed class CaptureResult
        {
            public readonly List<string> Pages = new List<string>();

            public string Failure;

            // true — тихий драйвер, отсчёт тишины не сбросился ни разу.
            public bool Abandoned;
        }

        // Варианты WIA-API идут по цепочке: следующий опрашивается, только
        // если предыдущий ничего не нашёл. Большинство машин отвечает на первом.
        private static async Task<List<CatalogSource>> BuildCatalog(int neededIndex)
        {
            var sources = new List<CatalogSource>();
            var controller = new ScanController(new ScanningContext(new GdiImageContext()) { Logger = Naps2Logging.Instance });

            foreach (WiaApiVersion version in new[]
                { WiaApiVersion.Default, WiaApiVersion.Wia10, WiaApiVersion.Wia20 })
            {
                var options = new ScanOptions
                {
                    Driver = Driver.Wia,
                    WiaOptions = { WiaApiVersion = version }
                };

                await CollectDevices(sources, controller, options);

                if (neededIndex >= 0 && neededIndex < sources.Count)
                    return sources;

                if (sources.Count > 0)
                    break;
            }

            Log.Info("Каталог собран: источников {0}", sources.Count);

            return sources;
        }

        private static async Task CollectDevices(
            List<CatalogSource> sources, ScanController controller, ScanOptions options)
        {
            string label = "WIA (" + options.WiaOptions.WiaApiVersion + ")";

            try
            {
                Log.Info("Опрос {0}...", label);

                List<ScanDevice> devices = await ListDevices(controller, options);

                foreach (ScanDevice device in devices.Where(device => sources.All(known => known.Name != device.Name)))
                {
                    sources.Add(new CatalogSource
                    {
                        Name = device.Name,
                        Device = device,
                        WiaVersion = options.WiaOptions.WiaApiVersion
                    });
                }

                Log.Info("Список {0}: устройств {1}", label, devices.Count);
            }
            catch (Exception ex)
            {
                Log.Warn("Список {0} не получен: {1}", label, ex.Message);
            }
        }

        private static async Task<List<ScanDevice>> ListDevices(ScanController controller, ScanOptions options)
        {
            var listing = controller.GetDeviceList(options);
            Task done = await Task.WhenAny(listing, Task.Delay(ListingWait));

            if (done != listing)
                throw new TimeoutException("Перечисление не ответило за 10 с");

            return await listing;
        }

        private static async Task<CaptureResult> RunCapture(CatalogSource source, string directory)
        {
            var result = new CaptureResult();
            var context = new ScanningContext(new GdiImageContext()) { Logger = Naps2Logging.Instance };
            var controller = new ScanController(context);

            Task consume = ConsumePages(source, directory, controller, result);

            // Стенка: если страницы продолжают приходить, отсчёт идёт заново.
            int lastCount = 0;

            while (true)
            {
                Task done = await Task.WhenAny(consume, Task.Delay(QuietWall));

                if (done == consume)
                    break;

                if (result.Pages.Count != lastCount)
                {
                    lastCount = result.Pages.Count;
                    continue;
                }

                result.Abandoned = true;
                result.Failure ??= string.Format(
                    CultureInfo.CurrentCulture,
                    "Захват молчал больше {0} с: драйвер не отвечает.",
                    (int)QuietWall.TotalSeconds);

                break;
            }

            return result;
        }

        private static async Task ConsumePages(
            CatalogSource source, string directory, ScanController controller, CaptureResult result)
        {
            using (var quiet = new CancellationTokenSource())
            {
                // CancelAfter перезапускается целиком: одна метка отсчитывает
                // тишину и до первой страницы, и между страницами.
                quiet.CancelAfter(QuietWall);

                try
                {
                    var options = new ScanOptions
                    {
                        Device = source.Device,
                        PaperSource = PaperSource.Auto,
                        PageSize = PageSize.A4,
                        UseNativeUI = true,
                        DialogParent = new IntPtr(DialogParentValue),
                        WiaOptions = new WiaOptions { WiaApiVersion = source.WiaVersion }
                    };

                    await foreach (var page in controller.Scan(options, quiet.Token))
                    {
                        // Отброшенная попытка — проснувшиеся страницы не нужны.
                        if (result.Abandoned)
                            break;

                        string path = Path.Combine(
                            directory, "scan" + (result.Pages.Count + 1).ToString("D3") + ".bmp");

                        using (page)
                            page.Save(path);

                        result.Pages.Add(path);
                        Log.Info("Страница {0}: {1}", result.Pages.Count, path);

                        quiet.CancelAfter(QuietWall);
                    }
                }
                catch (OperationCanceledException)
                {
                    Log.Warn("Захват прерван по тишине, страниц {0}", result.Pages.Count);

                    if (result.Pages.Count == 0)
                        result.Failure = string.Format(
                            CultureInfo.CurrentCulture,
                            "За {0} мин в окне драйвера страница не была просканирована.",
                            (int)QuietWall.TotalMinutes);
                }
                catch (Exception ex)
                {
                    result.Failure = FailureText(ex, result.Pages.Count > 0);

                    // Отмена человеком (и закрытие окна) рвёт передачу — не сбой.
                    if (ex is FileNotFoundException)
                        Log.Info("Отменено человеком в окне драйвера");
                    else
                        Log.Error("Захват упал: {0}", ex);
                }
            }
        }

        private static string FailureText(Exception error, bool hasPages)
        {
            if (error is FileNotFoundException)
                return ScanExchange.CancelledInDialog;

            // Драйвер уже показал свою ошибку сам — приложение не дублирует её.
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
