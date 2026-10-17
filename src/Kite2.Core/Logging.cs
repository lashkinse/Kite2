using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Kite2.Core
{
    /// <summary>
    /// Настройка журнала работы программы.
    /// </summary>
    /// <remarks>
    /// Конфигурация задаётся кодом, а не файлом NLog.config: путь к журналу должен быть
    /// известен из одного места и показываться пользователю вместе с текстом ошибки.
    /// Файл открывается на каждую запись — держать его открытым нельзя: через буфер
    /// последние строки не переживают аварийное завершение процесса.
    /// </remarks>
    public static class Logging
    {
        /// <summary>Максимальный размер одного файла журнала, 2 МБ.</summary>
        private const long MaxFileSize = 2 * 1024 * 1024;

        /// <summary>Сколько предыдущих файлов хранить вместе с текущим.</summary>
        private const int MaxArchiveFiles = 5;

        /// <summary>Имя журнала для служебных записей о самой программе.</summary>
        private const string ProgramLogger = "KITE2";

        /// <summary>Файл журнала текущего запуска, или null, если он недоступен.</summary>
        public static string LogFile { get; private set; }

        /// <summary>Ключи запуска, с которыми работала программа: полезно при разборе аварий.</summary>
        public static string StartupArguments { get; private set; } = string.Empty;

        /// <summary>
        /// Готовит журнал. Вызывается самым первым, до всякой другой работы программы:
        /// с этого момента любая ошибка попадает в файл.
        /// </summary>
        public static void Initialize(string[] arguments)
        {
            StartupArguments = arguments == null || arguments.Length == 0
                ? "(без ключей)"
                : string.Join(" ", arguments);

            try
            {
                LogFile = Path.Combine(AppPaths.Logs, "Kite2-" + DateTime.Now.ToString("yyyyMMdd") + ".log");

                var configuration = new LoggingConfiguration();

                var file = new FileTarget("journal")
                {
                    FileName = LogFile,
                    Layout = "${longdate}|${level:uppercase=true}|${logger}|${message} ${exception:format=ToString}",
                    ArchiveAboveSize = MaxFileSize,
                    MaxArchiveFiles = MaxArchiveFiles,
                    KeepFileOpen = false,
                    ConcurrentWrites = false
                };

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

        /// <summary>
        /// Записывает сбой и сразу отправляет журнал на диск: если процесс падает,
        /// записанное должно успеть оказаться в файле.
        /// </summary>
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

        /// <summary>
        /// Подписывает журнал на сбои фоновых потоков. Без этого падение в потоке
        /// выглядит как молча пропавший процесс.
        /// </summary>
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