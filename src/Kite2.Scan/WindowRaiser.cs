using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Kite2.Scan
{
    // Окно WIA-драйвера живёт то в нашем процессе, то в чужом (прокси WIA
    // по битности), поэтому по процессу искать нельзя. Ориентир — появление:
    // окна нашей программы поднимаются безусловно, чужие — только когда их
    // название похоже на имя устройства. Первый показ получает фокус через
    // прикрепление потоков: прямую просьбу о переднем плане из фонового
    // процесса Windows отклоняет.
    internal static class WindowRaiser
    {
        private delegate bool EnumProc(IntPtr window, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumProc callback, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr window);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr window);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr window, StringBuilder title, int length);

        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        private static extern uint WindowThread(IntPtr window, IntPtr threadId);

        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        private static extern uint WindowThreadAndProcess(IntPtr window, out uint processId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr window, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr window);

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr window);

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint attachThread, uint attachToThread, bool attach);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        private const int Topmost = -1; // HWND_TOPMOST
        private const uint ShowWindowFlag = 0x0040;
        private const uint NoSize = 0x0001;
        private const uint NoMove = 0x0002;
        private const uint NoActivate = 0x0010;

        // Видимые сейчас окна с заголовком — срез «что было до захвата».
        internal static HashSet<IntPtr> VisibleWindows()
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

        // Наверх без отбирания фокуса: окно держится над программой
        // постоянно, а работа в окне программы не сбивается.
        internal static void KeepAboveOthers(IntPtr window) =>
            SetWindowPos(window, new IntPtr(Topmost), 0, 0, 0, 0, NoSize | NoMove | ShowWindowFlag | NoActivate);

        // Первый показ: окно чужого процесса обязано выйти на передний план
        // само, иначе оно мигает на панели задач в ожидании внимания. Прямая
        // просьба из фонового процесса отклоняется системой, поэтому потоки
        // окна переднего плана и нашего процесса склеиваются ненадолго.
        internal static void BringToFront(IntPtr window)
        {
            KeepAboveOthers(window);

            uint foregroundThread = WindowThread(GetForegroundWindow(), IntPtr.Zero);
            uint ownThread = GetCurrentThreadId();

            bool attached = foregroundThread != 0
                && foregroundThread != ownThread
                && AttachThreadInput(ownThread, foregroundThread, true);

            try
            {
                BringWindowToTop(window);
                SetForegroundWindow(window);
            }
            finally
            {
                if (attached)
                    AttachThreadInput(ownThread, foregroundThread, false);
            }
        }

        internal static uint ProcessOf(IntPtr window)
        {
            WindowThreadAndProcess(window, out uint processId);
            return processId;
        }

        internal static string Title(IntPtr window)
        {
            var text = new StringBuilder(128);
            GetWindowText(window, text, 128);
            return text.ToString();
        }

        // Название чужого окна принимается, только если оно говорит о нашем
        // устройстве: «WIA» и больше половины значимых слов имени устройства,
        // либо имя повторено целиком (от двух значимых слов).
        internal static bool TitleMatchesDevice(string title, string deviceName)
        {
            string cleanTitle = (title ?? "").Trim().ToLowerInvariant();
            string cleanName = (deviceName ?? "").ToLowerInvariant();

            if (cleanTitle.Length == 0 || cleanName.Length == 0)
                return false;

            string[] words = cleanName
                .Split(new[] { ' ', '-', '_', '(', ')' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(word => word.Length >= 3)
                .ToArray();

            if (words.Length == 0)
                return false;

            int matched = words.Count(cleanTitle.Contains);
            bool wia = cleanTitle.Contains("wia");

            return (matched * 2 > words.Length && wia)
                || (matched == words.Length && words.Length >= 2);
        }
    }
}
