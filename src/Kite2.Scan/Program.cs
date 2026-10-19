using System;
using System.Collections.Generic;
using System.Globalization;
using Kite2.ScanProtocol;
using Saraff.Twain;

namespace Kite2.Scan
{
    // Запускается программой, не человеком:
    //   Kite2.Scan.exe sources <файл ответа>
    //   Kite2.Scan.exe acquire <индекс> <dpi> <цвет> <каталог> <файл ответа>
    // Консоли нет — иначе при каждом захвате мелькало бы чёрное окно.
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length < 2)
                return 2;

            string responsePath = args[args.Length - 1];

            try
            {
                if (args[0] == "sources")
                    return Answer(responsePath, TwainCapture.GetSources());

                if (args[0] == "acquire" && args.Length == 6)
                {
                    return Answer(responsePath, TwainCapture.Capture(
                        Number(args[1]),
                        Number(args[2]),
                        Number(args[3]),
                        args[4]));
                }
            }
            catch (Exception ex)
            {
                // Ответ пишется всегда, даже при ошибке: программа ждёт файл и иначе
                // решила бы, что помощника нет вовсе.
                ScanExchange.Write(responsePath, false, Describe(ex), null);

                return 1;
            }

            return 2;
        }

        // Слова драйвера полезны для разбора, но сами по себе ничего не значат:
        // «It worked!» — это ответ DSM, у которого нет ни одного устройства.
        private static string Describe(Exception error) =>
            error is TwainException ? "Драйвер сканера не ответил: " + error.Message : error.Message;

        private static int Answer(string responsePath, List<string> items)
        {
            ScanExchange.Write(responsePath, true, null, items);

            return 0;
        }

        private static int Number(string text) =>
            int.Parse(text, NumberStyles.Integer, CultureInfo.InvariantCulture);
    }
}
