using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Diagnostics.Runtime;
using NLog;

namespace Kite2.Scan
{
    // Снимок стеков потоков помощника в момент тишины передачи. После XferReady
    // драйвер собирает страницу, помпа сообщений NAPS2 свободна, а ни NTwain,
    // ни NAPS2 ничего не логируют: где висит обработка XferReady — не видно.
    // Снимок покажет, где застрял поток помпы (внутри вызова triplet'а, в
    // ожидании на объекте или в чём-то ещё), и заменит догадки точным местом.
    internal static class ThreadDumpDiag
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // Один раскрытый стек на поток: глубокиего буфер в стеке бьёт по читаемости
        // журнала, а интересующий вызов всегда в первых кадрах.
        private const int MaxFramesPerThread = 40;

        public static void Dump(string reason)
        {
            try
            {
                Log.Info("==== Снимок потоков: {0} ====", reason);

                var text = new StringBuilder();

                // Снимок живого собственного процесса: PssCreateSnapshot замораживает
                // момент, а анализ идёт по копии — сам захват при этом не замирает.
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

                        // Последнее брошенное исключение потока: его наличие при
                        // молчащей передаче ответит, что уже поглотили до тишины.
                        if (thread.CurrentException != null)
                            text.AppendLine("   брошено: " + thread.CurrentException);
                    }
                }

                Log.Info(text.ToString());
                Log.Info("==== Снимок потоков окончен ====");
            }
            catch (Exception ex)
            {
                // Диагностика не должна ломать сам захват: любой её сбой только
                // журналируем и продолжаем.
                Log.Warn(ex, "Снимок потоков не удался");
            }
        }
    }
}
