using System;
using System.Globalization;
using System.Text;
using NLog;
using NTwain;

namespace Kite2.Scan
{
    // NTwain пишет всю хронику TWAIN-вызовов (открытие диспетчера, коды возврата,
    // выбор источника) через System.Diagnostics.Trace, и без слушателя эти строки
    // никуда не попадают. Из-за этого последние зависания на «Список TWAIN (Old)»
    // остались без деталей: не видно, что в это время говорят сами TWAIN-вызовы.
    // Свой ILog направляет их в общий файл, рядом с логами самого помощника.
    internal static class NTwainLogging
    {
        private sealed class NLogLog : ILog
        {
            private readonly Logger _log = LogManager.GetLogger("NTwain");

            public bool IsInfoEnabled { get; set; } = true;

            public bool IsDebugEnabled { get; set; } = true;

            public bool IsErrorEnabled { get; set; } = true;

            public void Info(string message) => _log.Info("NTwain: {0}", Short(message));

            public void Info(string messageFormat, params object[] args) =>
                Info(string.Format(CultureInfo.InvariantCulture, messageFormat, args));

            public void Debug(string message) => _log.Debug("NTwain: {0}", Short(message));

            public void Debug(string messageFormat, params object[] args) =>
                Debug(string.Format(CultureInfo.InvariantCulture, messageFormat, args));

            public void Error(string message) => _log.Error("NTwain: {0}", Short(message));

            public void Error(string message, Exception exception) =>
                _log.Error(exception, "NTwain: {0}", Short(message));

            public void Error(string messageFormat, Exception exception, params object[] args) =>
                Error(string.Format(CultureInfo.InvariantCulture, messageFormat, args), exception);

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
            // Нет смысла заворачивать в try/catch: если мост не встанет,
            // собирать TWAIN-диагностику всё равно будет нечем.
            PlatformInfo.Current.Log = new NLogLog();
        }
    }
}
