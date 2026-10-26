using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Kite2.ScanProtocol;
using NLog;

namespace Kite2.Scan
{
    // Запускается программой, не человеком:
    //   Kite2.Scan.exe sources <файл ответа>
    //   Kite2.Scan.exe acquire <индекс> <каталог> <файл ответа>
    // Консоли нет — иначе при каждом захвате мелькало бы чёрное окно.
    internal static class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // Падение фонового помощника (например, срыв уборки в драйвере)
        // не должно выкатывать пользователю системный диалог Windows
        // «программа завершилась с ошибкой»: результат докладывается
        // в файле ответа, а окно с ошибкой только пугает и висит поверх
        // всего, пока человек его не закроет — по два таких окна на захват.
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern uint SetErrorMode(uint newMode);

        private const uint SemFailCriticalErrors = 0x0001;
        private const uint SemNoGpFaultErrorBox = 0x0002;

        // STA обязан остаться: WIA общается с процессом COM-сообщениями,
        // которые без STA не доставляются.
        [STAThread]
        private static async Task<int> Main(string[] args)
        {
            HelperLogging.Initialize();

            SetErrorMode(SemFailCriticalErrors | SemNoGpFaultErrorBox);

            Log.Info("Помощник, версия {0}", typeof(Program).Assembly.GetName().Version);

            // Окно программы — владелец диалога драйвера; передаётся
            // переменной окружения, а не аргументом.
            string parent = Environment.GetEnvironmentVariable("KITE2_SCAN_PARENT");

            if (long.TryParse(parent, NumberStyles.Integer, CultureInfo.InvariantCulture, out long handle)
                && handle != 0)
            {
                CaptureEngine.DialogParentValue = handle;
            }

            // Тихие сбои фонового захвата больше нигде не видны: зависания
            // и падения без этих записей в логе не объяснить.
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                Log.Fatal("Необработанный сбой: {0}", e.ExceptionObject);

            TaskScheduler.UnobservedTaskException += (_, e) =>
                Log.Warn("Утерянный сбой фоновой задачи: {0}", e.Exception);

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
                else if (args[0] == "acquire" && args.Length == 4)
                {
                    Log.Info("Команда acquire: источник={0}, каталог={1}", args[1], args[2]);

                    List<string> pages = await CaptureEngine.Capture(
                        Number(args[1]),
                        args[2]);

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
                ScanExchange.Write(
                    responsePath,
                    false,
                    Describe(ex),
                    null);

                code = 1;
            }

            LogManager.Flush();

            // Явный выход: зависшие COM-потоки WIA не должны держать процесс.
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
