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

        // Окно диагностики открыто между XferReady (физический проход стекла)
        // и началом сворачивания захвата. Первичные сбои интересны только
        // внутри окна: до него NTwain бросает безвредные исключения при опросе
        // возможностей драйвера, и именно они съедали лимит записей, не давая
        // заметить глотателя, молчащего в момент передачи.
        public static volatile bool TransferWindow;

        // Сколько сбоев записано в текущем окне: лимит считается заново на
        // каждом XferReady, так что длинная сессия перегрузки не глушила
        // диагностику следующей попытки.
        public static int LoggedInWindow;

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
            // разбор текста: «Got TWAIN msg XferReady» фиксирует физический проход
            // и открывает диагностическое окно. Сворачивание захвата окно закрывает:
            // после ForceStepDown источник Disabled, CloseDSReq и DisableSource
            // идут уже по чистой линии разматывания.
            private static void Sniff(string message)
            {
                if (message == null)
                    return;

                if (message.IndexOf("Got TWAIN msg XferReady", StringComparison.Ordinal) >= 0)
                {
                    NTwainLogging.SawTransferReady = true;
                    NTwainLogging.TransferWindow = true;
                    NTwainLogging.LoggedInWindow = 0;
                    return;
                }

                if (NTwainLogging.TransferWindow &&
                    (message.IndexOf("ForceStepDown", StringComparison.Ordinal) >= 0 ||
                     message.IndexOf("Got TWAIN msg CloseDSReq", StringComparison.Ordinal) >= 0 ||
                     message.IndexOf("DisableSource", StringComparison.Ordinal) >= 0))
                {
                    NTwainLogging.TransferWindow = false;
                }
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
