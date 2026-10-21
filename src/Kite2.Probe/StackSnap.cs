// Внешний снимок стеков живого процесса (как в Kite2.Scan): отладчик не
// подключается, процесс лишь замораживается на несколько секунд, приложение
// пасивное. Снимаем ребёнка, который сидит в висящем нативном вызове —
// на снимке видно, в каком DAT_… останавливается цепочка.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Diagnostics.Runtime;

namespace Kite2.Probe
{
    internal static class StackSnap
    {
        private const int MaxFramesPerThread = 40;

        public static void Foreign(int targetPid, string outPath)
        {
            try
            {
                using (DataTarget target = DataTarget.CreateSnapshotAndAttach(targetPid))
                using (ClrRuntime runtime = target.ClrVersions.First().CreateRuntime())
                {
                    File.WriteAllText(outPath, StackText(runtime), new UTF8Encoding(false));
                }
            }
            catch (Exception ex)
            {
                try
                {
                    File.WriteAllText(outPath, "снимок стеков не удался: " + ex.Message, new UTF8Encoding(false));
                }
                catch
                {
                    // Ничего не сделать: процесс мог уйти к моменту снимка.
                }
            }
        }

        private static string StackText(ClrRuntime runtime)
        {
            var text = new StringBuilder();

            foreach (ClrThread thread in runtime.Threads)
            {
                if (!thread.IsAlive)
                    continue;

                text.Append("Поток ").Append(thread.OSThreadId)
                    .Append(" (управляемый ").Append(thread.ManagedThreadId).AppendLine("):");

                int frames = 0;
                foreach (ClrStackFrame frame in thread.EnumerateStackTrace())
                {
                    text.AppendLine("   " + frame);
                    if (++frames >= MaxFramesPerThread)
                    {
                        text.AppendLine("   …(дальше обрезано)");
                        break;
                    }
                }

                if (frames == 0)
                    text.AppendLine("   (управляемых кадров нет)");
            }

            return text.ToString();
        }
    }
}
