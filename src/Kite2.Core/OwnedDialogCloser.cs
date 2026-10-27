using System;
using System.Collections.Generic;
using System.Text;
using Kite2.Core;

namespace Kite2.Core
{
    // Диалог WIA-драйвера живёт в чужом процессе-хосте, и смерть помощника
    // такое окно не гасит. Закрываются только чужие диалоги в владельческой
    // цепочке главного окна; в журнал пишется весь осмотр окон, потому что
    // владение у драйверов бывает всякое — править нужно по факту.
    internal static class OwnedDialogCloser
    {
        private const int ReportLimit = 25;

        // ownerWindow — главное окно программы; helperPid — процесс помощника,
        // чьи окна тоже «свои». Возвращает отчёт для журнала.
        public static string CloseFor(IntPtr ownerWindow, int helperPid)
        {
            if (ownerWindow == IntPtr.Zero)
                return "у программы нет окна-владельца";

            var report = new List<string>();

            User32.EnumWindows((window, _) =>
            {
                if (report.Count > ReportLimit || !User32.IsWindowVisible(window))
                    return true;

                IntPtr owner = User32.GetWindow(window, User32.CommandGetOwner);
                IntPtr rootOwner = User32.GetAncestor(window, User32.CommandRootOwner);
                User32.WindowThreadAndProcess(window, out uint processId);

                string entry = string.Format(
                    "0x{0:X} [{1}] \"{2}\" pid={3} владелец=0x{4:X}({5}) корень=0x{6:X}",
                    window.ToInt64(), User32.ClassOf(window), User32.TitleOf(window), processId,
                    OwnerHandle(owner), OwnerDescription(owner, helperPid),
                    OwnerHandle(rootOwner));

                bool forOwner = owner == ownerWindow || rootOwner == ownerWindow;

                if (window != ownerWindow && forOwner)
                {
                    User32.Close(window);
                    report.Add("ЗАКРЫТО " + entry);
                    return true;
                }

                report.Add("пропущено " + entry);

                return true;
            }, IntPtr.Zero);

            return string.Join(" | ", report);
        }

        private static string OwnerHandle(IntPtr owner) => owner.ToInt64().ToString("X");

        private static string OwnerDescription(IntPtr owner, int helperPid)
        {
            if (owner == IntPtr.Zero)
                return "нет";

            if (!User32.IsWindow(owner))
                return "мёртв";

            User32.WindowThreadAndProcess(owner, out uint processId);

            return "pid=" + processId + (processId == (uint)helperPid ? " помощник" : null);
        }
    }
}
