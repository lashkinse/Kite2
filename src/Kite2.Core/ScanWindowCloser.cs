using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Kite2.Core
{
    // Окно WIA-драйвера нередко живёт в чужом процессе (прокси WIA
    // по битности), и смерть помощника сканирования такое окно не гасит.
    // Помощник записывает названия поднятых окон рядом с файлом ответа;
    // при отмене программа закрывает окна точно по этим названиям,
    // а если записей нет — любые новые окна с «WIA» в названии.
    internal static class ScanWindowCloser
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

        [DllImport("user32.dll", EntryPoint = "PostMessageW")]
        private static extern bool PostMessage(IntPtr window, uint message, IntPtr wParameter, IntPtr lParameter);

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

        // Просьба закрыть, а не удар: диалог WIA отвечает на сообщение так
        // же, как на кнопку «Отмена». Возвращает названия закрытых окон.
        public static IReadOnlyList<string> CloseCaptured(HashSet<IntPtr> before, string raisedTitlesFile)
        {
            var recorded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                if (File.Exists(raisedTitlesFile))
                {
                    foreach (string line in File.ReadAllLines(raisedTitlesFile))
                        if (!string.IsNullOrWhiteSpace(line))
                            recorded.Add(line.Trim());
                }
            }
            catch
            {
                // Без записей остаётся узкий резерв по слову «WIA».
            }

            bool knownTitles = recorded.Count > 0;
            var closed = new List<string>();

            EnumWindows((window, _) =>
            {
                if (before.Contains(window)
                    || !IsWindowVisible(window)
                    || GetWindowTextLength(window) == 0)
                    return true;

                var text = new StringBuilder(128);
                GetWindowText(window, text, 128);
                string title = text.ToString();

                bool target = knownTitles
                    ? recorded.Contains(title)
                    : title.IndexOf("wia", StringComparison.OrdinalIgnoreCase) >= 0;

                if (target)
                {
                    PostMessage(window, CloseMessage, IntPtr.Zero, IntPtr.Zero);
                    closed.Add(title);
                }

                return true;
            }, IntPtr.Zero);

            return closed;
        }
    }
}
