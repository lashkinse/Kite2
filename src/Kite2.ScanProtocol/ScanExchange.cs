using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kite2.ScanProtocol
{
    // Обмен между программой и помощником захвата идёт файлом, а не консолью:
    // названия источников и пути содержат кириллицу, и через консоль она не
    // пережила бы перекодировку кодовых страниц.
    //
    // Формат ответа — строки «ключ=значение», по ключу на строку:
    //   status=ok | failed   — чем закончилась работа помощника;
    //   error=…               — причина неудачи, по-русски и для показа человеку;
    //   item=…                — источник или путь к готовой странице, по строке на штуку;
    //   signal=…              — запас на будущее: пометка о ходе попытки.
    // Разделителем считается первый знак «=»: в пути к файлу знаки равны встречаются.
    public static class ScanExchange
    {
        // Порядок CaptureColour в программе: помощник о типах программы не знает и
        // должен понимать те же числа.
        public const int BlackAndWhite = 0;

        public const int Colour = 1;

        public static void Write(
            string path, bool succeeded, string error, IEnumerable<string> items,
            IEnumerable<string> signals = null)
        {
            var lines = new List<string>
            {
                "status=" + (succeeded ? "ok" : "failed")
            };

            if (signals != null)
            {
                foreach (string signal in signals)
                    lines.Add("signal=" + signal);
            }

            if (!string.IsNullOrEmpty(error))
                lines.Add("error=" + error);

            if (items != null)
            {
                foreach (string item in items)
                    lines.Add("item=" + item);
            }

            File.WriteAllLines(path, lines, new UTF8Encoding(false));
        }

        // Ответ без строки status или с неизвестным ключом читается как неудача:
        // помощник мог быть убит посередине, и тогда доверять написанному нельзя.
        public static ScanResult Read(string path)
        {
            var items = new List<string>();
            var signals = new List<string>();

            bool succeeded = false;
            string error = null;

            foreach (string line in File.ReadAllLines(path, Encoding.UTF8))
            {
                int split = line.IndexOf('=');

                if (split <= 0)
                    continue;

                string value = line.Substring(split + 1);

                switch (line.Substring(0, split))
                {
                    case "status":
                        succeeded = value == "ok";
                        break;
                    case "error":
                        error = value;
                        break;
                    case "item":
                        items.Add(value);
                        break;
                    case "signal":
                        signals.Add(value);
                        break;
                }
            }

            return new ScanResult(succeeded, error, items, signals);
        }
    }

    public sealed class ScanResult
    {
        public ScanResult(
            bool succeeded, string error, IReadOnlyList<string> items,
            IReadOnlyList<string> signals = null)
        {
            Succeeded = succeeded;
            Error = error;
            Items = items;
            Signals = signals ?? new string[0];
        }

        public bool Succeeded { get; }

        public string Error { get; }

        public IReadOnlyList<string> Items { get; }

        public IReadOnlyList<string> Signals { get; }

        public bool HasSignal(string signal) => Signals.Contains(signal);
    }
}
