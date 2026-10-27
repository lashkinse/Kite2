using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Kite2.Core;
using NLog;

namespace Kite2.Scan
{
    // Помощник занят модальным диалогом, поэтому отмена из программы доходит
    // файлом-сигналом и исполняется фоновым потоком: он закрывает окна сессии
    // сканера штатным WM_CLOSE («Отмена» для диалога драйвера).
    //
    // Окно сессии — то, что не существовало до захвата и чей процесс создан
    // во время захвата (хост UI драйвера), либо окно с новым корневым
    // владельцем. Всплывашки давно открытых программ не попадают.
    internal static class AbortWatcher
    {
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

        // Процесс, владеющий переданным окном: наблюдатель не трогает
        // окна самой программы.
        internal static uint ProcessOfWindow(IntPtr window)
        {
            User32.WindowThreadAndProcess(window, out uint processId);

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

            User32.EnumWindows((window, _) =>
            {
                if (User32.IsWindowVisible(window) && User32.GetWindowTextLength(window) > 0)
                    windows.Add(window);

                return true;
            }, IntPtr.Zero);

            return windows;
        }

        private static void CloseSessionWindows()
        {
            uint ownPid = (uint)Process.GetCurrentProcess().Id;
            var closed = new List<string>();

            User32.EnumWindows((window, _) =>
            {
                if (!User32.IsWindowVisible(window) || User32.GetWindowTextLength(window) == 0)
                    return true;

                if (existed.Contains(window))
                    return true;

                User32.WindowThreadAndProcess(window, out uint processId);

                if (processId == ownPid || processId == appPid)
                    return true;

                if (!IsSessionWindow(window, processId))
                    return true;

                User32.Close(window);

                closed.Add("0x" + window.ToInt64().ToString("X") + " ["
                    + User32.ClassOf(window) + "] " + User32.TitleOf(window));

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

            // Либо корневой владелец — новое окно сессии: у диалога драйвера
            // это окно-хост того же процесса.
            IntPtr rootOwner = User32.GetAncestor(window, User32.CommandRootOwner);

            return rootOwner != IntPtr.Zero
                && rootOwner != window
                && User32.IsWindow(rootOwner)
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
