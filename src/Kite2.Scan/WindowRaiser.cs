using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Kite2.Scan
{
    // Окно WIA-драйвера живёт то в нашем процессе, то в чужом (прокси WIA
    // по битности), поэтому ориентир — появление: срез видимых окон при
    // старте захвата, и всё новое после него поднимается наверх. Первому
    // показу даём фокус: иначе окно чужого процесса мигает на панели
    // задач, ожидая внимания, вместо того чтобы остаться над программой.
    internal static class WindowRaiser
    {
        private delegate bool EnumProc(IntPtr window, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumProc callback, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr window);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr window);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextW(IntPtr window, StringBuilder title, int length);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr window, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr window);

        private const int Topmost = -1; // HWND_TOPMOST
        private const uint ShowWindowFlag = 0x0040;
        private const uint NoSize = 0x0001;
        private const uint NoMove = 0x0002;
        private const uint NoActivate = 0x0010;

        // Видимые сейчас окна с заголовком — срез для поиска новых.
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

        // Наверх без отбирания фокуса: набор в окне программы не сбивается.
        internal static void KeepAboveOthers(IntPtr window) =>
            SetWindowPos(window, new IntPtr(Topmost), 0, 0, 0, 0, NoSize | NoMove | ShowWindowFlag | NoActivate);

        // Первый показ: фокус забирает мигание окна чужого процесса на панели.
        internal static void BringToFront(IntPtr window)
        {
            KeepAboveOthers(window);
            SetForegroundWindow(window);
        }

        internal static string Title(IntPtr window)
        {
            var text = new StringBuilder(128);
            GetWindowTextW(window, text, 128);
            return text.ToString();
        }
    }
}
