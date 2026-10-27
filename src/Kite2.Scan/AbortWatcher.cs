using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using NLog;

namespace Kite2.Scan
{
    // Помощник занят модальным диалогом, поэтому отмена из программы доходит
    // файлом-сигналом и исполняется фоновым потоком: он закрывает окна сессии
    // сканера штатным WM_CLOSE («Отмена» для диалога драйвера).
    //
    // Окно сессии — то, что не существовало до захвата, и его процесс создан
    // уже во время захвата (хост UI драйвера), либо его корневой владелец —
    // новое окно сессии. Всплывашки давно открытых программ не попадают:
    // их процессы старые, а их новые окна ничьих окон не владеют.
    internal static class AbortWatcher
    {
        private delegate bool EnumProc(IntPtr window, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumProc callback, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr window);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr window);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr window);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr window, StringBuilder title, int length);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr window, StringBuilder className, int length);

        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        internal static extern uint WindowAndProcess(IntPtr window, out uint processId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetAncestor(IntPtr window, uint command);

        [DllImport("user32.dll", EntryPoint = "PostMessageW")]
        private static extern bool PostMessage(IntPtr window, uint message, IntPtr wParameter, IntPtr lParameter);

        private const uint CommandRootOwner = 0x0003; // GA_ROOTOWNER
        private const uint CloseMessage = 0x0010;     // WM_CLOSE

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static DateTime sessionStart;
        private static HashSet<IntPtr> existed;
        private static uint appPid;

        internal static void Start(string abortFilePath, uint appProcessId)
        {
            appPid = appProcessId;
            existed = VisibleWindows();
            sessionStart = DateTime.Now;

            var watcher = new Thread(() => Watch(abortFilePath)) { IsBackground = true };
            watcher.Start();
        }

        // Идентификатор процесса, владеющего окном: приложение передаёт своё
        // окно-владельца, чтобы помощник не трогал окна программы.
        internal static uint ProcessOfWindow(IntPtr window)
        {
            WindowAndProcess(window, out uint processId);
            return processId;
        }

        private static void Watch(string abortFilePath)
        {
            try
            {
                while (!File.Exists(abortFilePath))
                    Thread.Sleep(500);

                CloseSessionWindows();
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Наблюдение отмены прервалось");
            }
        }

        private static HashSet<IntPtr> VisibleWindows()
        {
            var windows = new HashSet<IntPtr>();

            EnumWindows((window, _) =>
            {
                if (IsWindowVisible(window) && GetWindowTextLength(window) > 0)
                    windows.Add(window);

                return true;
            }, IntPtr.Zero);

            return windows;
        }

        private static void CloseSessionWindows()
        {
            uint ownPid = (uint)Process.GetCurrentProcess().Id;
            var closed = new List<string>();

            EnumWindows((window, _) =>
            {
                if (!IsWindowVisible(window) || GetWindowTextLength(window) == 0)
                    return true;

                if (existed.Contains(window))
                    return true;

                WindowAndProcess(window, out uint processId);

                if (processId == ownPid || processId == appPid)
                    return true;

                if (!IsSessionWindow(window, processId))
                    return true;

                PostMessage(window, CloseMessage, IntPtr.Zero, IntPtr.Zero);

                var title = new StringBuilder(64);
                GetWindowText(window, title, 64);
                var className = new StringBuilder(64);
                GetClassName(window, className, 64);

                closed.Add("0x" + window.ToInt64().ToString("X") + " [" + className + "] " + title);

                return true;
            }, IntPtr.Zero);

            Log.Info("Окна сессии при отмене: {0}",
                closed.Count > 0 ? string.Join("; ", closed) : "новых окон сессии не найдено");
        }

        private static bool IsSessionWindow(IntPtr window, uint processId)
        {
            // Процесс создан во время сессии: хост UI драйвера.
            DateTime? started = ProcessStartTime(processId);

            if (started.HasValue && started.Value >= sessionStart)
                return true;

            // Либо его корневой владелец — новое окно сессии: у диалога
            // драйвера это окно-хост того же процесса.
            IntPtr rootOwner = GetAncestor(window, CommandRootOwner);

            return rootOwner != IntPtr.Zero
                && rootOwner != window
                && IsWindow(rootOwner)
                && !existed.Contains(rootOwner);
        }

        private static DateTime? ProcessStartTime(uint processId)
        {
            try
            {
                return Process.GetProcessById((int)processId).StartTime;
            }
            catch
            {
                return null;
            }
        }
    }
}
