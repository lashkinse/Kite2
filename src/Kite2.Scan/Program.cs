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

            if (args.Length < 2)
            {
                Log.Error("Не хватает аргументов: получено {0}", args.Length);
                return 2;
            }

            string responsePath = args[args.Length - 1];

            try
            {
                if (args[0] == "sources")
                {
                    Log.Info("Команда sources");
                    List<string> sources = await CaptureEngine.GetSources();
                    Log.Info("Список получен: источников {0}", sources.Count);
                    return Answer(responsePath, sources);
                }

                if (args[0] == "acquire" && args.Length == 6)
                {
                    Log.Info("Команда acquire: источник={0}, dpi={1}, цвет={2}, каталог={3}",
                        args[1], args[2], args[3], args[4]);

                    List<string> pages = await CaptureEngine.Capture(
                        Number(args[1]),
                        Number(args[2]),
                        Number(args[3]),
                        args[4]);

                    Log.Info("Захват завершён, страниц {0}", pages.Count);
                    return Answer(responsePath, pages);
                }

                Log.Error("Неизвестная команда: {0}, аргументов {1}", args[0], args.Length);
            }
            catch (Exception ex)
            {
                Log.Error("Захват не удался: {0}", ex);

                // Ответ пишется всегда, даже при ошибке: программа ждёт файл и иначе
                // решила бы, что помощника нет вовсе.
                ScanExchange.Write(responsePath, false, Describe(ex), null);

                return 1;
            }

            return 2;
        }

        // Тексты об ошибках сканера уже сказаны по-русски внутри захвата; здесь
        // остаётся fall-through для неожиданных сбоев самого помощника.
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
