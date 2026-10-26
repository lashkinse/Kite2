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
    // WIA-драйверы части старых сканеров живут по x86, а приложение x64, поэтому
    // всю работу со сканером выполняет помощник Kite2.Scan.exe — он живёт
    // рядом, в том же каталоге, что и сама программа: оба процесса берут сборки
    // из одного места. Обмен идёт файлом ответа, отмена — закрытием процесса:
    // зависший драйвер не вернётся сам.
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
                    // диалога WIA-драйвера: владельцу окно не может уйти за
                    // окно программы, а панель задач у него не мигает. Дескриптор
                    // окна — не часть протокола сканирования, поэтому идёт
                    // переменной окружения, а не аргументом.
                    IntPtr mainWindow = Process.GetCurrentProcess().MainWindowHandle;

                    if (mainWindow != IntPtr.Zero)
                        startInfo.EnvironmentVariables[ScanParentEnvironmentKey] =
                            mainWindow.ToInt64().ToString(CultureInfo.InvariantCulture);

                    using (var helper = Process.Start(startInfo))
                    {
                        if (helper == null)
                            return Failure(Messages.ScanHelperMissing);

                        // Право выводить окно на передний план есть только у окна
                        // переднего плана; помощнику оно передаётся явно.
                        AllowSetForegroundWindow((uint)helper.Id);

                        var clock = Stopwatch.StartNew();
                        Log.Info("Помощник запущен: {0}", arguments);

                        using (cancellationToken.Register(() => StopHelper(helper, mainWindow)))
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

        // Отмена: диалогу драйвера, живущему в чужом процессе, уходит
        // штатное закрытие по его владельцу — окну программы; помощник
        // добивается следом и не остаётся при висящем диалоге.
        private static void StopHelper(Process helper, IntPtr ownerWindow)
        {
            try
            {
                Log.Info("Окна драйвера при отмене: {0}", OwnedDialogCloser.CloseFor(ownerWindow));

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
