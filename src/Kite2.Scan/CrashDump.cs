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
                // Рекурсивный вход: запись уже началась — не рвать её вторым крашем.
                return 1;
            }

            // ПОСЛЕДНЯЯ запись журнала — самая ценная: код краша и адрес
            // инструкции, найдя которым модуль драйвера в момент падения.
            try
            {
                uint code = unchecked((uint)Marshal.ReadInt32(info.PointerRecord, 0));
                uint exAddr = unchecked((uint)Marshal.ReadInt32(info.PointerRecord, 0x10));
                uint eip = unchecked((uint)Marshal.ReadInt32(info.PointerContext, 0xB8));
                uint esp = unchecked((uint)Marshal.ReadInt32(info.PointerContext, 0xC4));

                string module = CrashModuleOf(eip);

                NLog.LogManager.GetLogger("crash").Fatal(
                    "Kyocera: краш 0x{0:X8} по адресу 0x{1:X8}; EIP 0x{2:X8} ({3}); ESP 0x{4:X8}",
                    code, exAddr, eip, module, esp);
                try { NLog.LogManager.Flush(); } catch { }
            }
            catch
            {
                // Даже чтение контекста сбоит — идём дальше к дампу.
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

        // Адрес → модуль в момент краша: GetModuleHandleEx не берёт ссылку
        // и терпит брошенные модули. Возврат «имя+RVA» для точного попадания
        // в карту Ghidra.
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool GetModuleHandleExW(uint flags, IntPtr name, out IntPtr module);

        [DllImport("psapi.dll", CharSet = CharSet.Unicode)]
        private static extern uint GetModuleFileNameExW(IntPtr process, IntPtr module, [Out] System.Text.StringBuilder name, uint size);

        private static string CrashModuleOf(uint address)
        {
            try
            {
                long allocBase = AllocationBaseOf(new IntPtr(unchecked((long)address)));
                if (allocBase == 0)
                    return "не в модуле (адрес за пределами образов)";

                if (GetModuleHandleExW(2 /*UNCHANGED_REFCOUNT*/ | 4 /*FROM_ADDRESS*/,
                    new IntPtr(unchecked((long)address)), out IntPtr module)
                    && module != IntPtr.Zero)
                {
                    var name = new System.Text.StringBuilder(512);
                    GetModuleFileNameExW(GetCurrentProcess(), module, name, 512);

                    string[] parts = name.ToString().Split('\\');
                    string file = parts[parts.Length - 1];

                    long rva = address - allocBase;
                    return file + "+0x" + rva.ToString("X");
                }
            }
            catch
            {
            }
            return "модуль не найден";
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualQueryEx(IntPtr process, IntPtr address, out MEMORY_BASIC_INFORMATION info, uint length);

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public uint RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        private static long AllocationBaseOf(IntPtr address)
        {
            try
            {
                if (VirtualQueryEx(GetCurrentProcess(), address,
                    out MEMORY_BASIC_INFORMATION info,
                    (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION))))
                    return info.AllocationBase.ToInt64();
            }
            catch
            {
            }
            return 0;
        }
    }
}
