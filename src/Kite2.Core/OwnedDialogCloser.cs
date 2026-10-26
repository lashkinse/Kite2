using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Kite2.Core
{
    // Диалог WIA-драйвера живёт в чужом процессе (прокси по битности), и смерть
    // помощника-клиента такое окно не гасит. При отмене сдирается полный отсек
    // видимых окон: класс, заголовок, процесс, владелец. Ровно свои окна
    // закрываются штатным WM_CLOSE; отсек записывается в журнал целиком,
    // потому что владение у драйверов бывает всякое, и править нужно по факту.
    internal static class OwnedDialogCloser
    {
        private delegate bool EnumProc(IntPtr window, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumProc callback, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr window, uint command);

        [DllImport("user32.dll")]
        private static extern IntPtr GetAncestor(IntPtr window, uint command);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr window);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr window);

        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        private static extern uint WindowThreadAndProcess(IntPtr window, out uint processId);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr window);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr window, StringBuilder title, int length);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr window, StringBuilder className, int length);

        [DllImport("user32.dll", EntryPoint = "PostMessageW")]
        private static extern bool PostMessage(IntPtr window, uint message, IntPtr wParameter, IntPtr lParameter);

        private const uint CommandGetOwner = 0x0004;   // GW_OWNER
        private const uint CommandRootOwner = 0x0003;  // GA_ROOTOWNER
        private const uint CloseMessage = 0x0010;      // WM_CLOSE

        private const int ReportLimit = 25;

        // ownerWindow — главное окно программы; helperPid — процесс помощника,
        // чьи окна тоже «свои» для закрытия. Возвращает отчёт для журнала.
        public static string CloseFor(IntPtr ownerWindow, int helperPid)
        {
            if (ownerWindow == IntPtr.Zero)
                return "у программы нет окна-владельца";

            var report = new List<string>();

            EnumWindows((window, _) =>
            {
                if (report.Count > ReportLimit || !IsWindowVisible(window))
                    return true;

                IntPtr owner = GetWindow(window, CommandGetOwner);
                IntPtr rootOwner = GetAncestor(window, CommandRootOwner);
                WindowThreadAndProcess(window, out uint processId);

                var title = new StringBuilder(64);
                if (GetWindowTextLength(window) > 0)
                    GetWindowText(window, title, 64);
                var className = new StringBuilder(64);
                GetClassName(window, className, 64);

                string entry = string.Format(
                    "0x{0:X} [{1}] \"{2}\" pid={3} владелец=0x{4:X}({5}) корень=0x{6:X}",
                    window.ToInt64(), className, title, processId,
                    GetOwnerHandle(owner), OwnerDescription(owner, helperPid),
                    GetOwnerHandle(rootOwner));

                bool forOwner = owner == ownerWindow || rootOwner == ownerWindow;

                // Окну-владельцу и нашей программе не касаемся вирально: только
                // чужие диалоги в владельческой цепочке главного окна.
                if (window != ownerWindow && forOwner)
                {
                    PostMessage(window, CloseMessage, IntPtr.Zero, IntPtr.Zero);
                    report.Add("ЗАКРЫТО " + entry);
                    return true;
                }

                report.Add("пропущено " + entry);

                return true;
            }, IntPtr.Zero);

            return string.Join(" | ", report);
        }

        private static string GetOwnerHandle(IntPtr owner) => owner.ToInt64().ToString("X");

        private static string OwnerDescription(IntPtr owner, int helperPid)
        {
            if (owner == IntPtr.Zero)
                return "нет";

            if (!IsWindow(owner))
                return "мёртв";

            WindowThreadAndProcess(owner, out uint processId);
            return "pid=" + processId + (processId == (uint)helperPid ? " помощник" : null);
        }
    }
}
