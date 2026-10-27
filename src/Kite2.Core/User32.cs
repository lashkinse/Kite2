using System;
using System.Runtime.InteropServices;
using System.Text;

// Один модуль на обе сборки (линкуется в Core и Scan): EnumWindows и соседи
// встречаются в OwnedDialogCloser и AbortWatcher, и держать два одинаковых
// списка P/Invoke незачем. internal честен в каждой сборке отдельно.
namespace Kite2.Core
{
    internal static class User32
    {
        internal delegate bool EnumProc(IntPtr window, IntPtr lParam);

        private const uint CloseMessage = 0x0010;      // WM_CLOSE

        [DllImport("user32.dll")]
        internal static extern bool EnumWindows(EnumProc callback, IntPtr lParam);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetWindow(IntPtr window, uint command);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetAncestor(IntPtr window, uint command);

        [DllImport("user32.dll")]
        internal static extern bool IsWindow(IntPtr window);

        [DllImport("user32.dll")]
        internal static extern bool IsWindowVisible(IntPtr window);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int GetWindowText(IntPtr window, StringBuilder title, int length);

        [DllImport("user32.dll")]
        internal static extern int GetWindowTextLength(IntPtr window);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int GetClassName(IntPtr window, StringBuilder className, int length);

        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
        internal static extern uint WindowThreadAndProcess(IntPtr window, out uint processId);

        [DllImport("user32.dll", EntryPoint = "PostMessageW")]
        internal static extern bool PostMessage(IntPtr window, uint message, IntPtr wParameter, IntPtr lParameter);

        // Константы владельческих связей окна (GW_OWNER, GA_ROOTOWNER).
        internal const uint CommandGetOwner = 0x0004;
        internal const uint CommandRootOwner = 0x0003;

        internal static string TitleOf(IntPtr window)
        {
            var title = new StringBuilder(64);

            if (GetWindowTextLength(window) > 0)
                GetWindowText(window, title, 64);

            return title.ToString();
        }

        internal static string ClassOf(IntPtr window)
        {
            var className = new StringBuilder(64);
            GetClassName(window, className, 64);

            return className.ToString();
        }

        internal static void Close(IntPtr window) =>
            PostMessage(window, CloseMessage, IntPtr.Zero, IntPtr.Zero);
    }
}
