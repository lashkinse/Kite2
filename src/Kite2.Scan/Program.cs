using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Kite2.ScanProtocol;
using Kite2.Scan.Twain;
using NLog;

namespace Kite2.Scan
{
    // Запускается программой, не человеком:
    //   Kite2.Scan.exe sources <файл ответа>
    //   Kite2.Scan.exe acquire <индекс> <dpi> <цвет> <каталог> <файл ответа>
    // Консоли нет — иначе при каждом захвате мелькало бы чёрное окно.
    internal static class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // STA обязан остаться: и TWAIN, и WIA общаются с процессом COM-сообщениями,
        // которые без STA не доставляются.
        [STAThread]
        private static async Task<int> Main(string[] args)
        {
            HelperLogging.Initialize();

            Log.Info("Помощник, версия {0}", typeof(Program).Assembly.GetName().Version);

            // Тихие сбои фоновых вызовов TWAIN больше нигде не видны: зависания
            // и падения без этих записей в логе не объяснить.
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                Log.Fatal("Необработанный сбой: {0}", e.ExceptionObject);

            RegisterFirstChanceDiagnostics();

            TaskScheduler.UnobservedTaskException += (_, e) =>
                Log.Warn("Утерянный сбой фоновой задачи: {0}", e.Exception);

            NTwainLogging.Attach();

            if (args.Length < 2)
            {
                Log.Error("Не хватает аргументов: получено {0}", args.Length);
                return 2;
            }

            string responsePath = args[args.Length - 1];

            int code;

            try
            {
                if (args[0] == "sources")
                {
                    Log.Info("Команда sources");
                    List<string> sources = await CaptureEngine.GetSources();
                    Log.Info("Список получен: источников {0}", sources.Count);
                    code = Answer(responsePath, sources);
                }
                else if (args[0] == "threaddump")
                {
                    // Скрытая команда: живой снимок стеков потоков. Нужна для
                    // проверки диагностики на машине без сканера.
                    Log.Info("Команда threaddump");
                    ThreadDumpDiag.Dump("ручной вызов");
                    code = Answer(responsePath, new List<string>());
                }
                else if (args[0] == "stackdump" && args.Length == 4)
                {
                    // Скрытая команда: снимок стеков ПОСТОРОННЕГО процесса
                    // (пассивное чтение памяти). Ребёнок с вмороженным нативным
                    // вызовом не может снять себя — его стеки снимает сосед.
                    Log.Info("Команда stackdump: цель {0}", args[1]);
                    ThreadDumpDiag.DumpForeign(Number(args[1]), args[2]);
                    code = Answer(responsePath, new List<string> { args[2] });
                }
                else if (args[0] == "acquire" && args.Length == 6)
                {
                    Log.Info("Команда acquire: источник={0}, dpi={1}, цвет={2}, каталог={3}",
                        args[1], args[2], args[3], args[4]);

                    List<string> pages = await CaptureEngine.Capture(
                        Number(args[1]),
                        Number(args[2]),
                        Number(args[3]),
                        args[4]);

                    Log.Info("Захват завершён, страниц {0}", pages.Count);
                    code = Answer(responsePath, pages);
                }
                else if (args[0] == "twain" && args.Length == 12)
                {
                    Log.Info(
                        "Команда twain (дочерний процесс): диспетчер={0}, режим={1}, dpi={2}, окно={3}, имя={4}, клиент={5}",
                        args[1], args[2], args[3], args[8], args[7], args[9]);

                    WatchParent(Number(args[10]));

                    int client = Number(args[9]);

                    if (client == 1 || client == 2 || client == 3)
                    {
                        // Kyocera: захват ведёт наш собственный клиент TWAIN.
                        // 1 — тихо полной последовательностью, 2 — с родным окном
                        // драйвера, 3 — тихо и без DAT_PENDINGXFERS (этот звонок
                        // на драйвере срывает процесс, а картинка приходит и так).
                        bool withWindow = client == 2;
                        bool skipPending = client == 3;

                        Log.Info(
                            "Команда twain: свой клиент TWAIN, {0}{1}",
                            withWindow ? "с родным окном драйвера" : "в тихом режиме",
                            skipPending ? ", без счётчика передач" : string.Empty);

                        string page = KyoceraTwainScanner.Capture(
                            args[6],
                            args[7],
                            withWindow ? TimeSpan.FromMinutes(6) : TimeSpan.FromSeconds(90),
                            withWindow,
                            skipPending);

                        if (page != null)
                        {
                            Log.Info("Захват готов, страниц {0}", KyoceraTwainScanner.Pages.Count);
                            code = Answer(responsePath, KyoceraTwainScanner.Pages);
                        }
                        else
                        {
                            Log.Warn("Захват пуст");

                            ScanExchange.Write(
                                responsePath,
                                false,
                                withWindow
                                    ? "Драйвер Kyocera не отдал страницу: в его окне нужно нажать «Скан»."
                                    : "Драйвер Kyocera не сообщил о готовности сканирования.",
                                null,
                                null);
                            code = 1;
                        }
                    }
                    else
                    {
                        var chapter = await CaptureEngine.TryTwainChild(
                            Number(args[1]),
                            Number(args[2]),
                            Number(args[3]),
                            Number(args[4]),
                            args[6],
                            args[7],
                            Number(args[5]),
                            args[8] == "1");

                        if (chapter.Pages.Count > 0)
                        {
                            Log.Info("Дочерний захват готов, страниц {0}", chapter.Pages.Count);
                            code = Answer(responsePath, chapter.Pages);
                        }
                        else
                        {
                            Log.Warn("Дочерний захват пуст: {0}", chapter.Failure);

                            // Ответ и на неудачу: иначе родитель решит, что файла нет.
                            // Сигнал «данные были готовы» говорит родителю, что стекло
                            // прошло физически: повторять для него TWAIN-попытки нельзя.
                            ScanExchange.Write(
                                responsePath,
                                false,
                                chapter.Failure ?? "Сканер не отдал ни одной страницы.",
                                null,
                                NTwainLogging.SawTransferReady ? new[] { "xfer" } : null);
                            code = 1;
                        }
                    }
                }
                else
                {
                    Log.Error("Неизвестная команда: {0}, аргументов {1}", args[0], args.Length);
                    code = 2;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Захват не удался: {0}", ex);

                // Ответ пишется всегда, даже при ошибке: программа ждёт файл и иначе
                // решила бы, что помощника нет вовсе. Сигнал нужен и здесь: сбой мог
                // случиться после готовности данных, и родителю полезно о ней знать.
                ScanExchange.Write(
                    responsePath,
                    false,
                    Describe(ex),
                    null,
                    NTwainLogging.SawTransferReady ? new[] { "xfer" } : null);

                code = 1;
            }

            LogManager.Flush();

            // Ответ к этому моменту уже записан; NTwain держит поток насоса
            // сообщений не фоновым, и обычный конец Main такой процесс не закрывает.
            Environment.Exit(code);

            return code;
        }

        // Тексты ошибок сканера уже сказаны по-русски внутри захвата.
        private static string Describe(Exception error) => error.Message;

        // В момент передачи NAPS2 прячет исключения: насос сообщений Win32
        // (DefaultTwainHandleManager → Win32MessagePump) глотает их без лога —
        // у той помпы Logger всегда NullLogger. Поэтому после XferReady
        // Kyocera TWAIN может «умереть» в транзите и мы видим только
        // тишину. FirstChanceException срабатывает ДО любых catch-глотателей:
        // пишет тип, сообщение и стек того, что было выброшено внутри NTwain.
        private static void RegisterFirstChanceDiagnostics()
        {
            AppDomain.CurrentDomain.FirstChanceException += (_, e) =>
            {
                // Сам обработчик обязан быть пустым по эффектам: любой сбой
                // внутри него циклится и роняет процесс.
                try
                {
                    // Ловим только момент передачи: до XferReady драйвер при
                    // опросе возможностей невинно роняет исключения в любом
                    // запросе, и они съедали лимит до точки смерти.
                    // а окно передачи открывает sniff строки XferReady.
                    if (!NTwainLogging.TransferWindow ||
                        NTwainLogging.LoggedInWindow >= 12 ||
                        e.Exception is OperationCanceledException)
                    {
                        return;
                    }

                    string stack = e.Exception.StackTrace ?? string.Empty;
                    if (!stack.Contains("NTwain") && !stack.Contains("NAPS2")
                        && !(e.Exception.GetType().FullName ?? string.Empty).Contains("NTwain"))
                    {
                        return;
                    }

                    Interlocked.Increment(ref NTwainLogging.LoggedInWindow);
                    Log.Debug(
                        "Первичный сбой (пойманный молча): {0}: {1}{2}   в   {3}",
                        e.Exception.GetType().Name,
                        e.Exception.Message,
                        Environment.NewLine,
                        stack);
                }
                catch
                {
                    // Тишина: диагностика не должна ломать сам захват.
                }
            };
        }

        // Иначе родитель умирает, а ребёнок остаётся с включённым источником:
        // Kyocera продолжала физически сканировать стекло ещё долго после
        // «Отмены» («повторное сканирование» на втором компьютере). Оба выхода
        // из осмотра — исключение переиспользованного PID и мёртвый родитель —
        // обязаны закончиться самоубийством процесса.
        private static void WatchParent(int parentPid)
        {
            var watcher = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(500);

                    bool alive = false;

                    try
                    {
                        // PID мог переиспуститься чужим процессом: имя обязательно.
                        Process parent = Process.GetProcessById(parentPid);
                        alive = parent.ProcessName == "Kite2.Scan" && !parent.HasExited;
                    }
                    catch (Exception)
                    {
                        // Родитель мёртв: GetProcessById не справляется.
                    }

                    if (alive)
                        continue;

                    Log.Warn("Родитель умер: дочерний процесс завершается сам");

                    // Драйвер может держать главный поток намертво: убиваем
                    // сам процесс, а не выходим через Exit.
                    Process.GetCurrentProcess().Kill();
                    return;
                }
            })
            {
                IsBackground = true
            };

            watcher.Start();
        }

        private static int Answer(string responsePath, List<string> items)
        {
            ScanExchange.Write(responsePath, true, null, items);

            return 0;
        }

        private static int Number(string text) =>
            int.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture);
    }
}
