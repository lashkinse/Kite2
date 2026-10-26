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
        // Обычный дамп: стеки всех потоков и модули. Чем меньше флагов, тем
        // меньше шансов, что сама запись сбоит раньше кода исключения.
        private const uint DumpType = 0;

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

        [DllImport("kernel32.dll")]
        private static extern int GetCurrentThreadId();

        private static int ProcessId => GetCurrentProcessId();

        // dbghelp ждёт не указатель на исключение, а упаковку: номер краш-потока,
        // указатель на EXCEPTION_POINTERS и флаг «клиентский указатель».
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct MINIDUMP_EXCEPTION_INFORMATION
        {
            public uint ThreadId;
            public IntPtr ExceptionPointers;
            public int ClientPointers;
        }

        // Ставится один раз: фильтр не должен звать сам себя, когда сбоит запись.
        private static int _writing;

        private static int OnException(ref EXCEPTION_POINTERS info)
        {
            if (System.Threading.Interlocked.Exchange(ref _writing, 1) == 1)
            {
                // Рекурсивный вход: дамп уже пишется — не затирать его крашем.
                return 1;
            }

            try
            {
                string directory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Kite2", "logs");
                Directory.CreateDirectory(directory);

                string file = Path.Combine(
                    directory,
                    "Kite2.Scan.crash-" + ProcessId + ".dmp");

                var pack = new MINIDUMP_EXCEPTION_INFORMATION
                {
                    ThreadId = unchecked((uint)GetCurrentThreadId()),
                    ExceptionPointers = Marshal.AllocHGlobal(
                        Marshal.SizeOf(typeof(EXCEPTION_POINTERS))),
                    ClientPointers = 0
                };
                Marshal.StructureToPtr(info, pack.ExceptionPointers, false);

                using (var stream = new FileStream(
                    file, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    IntPtr packBuffer = Marshal.AllocHGlobal(
                        Marshal.SizeOf(typeof(MINIDUMP_EXCEPTION_INFORMATION)));
                    Marshal.StructureToPtr(pack, packBuffer, false);
                    try
                    {
                        MiniDumpWriteDump(
                            GetCurrentProcess(), ProcessId,
                            stream.SafeFileHandle.DangerousGetHandle(), DumpType,
                            packBuffer, IntPtr.Zero, IntPtr.Zero);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(packBuffer);
                        Marshal.FreeHGlobal(pack.ExceptionPointers);
                    }
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
