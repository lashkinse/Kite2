using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Diagnostics.Runtime;
using NLog;

namespace Kite2.Scan
{
    // Диагностика тишины передачи: после XferReady драйвер не отдаёт страницу.
    // Основной путь безопасный — состояние потоков берётся из Process.Threads
    // (время процессора и режим ожидания), это не трогает сам процесс. Полный
    // снимок стеков живого процесса (ClrMD) включается только переменной
    // окружения: в процессе с висящим нативным вызовом драйвера такой снимок
    // убивал помощника (вылет «не передав результат»).
    internal static class ThreadDumpDiag
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // Полный снимок стеков разрешён только при KITE2_STACK_DIAG=1.
        public static bool FullSnapshotEnabled =>
            Environment.GetEnvironmentVariable("KITE2_STACK_DIAG") == "1";

        // Один раскрытый стек на поток: глубокий стек бьёт по читаемости
        // журнала, а интересующий вызов всегда в первых кадрах.
        private const int MaxFramesPerThread = 40;

        public static void Dump(string reason)
        {
            DumpThreads(reason);

            if (FullSnapshotEnabled)
                DumpStacks(reason);
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

        private static void DumpStacks(string reason)
        {
            try
            {
                Log.Info("==== Снимок стеков: {0} ====", reason);

                var text = new StringBuilder();

                using (DataTarget target = DataTarget.CreateSnapshotAndAttach(Process.GetCurrentProcess().Id))
                using (ClrRuntime runtime = target.ClrVersions.First().CreateRuntime())
                {
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
                }

                Log.Info(text.ToString());
                Log.Info("==== Снимок стеков окончен ====");
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Снимок стеков не удался");
            }
        }
    }
}
