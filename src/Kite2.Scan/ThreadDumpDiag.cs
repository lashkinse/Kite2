using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Diagnostics.Runtime;
using NLog;

namespace Kite2.Scan
{
    // Диагностика тишины передачи: после XferReady драйвер не отдаёт страницу.
    // Быстрый путь безопасный: состояние потоков берётся из Process.Threads
    // (время процессора и режим ожидания), сам процесс это не трогает.
    // Стеки снимает соседний процесс (см. DumpForeign): собственный снимок
    // ClrMD в процессе с вмороженным нативным вызовом драйвера убивал
    // помощника (вылет «не передав результат»).
    internal static class ThreadDumpDiag
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // Один раскрытый стек на поток: глубокий стек бьёт по читаемости
        // журнала, а интересующий вызов всегда в первых кадрах.
        private const int MaxFramesPerThread = 40;

        public static void Dump(string reason)
        {
            DumpThreads(reason);
        }

        // Внешний снимок стеков чужого процесса: съёмщик стартует отдельным
        // процессом и прикладывается к ребёнку с вмороженным нативным вызовом.
        // Приложение пассивное — не отладчик, целевой процесс от съёмщика
        // (включая его смерть) не зависит.
        public static void DumpForeign(int targetPid, string outPath)
        {
            try
            {
                using (DataTarget target = DataTarget.CreateSnapshotAndAttach(targetPid))
                using (ClrRuntime runtime = target.ClrVersions.First().CreateRuntime())
                {
                    File.WriteAllText(outPath, StackText(runtime));
                    Log.Info("Снимок стеков процесса {0} записан в {1}", targetPid, outPath);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Снимок стеков процесса {0} не удался", targetPid);

                try
                {
                    File.WriteAllText(outPath, "снимок не удался: " + ex.Message);
                }
                catch
                {
                    // Файл мог не открыться: диагноз уже в журнале.
                }

                throw;
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

                if (thread.CurrentException != null)
                    text.AppendLine("   брошено: " + thread.CurrentException);
            }

            return text.ToString();
        }

        // Безопасный срез: время процессора и режим ожидания каждого потока.
        // Висящий в ядре поток не набирает процессорное время, а жёстко
        // работающий — набирает: по срезам в 45, 90 и 148 секунд виден вектор.
        private static void DumpThreads(string reason)
        {
            try
            {
                Log.Info("==== Срез потоков (безопасный): {0} ====", reason);

                var text = new StringBuilder();

                ProcessThread[] threads = Process.GetCurrentProcess()
                    .Threads.Cast<ProcessThread>().ToArray();

                // Потоки умирают под руками: каждый в своём перехвате.
                foreach (ProcessThread thread in threads)
                {
                    try
                    {
                        bool running = thread.ThreadState == ThreadState.Running;
                        string mode = running
                            ? "работает"
                            : "ожидание: " + thread.WaitReason;

                        text.Append("Поток ").Append(thread.Id)
                            .Append(": cpu ")
                            .Append(thread.TotalProcessorTime.TotalMilliseconds.ToString("N0"))
                            .Append(" мс, ").AppendLine(mode);
                    }
                    catch
                    {
                        // Поток завершился между снапшотом списка и чтением.
                    }
                }

                Log.Info(text.ToString());
                Log.Info("==== Срез окончен ====");
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Срез потоков не удался");
            }
        }
    }
}
