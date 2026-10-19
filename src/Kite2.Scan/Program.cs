using System;
using System.Collections.Generic;
using System.Globalization;
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
                // решила бы, что помощника нет вовсе.
                ScanExchange.Write(responsePath, false, Describe(ex), null);

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

        private static int Answer(string responsePath, List<string> items)
        {
            ScanExchange.Write(responsePath, true, null, items);

            return 0;
        }

        private static int Number(string text) =>
            int.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture);
    }
}
