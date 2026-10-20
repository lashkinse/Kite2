using System;
using System.Runtime.InteropServices;

namespace Kite2.Scan
{
    // Окно диалога TWAIN-драйвера (ShowUI) живёт в нашем процессе, но хозяин
    // у него — скрытое служебное окно насоса сообщений. Такое окно нередко
    // остаётся позади окна программы, и человек не видит, что скан начался.
    // Поднимаем все видимые окна процесса наверх: Z-порядок меняется без
    // перехвата фокуса, драйвер при этом не трогается вовсе.
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

        private static readonly IntPtr TopWindow = new IntPtr(0);

        private const uint NoSize = 0x0001;
        private const uint NoMove = 0x0002;
        private const uint ShowWindow = 0x0040;

        internal static void RaiseOwnedWindows(int processId)
        {
            EnumWindows((window, state) =>
            {
                GetWindowThreadProcessId(window, out uint owner);

                if (owner == (uint)processId
                    && IsWindowVisible(window)
                    && GetWindowTextLength(window) > 0)
                {
                    SetWindowPos(window, TopWindow, 0, 0, 0, 0, NoSize | NoMove | ShowWindow);
                }

                return true;
            }, IntPtr.Zero);
        }
    }
}
