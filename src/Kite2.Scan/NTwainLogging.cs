using System;
using System.Globalization;
using System.Text;
using NLog;
using NTwain;

namespace Kite2.Scan
{
    // NTwain пишет хронику TWAIN-вызовов через System.Diagnostics.Trace, и без
    // слушателя эти строки никуда не попадают. Свой ILog направляет их в общий
    // файл журнала помощника.
    internal static class NTwainLogging
    {
        // Готовность данных означает, что стекло прошло физически и драйвер собрал
        // страницу: это единственное событие, видимое не NAPS2, а только NTwain.
        // Родитель по нему решает, можно ли повторять TWAIN-попытки (сломанным
        // драйвером Kyocera страница сканировалась раз за разом без результата).
        public static bool SawTransferReady;

        private sealed class NLogLog : ILog
        {
            private readonly Logger _log = LogManager.GetLogger("NTwain");

            public bool IsInfoEnabled { get; set; } = true;

            public bool IsDebugEnabled { get; set; } = true;

            public bool IsErrorEnabled { get; set; } = true;

            public void Info(string message)
            {
                Sniff(message);
                _log.Info("NTwain: {0}", Short(message));
            }

            public void Info(string messageFormat, params object[] args) =>
                Info(string.Format(CultureInfo.InvariantCulture, messageFormat, args));

            public void Debug(string message)
            {
                Sniff(message);
                _log.Debug("NTwain: {0}", Short(message));
            }

            public void Debug(string messageFormat, params object[] args) =>
                Debug(string.Format(CultureInfo.InvariantCulture, messageFormat, args));

            public void Error(string message)
            {
                Sniff(message);
                _log.Error("NTwain: {0}", Short(message));
            }

            public void Error(string message, Exception exception) =>
                _log.Error(exception, "NTwain: {0}", Short(message));

            public void Error(string messageFormat, Exception exception, params object[] args) =>
                Error(string.Format(CultureInfo.InvariantCulture, messageFormat, args), exception);

            // Готовность данных — единственная строка NTwain, ради которой нужен
            // разбор текста: «Got TWAIN msg XferReady» фиксирует физический проход.
            private static void Sniff(string message)
            {
                if (message != null &&
                    message.IndexOf("Got TWAIN msg XferReady", StringComparison.Ordinal) >= 0)
                    NTwainLogging.SawTransferReady = true;
            }

            // Драйверы могут прислать строку с нулями внутри: лог-файл должен
            // остаться текстовым файлом, а не испортиться о чужие байты.
            private static string Short(string message)
            {
                if (message == null)
                    return string.Empty;

                var text = new StringBuilder(message.Length);

                foreach (char ch in message)
                {
                    if (ch != 0)
                        text.Append(ch);
                }

                return text.ToString();
            }
        }

        public static void Attach()
        {
            // Если мост не встанет, собирать TWAIN-диагностику всё равно будет нечем.
            PlatformInfo.Current.Log = new NLogLog();
        }
    }
}
