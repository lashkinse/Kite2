using System;
using System.Runtime.InteropServices;

namespace Kite2.Scan
{
    // Диалог WIA-драйвера (ShowUI) живёт в нашем процессе, но хозяин у него —
    // скрытое служебное окно насоса сообщений, поэтому диалог нередко остаётся
    // позади окна программы. Ставим его topmost: он держится наверху постоянно,
    // не мигает и не отбирает фокус.
    internal static class WindowRaiser
    {
        private delegate bool EnumProc(IntPtr window, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumProc callback, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr window);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr window);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr window, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);

        private const int TopmostFlag = -1;    // HWND_TOPMOST
        private const uint ShowWindow = 0x0040;
        private const uint NoSize = 0x0001;
        private const uint NoMove = 0x0002;
        private const uint NoActivate = 0x0010;

        internal static void RaiseOwnedWindows(int processId)
        {
            EnumWindows((window, state) =>
            {
                GetWindowThreadProcessId(window, out uint owner);

                if (owner == (uint)processId
                    && IsWindowVisible(window)
                    && GetWindowTextLength(window) > 0)
                {
                    SetWindowPos(window, new IntPtr(TopmostFlag), 0, 0, 0, 0,
                        NoSize | NoMove | ShowWindow | NoActivate);
                }

                return true;
            }, IntPtr.Zero);
        }
    }
}
