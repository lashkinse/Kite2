using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Kite2.ScanProtocol;
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

            // Тихие сбои фоновых вызовов TWAIN больше нигде не видны: зависания
            // и падения без этих записей в логе не объяснить.
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                Log.Fatal("Необработанный сбой: {0}", e.ExceptionObject);

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
                else if (args[0] == "twain" && args.Length == 11)
                {
                    Log.Info(
                        "Команда twain (дочерний процесс): диспетчер={0}, режим={1}, dpi={2}, окно={3}, имя={4}",
                        args[1], args[2], args[3], args[8], args[7]);

                    WatchParent(Number(args[9]));

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
