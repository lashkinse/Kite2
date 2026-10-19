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
    // Всё общение со сканером живёт здесь, в отдельном 32-разрядном процессе.
    // Движок — NAPS2.Sdk: один API закрывает TWAIN (обоих диспетчеров) и WIA,
    // разбор страниц при любом механизме передачи сделан внутри библиотеки и
    // обкатан на сотнях моделей — повторять это руками ради отдельного драйвера
    // больше незачем. Прерывания изнутри всё равно нет: зависший драйвер
    // завершается вместе с процессом, когда человек нажмёт «Отмена».
    internal static class CaptureEngine
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // До первой страницы пауза тишины короче: разогрев и сетевые ожидания
        // укладываются в неё почти всегда, а лишнюю минуту тишины пользователь
        // увидел бы пустым ожиданием.
        private static readonly TimeSpan FirstPageQuiet = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan BetweenPagesQuiet = TimeSpan.FromSeconds(60);

        // Перечислениям каталога свой потолок: зависший вызов TWAIN (с этим
        // столкнулись живьём на старом диспетчере) не утащил бы за собой и
        // «sources», и подготовку попыток «acquire».
        private static readonly TimeSpan CatalogWait = TimeSpan.FromSeconds(10);

        // Устройство каталога: источник адресуется не именем, а индексом, поэтому
        // состав обязан воспроизводиться одинаково в команде «sources» и в «acquire».
        // Device хранится полностью: NAPS2 требует в ScanDevice.ID точный
        // внутренний идентификатор, и имя для этого не годится.
        private sealed class CatalogEntry
        {
            public string Name;
            public ScanDevice Device;

            // WIA-источник обязан сканироваться через ту же версию API, через
            // которую его нашли: идентификаторы устройства в WIA 1.0 и WIA 2.0
            // друг другу не соответствуют.
            public WiaApiVersion WiaVersion;
        }

        // Один способ добраться до сканера. Их у TWAIN-источника несколько:
        // если первый не дал ни страницы, серия следующего не считается странной.
        private sealed class ScanAttempt
        {
            public Driver Driver;

            // Диспетчер TWAIN: «новый» — TWAIN 2, «старый» — легендарный twain_32.
            // У старых драйверов передача через TWAIN 2 молчит намертво, поэтому
            // дешёвого «одного пути для всех» не существует.
            public TwainDsm Dsm;

            public TwainTransferMode Mode;

            public string Title;
        }

        public static async Task<List<string>> GetSources()
        {
            var entries = await BuildCatalog();

            return entries.Select(entry => entry.Name).ToList();
        }

        public static async Task<List<string>> Capture(int sourceIndex, int dpi, int colour, string directory)
        {
            var entries = await BuildCatalog();

            if (sourceIndex < 0 || sourceIndex >= entries.Count)
            {
                throw new InvalidOperationException(
                    "Выбранный источник не найден. Обновите список источников и повторите попытку.");
            }

            var entry = entries[sourceIndex];
            Log.Info("Источник {0}: {1} ({2})", sourceIndex, entry.Name, entry.Device.Driver);

            string lastProblem = null;
            var pages = new List<string>();

            foreach (var attempt in AttemptsFor(entry))
            {
                Log.Info("Попытка: {0}", attempt.Title);

                var result = await RunAttempt(entry, attempt, dpi, colour, directory, pages.Count);

                if (result.Problem != null)
                    Log.Warn("Попытка не сработала: {0}", result.Problem);

                if (result.Pages.Count > 0)
                {
                    // Страницы важнее идеальной серии: даже если попытка кончилась
                    // отказом после нескольких страниц, серия неполная, но не пустая,
                    // и приложение продолжит её обычным вопросом.
                    pages.AddRange(result.Pages);
                    return pages;
                }

                lastProblem = result.Problem;
            }

            throw new InvalidOperationException(
                lastProblem ?? "Сканер не отдал ни одной страницы.");
        }

        // Каталог собирается в жёстком порядке: TWAIN по обоим диспетчерам, затем WIA.
        // Повторы по имени выбрасываются, и первое появление имени
        // остаётся на своём месте — индексы между запусками не едут.
        private static async Task<List<CatalogEntry>> BuildCatalog()
        {
            var controller = NewController();
            var entries = new List<CatalogEntry>();

            await AddTwain(entries, controller, TwainDsm.New);
            await AddTwain(entries, controller, TwainDsm.Old);
            await AddWia(entries, controller, WiaApiVersion.Default);
            await AddWia(entries, controller, WiaApiVersion.Wia10);
            await AddWia(entries, controller, WiaApiVersion.Wia20);

            Log.Info("Каталог собран: источников {0}", entries.Count);

            return entries;
        }

        // Запрос списка с потолком времени: зависший TWAIN-вызов не должен
        // останавливать сбор каталога — недобежавший список просто попадёт в Warn.
        private static async Task<List<ScanDevice>> Listed(ScanController controller, ScanOptions options)
        {
            var listing = controller.GetDeviceList(options);

            // Отменить сам вызов изнутри нельзя (он сидит в нативном коде), поэтому
            // ограждение — таймер: не ответившая задача остаётся в фоне, процесс
            // всё равно одноразовый и закроется сразу после ответа.
            var done = await Task.WhenAny(listing, Task.Delay(CatalogWait));

            if (done != listing)
                throw new TimeoutException("Перечисление не ответило за 10 с");

            return await listing;
        }

        private static async Task AddTwain(
            List<CatalogEntry> entries, ScanController controller, TwainDsm dsm)
        {
            var options = new ScanOptions
            {
                Driver = Driver.Twain,
                TwainOptions = { Dsm = dsm }
            };

            try
            {
                Log.Info("Опрос TWAIN ({0})...", dsm);

                var devices = await Listed(controller, options);

                foreach (var device in devices)
                {
                    if (entries.All(existing => existing.Name != device.Name))
                    {
                        entries.Add(new CatalogEntry
                        {
                            Name = device.Name,
                            Device = device
                        });
                    }
                }

                Log.Info("Список TWAIN ({0}): устройств {1}", dsm, devices.Count);
            }
            catch (Exception ex)
            {
                // Диспетчера может не быть вовсе: отсутствие списков — не сбой,
                // пока хоть один способ добраться до сканера существует.
                Log.Warn("Список TWAIN ({0}) не получен: {1}", dsm, ex.Message);
            }
        }

        private static async Task AddWia(
            List<CatalogEntry> entries, ScanController controller, WiaApiVersion version)
        {
            var options = new ScanOptions
            {
                Driver = Driver.Wia,
                WiaOptions = { WiaApiVersion = version }
            };

            try
            {
                Log.Info("Опрос WIA ({0})...", version);

                var devices = await Listed(controller, options);

                foreach (var device in devices)
                {
                    if (entries.All(existing => existing.Name != device.Name))
                    {
                        entries.Add(new CatalogEntry
                        {
                            Name = device.Name,
                            Device = device,
                            WiaVersion = version
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Список WIA ({0}) не получен: {1}", version, ex.Message);
            }
        }

        // Порядок попыток — не перестановка ради красоты: старый диспетчер с
        // передачей памятью — это рецепт, который заработал с молчащим драйвером
        // Kyocera; новые диспетчер и передача целиком идут следом как самый
        // обычный путь современных драйверов.
        private static List<ScanAttempt> AttemptsFor(CatalogEntry entry)
        {
            var attempts = new List<ScanAttempt>();

            if (entry.Device.Driver == Driver.Twain)
            {
                attempts.Add(new ScanAttempt
                {
                    Driver = Driver.Twain,
                    Dsm = TwainDsm.Old,
                    Mode = TwainTransferMode.Memory,
                    Title = "TWAIN (старый диспетчер, передача памятью)"
                });
                attempts.Add(new ScanAttempt
                {
                    Driver = Driver.Twain,
                    Dsm = TwainDsm.New,
                    Mode = TwainTransferMode.Memory,
                    Title = "TWAIN (новый диспетчер, передача памятью)"
                });
                attempts.Add(new ScanAttempt
                {
                    Driver = Driver.Twain,
                    Dsm = TwainDsm.New,
                    Mode = TwainTransferMode.Native,
                    Title = "TWAIN (новый диспетчер, передача целиком)"
                });
                attempts.Add(new ScanAttempt
                {
                    Driver = Driver.Twain,
                    Dsm = TwainDsm.Old,
                    Mode = TwainTransferMode.Native,
                    Title = "TWAIN (старый диспетчер, передача целиком)"
                });
            }
            else if (entry.Device.Driver == Driver.Wia)
            {
                attempts.Add(new ScanAttempt
                {
                    Driver = Driver.Wia,
                    Title = "WIA"
                });
            }

            return attempts;
        }

        private sealed class AttemptResult
        {
            public readonly List<string> Pages = new List<string>();

            // Текст для человека; null — попытка тихо закончилась без страниц
            // (например, пустой лоток, когда страницы уже были).
            public string Problem;
        }

        private static async Task<AttemptResult> RunAttempt(
            CatalogEntry entry, ScanAttempt attempt, int dpi, int colour, string directory, int pagesBefore)
        {
            var result = new AttemptResult();

            // «CancelAfter» перезапускает внутренний таймер целиком, поэтому
            // одна и та же метка служит и до первой страницы, и между страницами:
            // каждое появление страницы сбрасывает отсчёт тишины заново.
            using (var quiet = new CancellationTokenSource())
            {
                quiet.CancelAfter(FirstPageQuiet);

                try
                {
                    var controller = NewController();
                    var options = new ScanOptions
                    {
                        Driver = attempt.Driver,
                        // NAPS2 сканирует только конкретное устройство: пустой Device
                        // валидатор отвергает ещё до обращения к сканеру.
                        Device = entry.Device,

                        // Решение о податчике отдаём драйверу: он сам знает,
                        // есть ли у него лоток, и молча падает на объявлении
                        // несуществующего источника бумаги.
                        PaperSource = PaperSource.Auto,
                        Dpi = dpi,
                        BitDepth = colour == ScanExchange.BlackAndWhite
                            ? BitDepth.BlackAndWhite
                            : BitDepth.Color,

                        // Рамка скана задаётся всегда — «дефолта драйвера» в NAPS2 нет,
                        // без явного размера вписывался бы Letter и обрезал низ А4.
                        PageSize = PageSize.A4,

                        TwainOptions = { Dsm = attempt.Dsm, TransferMode = attempt.Mode }
                    };

                    await foreach (var page in controller.Scan(options, quiet.Token))
                    {
                        string path = Path.Combine(
                            directory,
                            "scan" + (pagesBefore + result.Pages.Count + 1).ToString("D3") + ".bmp");

                        using (page)
                            page.Save(path);

                        result.Pages.Add(path);
                        Log.Info("Страница {0}: {1} ({2})", pagesBefore + result.Pages.Count, path, attempt.Title);

                        quiet.CancelAfter(BetweenPagesQuiet);
                    }
                }
                catch (OperationCanceledException)
                {
                    Log.Warn("Попытка прервана по тишине, страниц {0}", result.Pages.Count);

                    if (result.Pages.Count == 0)
                    {
                        var pause = (int)FirstPageQuiet.TotalSeconds;
                        result.Problem = string.Format(
                            CultureInfo.CurrentCulture,
                            "Сканер молчит уже {0} с и не отдал ни одной страницы. " +
                            "Проверьте, включён ли он и заправлена ли бумага в лоток.",
                            pause);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Попытка упала: {0}", ex);
                    result.Problem = Describe(ex, result.Pages.Count > 0);
                }

                return result;
            }
        }

        private static ScanController NewController() =>
            new ScanController(new ScanningContext(new GdiImageContext()));

        private static string Describe(Exception error, bool hasPages)
        {
            // Драйвер уже сообщил человеку об ошибке сам (например, Kyocera на пустом
            // лотке приходит как OperationError → AlreadyHandled) — NAPS2 в этом месте
            // молчит. Молчим и мы: сверху не сработавший попытке останется общий текст
            // «Сканер не отдал ни одной страницы.».
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

            // У DeviceCommunicationException и DeviceException сообщений может не быть
            // вовсе — NAPS2 делает их без текста, ориентируясь на свой справочник:
            // поэтому вместо error.Message здесь общий понятный текст.
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
