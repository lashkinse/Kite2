using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Kite2.Core
{
    // Окно WIA-драйвера может жить в чужом процессе (прокси WIA по битности),
    // и смерть помощника сканирования такое окно не гасит. Поэтому до запуска
    // помощника снимается срез видимых окон, а при отмене программа просит
    // закрыться всё, что появилось после, — это и есть окно драйвера.
    internal static class ScanWindowCloser
    {
        private delegate bool EnumProc(IntPtr window, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumProc callback, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr window);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr window);

        [DllImport("user32.dll")]
        private static extern bool PostMessageW(IntPtr window, uint message, IntPtr wParameter, IntPtr lParameter);

        private const uint CloseMessage = 0x0010; // WM_CLOSE

        // Срез перед запуском помощника: всё после него — окна захвата.
        public static HashSet<IntPtr> VisibleWindows()
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

        // Просьба закрыть, а не удар: диалог WIA отвечает на сообщение
        // так же, как на кнопку «Отмена».
        public static void CloseNew(HashSet<IntPtr> before)
        {
            EnumWindows((window, _) =>
            {
                if (!before.Contains(window)
                    && IsWindowVisible(window)
                    && GetWindowTextLength(window) > 0)
                {
                    PostMessageW(window, CloseMessage, IntPtr.Zero, IntPtr.Zero);
                }

                return true;
            }, IntPtr.Zero);
        }
    }
}
