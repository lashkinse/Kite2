using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Kite2.Core
{
    // Диалог WIA-драйвера живёт в чужом процессе (прокси по битности), и смерть
    // помощника-клиента такое окно не гасит. Перечень «своих» диалогов точен:
    // это окна, чей корень владельцев — главное окно программы (само владение
    // бывает провести не напрямую, а через цепочку: диалог WIA — прогресс).
    // Штатное WM_CLOSE действует как «Отмена» в самом диалоге.
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
        private static extern bool IsWindowVisible(IntPtr window);

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

        // Возвращает краткий отчёт: какие окна закрыты либо почему ни одного.
        public static string CloseFor(IntPtr ownerWindow)
        {
            if (ownerWindow == IntPtr.Zero)
                return "у программы нет окна-владельца";

            var found = new List<string>();

            EnumWindows((window, _) =>
            {
                // Окно-владелец само себе корень: исключается сразу, иначе
                // «чин» закроет главное окно программы.
                if (window == ownerWindow || !IsWindowVisible(window))
                    return true;

                // Прямое владение и владение через цепочку: диалог драйвера
                // может сам владеть окном прогресса.
                IntPtr owner = GetWindow(window, CommandGetOwner);
                IntPtr rootOwner = GetAncestor(window, CommandRootOwner);

                if (owner != ownerWindow && rootOwner != ownerWindow)
                    return true;

                PostMessage(window, CloseMessage, IntPtr.Zero, IntPtr.Zero);

                var title = new StringBuilder(64);
                GetWindowText(window, title, 64);
                var className = new StringBuilder(64);
                GetClassName(window, className, 64);

                found.Add("0x" + window.ToInt64().ToString("X") + " [" + className + "] " + title);

                return true;
            }, IntPtr.Zero);

            return found.Count > 0
                ? "закрыто: " + string.Join("; ", found)
                : "владельческих окон не найдено";
        }
    }
}
