using System;
using Microsoft.Extensions.Logging;
using NLog;
using MsLogLevel = Microsoft.Extensions.Logging.LogLevel;
using MsLogging = Microsoft.Extensions.Logging.ILogger;

namespace Kite2.Scan
{
    // Мост в ILogger NAPS2: внутренние дневники NAPS2.TW без него не видны нигде,
    // а именно там лежит причина молчаливой передачи.
    internal sealed class Naps2Logging : MsLogging
    {
        public static readonly Naps2Logging Instance = new Naps2Logging();

        private readonly Logger _log = LogManager.GetLogger("NAPS2");

        public bool IsEnabled(MsLogLevel logLevel) => logLevel >= MsLogLevel.Debug;

        IDisposable MsLogging.BeginScope<TState>(TState state) => null;

        public void Log<TState>(
            MsLogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            string message = formatter(state, exception);

            switch (logLevel)
            {
                case MsLogLevel.Debug:
                    _log.Debug("NAPS2: {0}", message);
                    break;
                case MsLogLevel.Information:
                    _log.Info("NAPS2: {0}", message);
                    break;
                case MsLogLevel.Warning:
                    _log.Warn(exception, "NAPS2: {0}", message);
                    break;
                case MsLogLevel.Error:
                    _log.Error(exception, "NAPS2: {0}", message);
                    break;
                case MsLogLevel.Critical:
                    _log.Fatal(exception, "NAPS2: {0}", message);
                    break;
            }
        }
    }
}
