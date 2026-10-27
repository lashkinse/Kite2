using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Kite2.ScanProtocol;
using NLog;

namespace Kite2.Core
{
    // WIA-драйверы части старых сканеров живут по x86, а приложение x64,
    // поэтому всю работу со сканером ведёт помощник Kite2.Scan.exe, живущий
    // рядом с программой. Обмен идёт файлом ответа, отмена — файлом-сигналом.
    internal static class ScanHelperClient
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const string HelperName = "Kite2.Scan.exe";

        private const string ScanParentEnvironmentKey = "KITE2_SCAN_PARENT";

        // Владелец диалога помощника берётся у этой программы, а мини-право
        // вывода на передний план — передаётся помощнику явно.
        [DllImport("user32.dll")]
        private static extern bool AllowSetForegroundWindow(uint processId);

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
            // Ни dpi, ни цвета: их задаёт человек в окне драйвера, помощнику
            // нужен только источник и каталог готовых страниц.
            string arguments = string.Format(
                CultureInfo.InvariantCulture,
                "acquire {0} \"{1}\"",
                request.SourceIndex,
                request.Directory);

            ScanResult result = Run(arguments, cancellationToken);

            if (result == null)
                return new string[0];

            if (!result.Succeeded)
                throw new InvalidOperationException(result.Error);

            return result.Items.ToArray();
        }

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

                // Окно программы передаётся помощнику как владелец будущего
                // диалога WIA-драйвера: дескриптор окна — не часть протокола,
                // поэтому идёт переменной окружения, а не аргументом.
                IntPtr mainWindow = Process.GetCurrentProcess().MainWindowHandle;

                if (mainWindow != IntPtr.Zero)
                    startInfo.EnvironmentVariables[ScanParentEnvironmentKey] =
                        mainWindow.ToInt64().ToString(CultureInfo.InvariantCulture);

                using (var helper = Process.Start(startInfo))
                {
                    Log.Info("Помощник запущен: {0}", arguments);

                    // Право выводить окно на передний план есть только у окна
                    // переднего плана; помощнику оно передаётся явно.
                    AllowSetForegroundWindow((uint)helper.Id);

                    using (cancellationToken.Register(() => StopHelper(helper, mainWindow, responsePath)))
                    {
                        helper.WaitForExit();
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        Log.Info("Захват отменён по команде пользователя");
                        return null;
                    }

                    Log.Info("Помощник завершился, код {0}", helper.ExitCode);

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
                // Файлы, не успевшие исчезнуть штатно, убираются как мусор.
                File.Delete(responsePath + ".abort");
                File.Delete(responsePath);
            }
        }

        // Отмена: помощнику уходит файл-сигнал — его фоновый поток сам закроет
        // окна сессии сканера (хост UI драйвера живёт в чужом процессе, смерти
        // помощника этого не отменяет). Если за 2,5 с помощник не вышел —
        // окно-владелец достаётся штатное закрытие, затем помощник добивается.
        private static void StopHelper(Process helper, IntPtr ownerWindow, string responsePath)
        {
            try
            {
                File.WriteAllText(responsePath + ".abort", "cancel");

                var clock = Stopwatch.StartNew();

                if (helper.WaitForExit(2500))
                {
                    Log.Info("Отмена принята, помощник вышел за {0:N1} с", clock.Elapsed.TotalSeconds);
                    return;
                }

                Log.Info("Окна драйвера при отмене: {0}", OwnedDialogCloser.CloseFor(ownerWindow, helper.Id));

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
