using System.IO;
using System.Linq;
using Kite2.ScanProtocol;

namespace Kite2.Tests
{
    public static class ScanExchangeTests
    {
        [Test("Ответ помощника читается обратно без потерь")]
        public static void RoundTrip()
        {
            string path = Path.Combine(Path.GetTempPath(), "kite2-exchange.txt");

            // Кириллица и знак «=» в пути: консоль такой ответ не сохранила бы,
            // а в файле всё это обязано пережить.
            var sources = new[] { "МФУ HP LaserJet MFP", "МФУ Brother DCP-L2540D", "C:\\путь=со знаком\\имя" };

            ScanExchange.Write(path, true, null, sources);

            ScanResult result = ScanExchange.Read(path);

            Check.True(result.Succeeded, "ответ успешный");
            Check.Equal(sources.Length, result.Items.Count, "число источников");
            Check.True(sources.SequenceEqual(result.Items), "порядок и содержимое источников");
        }

        [Test("Причина неудачи доезжает в том виде, в каком её показали")]
        public static void FailureKeepsText()
        {
            string path = Path.Combine(Path.GetTempPath(), "kite2-exchange.txt");

            ScanExchange.Write(path, false, "Драйвер сканера не ответил: It worked!", null);

            ScanResult result = ScanExchange.Read(path);

            Check.False(result.Succeeded, "ответ неуспешный");
            Check.Equal("Драйвер сканера не ответил: It worked!", result.Error, "текст ошибки");
            Check.Equal(0, result.Items.Count, "источников нет");
        }

        [Test("Ответ без строки status читается как неудача")]
        public static void WithoutStatusIsFailure()
        {
            // Так выглядит ответ, если помощника убили посреди работы.
            string path = Path.Combine(Path.GetTempPath(), "kite2-exchange.txt");

            File.WriteAllText(path, "item=недописанная страница");

            Check.False(ScanExchange.Read(path).Succeeded, "ответ без status — неудача");
        }
    }
}
