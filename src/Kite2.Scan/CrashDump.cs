using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Kite2.Scan
{
    // Автодамп на падение процесса: краши драйвера (0xC000041D и прочие) гасят
    // ребёнка до того, как что-то успевает записать обратную связь. Фильтр
    // необработанных исключений пишет minidump с потоками и стеками — по нему
    // видно, на каком звонке слоя драйвера (kcTwain → Ns1815 → KMUSBSCN3)
    // процесс умер. Дамп кладётся рядом с журналом: %APPDATA%\Kite2\logs.
    internal static class CrashDump
    {
        // Normal + стабильные потоки/стеки; без памяти целиком — файл лёгкий.
        private const uint DumpType = 0
            | 0x00000001 // MiniDumpWithDataSegs
            | 0x00000004; // MiniDumpWithHandleData

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int FilterDelegate(ref EXCEPTION_POINTERS info);

        [StructLayout(LayoutKind.Sequential)]
        private struct EXCEPTION_POINTERS
        {
            public IntPtr PointerRecord;
            public IntPtr PointerContext;
        }

        [DllImport("dbghelp.dll", SetLastError = true)]
        private static extern bool MiniDumpWriteDump(
            IntPtr process, int processId, IntPtr file,
            uint dumpType, IntPtr exceptionParam,
            IntPtr userStream, IntPtr callerStream);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr SetUnhandledExceptionFilter(FilterDelegate filter);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        private static extern int GetCurrentProcessId();

        private static int ProcessId => GetCurrentProcessId();

        private static int OnException(ref EXCEPTION_POINTERS info)
        {
            try
            {
                string directory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Kite2", "logs");
                Directory.CreateDirectory(directory);

                string file = Path.Combine(
                    directory,
                    "Kite2.Scan.crash-" + ProcessId + ".dmp");

                using (var stream = new FileStream(
                    file, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    MiniDumpWriteDump(
                        GetCurrentProcess(), ProcessId,
                        stream.SafeFileHandle.DangerousGetHandle(), DumpType,
                        info.PointerRecord, IntPtr.Zero, IntPtr.Zero);
                }
            }
            catch
            {
                // Дамп не удался — падение всё равно идёт своим ходом.
            }

            // EXCEPTION_EXECUTE_HANDLER: штатно оборвываем процесс.
            return 1;
        }

        // Фильтр обязан жить в процессе весь срок: держим делегат за хэндлом.
        private static readonly FilterDelegate Held = OnException;

        public static void Install()
        {
            try
            {
                SetUnhandledExceptionFilter(Held);
            }
            catch
            {
                // Без дампа работаем как раньше.
            }
        }
    }
}
