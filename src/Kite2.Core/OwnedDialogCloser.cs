using System;
using System.Runtime.InteropServices;

namespace Kite2.Core
{
    // Диалог WIA-драйвера живёт в чужом процессе (прокси по битности), а его
    // владелец — окно программы. Смерть помощника-клиента чужой диалог не
    // гасит, но перечень диалогов программы известен точно: это окна, чей
    // владелец — главное окно. Такое окно получает штатное WM_CLOSE,
    // действующее как «Отмена» в самом диалоге.
    internal static class OwnedDialogCloser
    {
        private delegate bool EnumProc(IntPtr window, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumProc callback, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr window, uint command);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr window);

        [DllImport("user32.dll", EntryPoint = "PostMessageW")]
        private static extern bool PostMessage(IntPtr window, uint message, IntPtr wParameter, IntPtr lParameter);

        private const uint CommandGetOwner = 0x0004; // GW_OWNER
        private const uint CloseMessage = 0x0010;    // WM_CLOSE

        public static void Close(IntPtr ownerWindow)
        {
            if (ownerWindow == IntPtr.Zero)
                return;

            EnumWindows((window, _) =>
            {
                // ТОЛЬКО наши диалоги: владелец совпал, так что чужие окна
                // обоих процессов не касаются.
                if (GetWindow(window, CommandGetOwner) == ownerWindow && IsWindowVisible(window))
                    PostMessage(window, CloseMessage, IntPtr.Zero, IntPtr.Zero);

                return true;
            }, IntPtr.Zero);
        }
    }
}
