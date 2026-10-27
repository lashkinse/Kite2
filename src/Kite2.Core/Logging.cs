using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Kite2.Core
{
    // Конфигурация задаётся кодом: путь к журналу показывается пользователю вместе
    // с текстом ошибки. Файл открывается на каждую запись — закрытый журнал
    // переживает аварийное завершение процесса.
    public static class Logging
    {
        private const long MaxFileSize = 2 * 1024 * 1024;

        private const int MaxArchiveFiles = 5;

        private const string ProgramLogger = "KITE2";

        public static string LogFile { get; private set; }

        public static string StartupArguments { get; private set; } = string.Empty;

        // Вызывается самым первым: с этого момента любая ошибка попадает в файл.
        public static void Initialize(string[] arguments)
        {
            StartupArguments = arguments == null || arguments.Length == 0
                ? "(без ключей)"
                : string.Join(" ", arguments);

            try
            {
                LogFile = Path.Combine(AppPaths.Logs, "Kite2-" + DateTime.Now.ToString("yyyyMMdd") + ".log");

                var file = new FileTarget("journal")
                {
                    FileName = LogFile,
                    Layout = "${longdate}|${level:uppercase=true}|${logger}|${message} ${exception:format=ToString}",
                    ArchiveAboveSize = MaxFileSize,
                    MaxArchiveFiles = MaxArchiveFiles,
                    KeepFileOpen = false,
                    ConcurrentWrites = false
                };

                var configuration = new LoggingConfiguration();
                configuration.AddTarget(file);
                configuration.AddRule(LogLevel.Debug, LogLevel.Fatal, file);

                LogManager.Configuration = configuration;
            }
            catch (Exception exception)
            {
                // Журнал — вспомогательное средство. Если он недоступен, программа
                // обязана продолжать работать, просто без записи.
                LogFile = null;
                Trace.WriteLine("Не удалось настроить журнал: " + exception);
            }
        }

        // Flush обязателен: записанное должно пережить аварийное завершение.
        public static void Report(Exception exception, string what)
        {
            string text = what + Environment.NewLine + exception;

            try
            {
                LogManager.GetLogger(ProgramLogger).Error(text);
                LogManager.Flush();
            }
            catch (Exception)
            {
                // Журнал недоступен — пишем хотя бы в отладку.
                Trace.WriteLine(text);
            }
        }

        public static void WriteStartup(params string[] lines)
        {
            var log = LogManager.GetLogger(ProgramLogger);

            foreach (string line in lines)
                log.Info(line);

            LogManager.Flush();
        }

        // Без подписки падение в фоновом потоке выглядит как молча пропавший процесс.
        public static void CatchBackgroundFailures()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                Report(args.ExceptionObject as Exception, "Незапланированный сбой в фоновом потоке");

            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                Report(args.Exception, "Незамеченный сбой в фоновой задаче");
                args.SetObserved();
            };
        }
    }
}