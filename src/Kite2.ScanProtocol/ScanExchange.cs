using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kite2.ScanProtocol
{
    // Обмен идёт файлом, а не консолью: кириллица в путях через консоль
    // перекодировку кодовых страниц не пережила бы.
    //
    // Формат ответа — строки «ключ=значение»: status=ok|failed, error=… (текст
    // для человека), item=… (источник или путь страницы, по строке на штуку).
    public static class ScanExchange
    {
        // Штатная отмена человеком в окне драйвера: не сбой, ошибки не показывает.
        public const string CancelledInDialog = "Сканирование отменено в окне драйвера.";
        public static void Write(string path, bool succeeded, string error, IEnumerable<string> items)
        {
            var lines = new List<string> { "status=" + (succeeded ? "ok" : "failed") };

            if (!string.IsNullOrEmpty(error))
                lines.Add("error=" + error);

            if (items != null)
                foreach (string item in items)
                    lines.Add("item=" + item);

            File.WriteAllLines(path, lines, new UTF8Encoding(false));
        }

        // Ответ без строки status или с неизвестным ключом читается как неудача:
        // помощник мог быть убит посередине, и тогда доверять написанному нельзя.
        public static ScanResult Read(string path)
        {
            var items = new List<string>();

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
                }
            }

            return new ScanResult(succeeded, error, items);
        }
    }

    public sealed class ScanResult
    {
        public ScanResult(bool succeeded, string error, IReadOnlyList<string> items)
        {
            Succeeded = succeeded;
            Error = error;
            Items = items;
        }

        public bool Succeeded { get; }

        public string Error { get; }

        public IReadOnlyList<string> Items { get; }
    }
}
