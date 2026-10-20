using System;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Kite2.Scan
{
    // Журнал составляется кодом: помощнику нечего знать о конфигурационных файлах,
    // а путь общий с программой (%AppData%\Kite2\logs). Файл открывается на каждую
    // запись: после зависшего драйвера всё записанное обязано оказаться на диске.
    internal static class HelperLogging
    {
        public static void Initialize()
        {
            try
            {
                var file = new FileTarget("helper")
                {
                    FileName = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "Kite2",
                        "logs",
                        "Kite2.Scan-" + DateTime.Now.ToString("yyyyMMdd") + ".log"),
                    Layout = "${longdate}|${level:uppercase=true}|${message} ${exception:format=Message}",
                    ArchiveAboveSize = 2 * 1024 * 1024,
                    MaxArchiveFiles = 5,
                    KeepFileOpen = false
                };

                var configuration = new LoggingConfiguration();
                configuration.AddTarget(file);

                // NTwain-мост и мост NAPS2 пишут самыми нижними уровнями, и без
                // отдельных правил их (debug-)строки утекли бы мимо общей границы
                // Info. Final=true: правило заканчивает выбор — иначе общее
                // правило записало бы те же Info/Fatal строки повторно.
                configuration.LoggingRules.Add(
                    new LoggingRule("NTwain", LogLevel.Debug, LogLevel.Fatal, file) { Final = true });
                configuration.LoggingRules.Add(
                    new LoggingRule("NAPS2", LogLevel.Debug, LogLevel.Fatal, file) { Final = true });
                configuration.LoggingRules.Add(
                    new LoggingRule("*", LogLevel.Info, LogLevel.Fatal, file));

                LogManager.Configuration = configuration;
            }
            catch (Exception)
            {
                // Журнал — вспомогательное средство: без него захват обязан работать.
            }
        }
    }
}
