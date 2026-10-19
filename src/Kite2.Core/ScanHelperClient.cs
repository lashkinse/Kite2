using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Kite2.ScanProtocol;
using NLog;

namespace Kite2.Core
{
    // TWAIN-драйвер открывается только 32-разрядным процессом, а приложение x64,
    // поэтому всю работу со сканером выполняет Kite2.Scan.exe рядом с программой.
    // Обмен идёт файлом ответа, отмена — закрытием процесса: зависший драйвер
    // не вернётся сам.
    internal static class ScanHelperClient
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const string HelperName = "Kite2.Scan.exe";

        public static IReadOnlyList<string> GetSources()
        {
            ScanResult result = Run("sources", CancellationToken.None);

            if (result == null || !result.Succeeded)
            {
                Log.Warn("Список источников не получен: {0}", Reason(result));

                return new string[0];
            }

            return result.Items;
        }

        public static string[] Capture(ScanRequest request, CancellationToken cancellationToken)
        {
            string arguments = string.Format(
                CultureInfo.InvariantCulture,
                "acquire {0} {1} {2} \"{3}\"",
                request.SourceIndex,
                request.Dpi,
                request.Colour == CaptureColour.BlackAndWhite ? ScanExchange.BlackAndWhite : ScanExchange.Colour,
                request.Directory);

            ScanResult result = Run(arguments, cancellationToken);

            if (result == null)
                return new string[0];

            if (!result.Succeeded)
                throw new InvalidOperationException(result.Error);

            return result.Items.ToArray();        }

        // null означает «захват прерван пользователем»: не читается как неудача.
        private static ScanResult Run(string arguments, CancellationToken cancellationToken)
        {
            string helperPath = Path.Combine(AppPaths.AppDirectory, HelperName);

            if (!File.Exists(helperPath))
            {
                Log.Error("Помощник сканирования не найден: {0}", helperPath);

                throw new InvalidOperationException(Messages.ScanHelperMissing);
            }

            string responsePath = Path.Combine(Path.GetTempPath(), "kite2-scan-" + Guid.NewGuid().ToString("N") + ".txt");

            try
            {
                var startInfo = new ProcessStartInfo(helperPath, arguments + " \"" + responsePath + "\"")
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = AppPaths.AppDirectory
                };

                using (var helper = Process.Start(startInfo))
                {
                    if (helper == null)
                        return Failure(Messages.ScanHelperMissing);

                    var clock = Stopwatch.StartNew();
                    Log.Info("Помощник запущен: {0}", arguments);

                    using (cancellationToken.Register(() => TryKill(helper)))
                    {
                        helper.WaitForExit();
                    }

                    clock.Stop();

                    if (cancellationToken.IsCancellationRequested)
                    {
                        Log.Info("Захват отменён по команде пользователя, помощник работал {0:N1} с", clock.Elapsed.TotalSeconds);
                        return null;
                    }

                    Log.Info("Помощник завершился за {0:N1} с, код {1}", clock.Elapsed.TotalSeconds, helper.ExitCode);

                    if (!File.Exists(responsePath))
                    {
                        Log.Error("Помощник сканирования не ответил, код {0}", helper.ExitCode);

                        return Failure(Messages.ScanHelperSilent);
                    }

                    return ScanExchange.Read(responsePath);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Помощник сканирования не отработал");

                return Failure(ex.Message);
            }
            finally
            {
                if (File.Exists(responsePath))
                    File.Delete(responsePath);
            }
        }

        private static void TryKill(Process helper)
        {
            try
            {
                helper.Kill();
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Помощник сканирования уже завершён");
            }
        }

        private static ScanResult Failure(string error) =>
            new ScanResult(false, error, new string[0]);

        private static string Reason(ScanResult result) =>
            result == null || string.IsNullOrEmpty(result.Error) ? "помощник не ответил" : result.Error;
    }
}
