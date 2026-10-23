// Мини-дамп при нативном падении ребёнка. Управляемый фильтр ставится
// через SetUnhandledExceptionFilter: когда драйвер/DSM рушит процесс по
// 0xC0000005 / 0xC000041D (видели на Kyocera), остался только код выхода —
// а с дампом можно открыть файл в WinDbg/на Ghidra и увидеть Faulting
// модуль и адрес. Нужно только для отладки — на верный канал не влияет.
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Kite2.Probe
{
    internal static class MiniDump
    {
        private static string _fileMask;
        private static int _counter;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr NativeFilter(IntPtr exceptionPointers);

        // Держим делегат живым на весь срок жизни процесса — иначе сборщик
        // мусора подберёт маршалинг-заглушку, и фильтр упадёт сам же.
        private static NativeFilter _keep;

        //WithDataSegs | WithHandleData | WithUnloadedModules | WithProcessId
        private const uint DumpFlags = 0x1 | 0x4 | 0x20 | 0x400;

        public static void Install(string dir, string nameBase)
        {
            _fileMask = Path.Combine(dir ?? ".", nameBase + ".crash{0}.dmp");
            _keep = Filter;
            SetUnhandledExceptionFilter(_keep);
        }

        private static IntPtr Filter(IntPtr exceptionPointers)
        {
            try
            {
                int n = Interlocked.Increment(ref _counter);
                string path = _fileMask != null
                    ? string.Format(_fileMask, n)
                    : "crash" + n + ".dmp";
                IntPtr h = CreateFileW(path, GenericWrite, ShareReadWrite,
                    IntPtr.Zero, CreateAlways, 0, IntPtr.Zero);
                if (h.ToInt64() != -1)
                {
                    try
                    {
                        MiniDumpWriteDump(GetCurrentProcess(), GetCurrentProcessId(),
                            h, DumpFlags, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                    }
                    catch { }
                    CloseHandle(h);
                    try
                    {
                        File.WriteAllText(Path.ChangeExtension(path, ".txt"),
                            "дамп на месте (см. парный .dmp); нативное падение потока процесса",
                            new System.Text.UTF8Encoding(false));
                    }
                    catch { }
                }
            }
            catch { }
            return (IntPtr)1; // EXCEPTION_EXECUTE_HANDLER — дальше корректная смерть
        }

        private const uint GenericWrite = 0x40000000;
        private const uint ShareReadWrite = 3;
        private const uint CreateAlways = 2;

        [DllImport("kernel32", SetLastError = true)]
        private static extern IntPtr SetUnhandledExceptionFilter(NativeFilter filter);

        [DllImport("kernel32")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32")]
        private static extern uint GetCurrentProcessId();

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateFileW(string name, uint access, uint share,
            IntPtr security, uint disposition, uint flags, IntPtr template);

        [DllImport("kernel32")]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("dbghelp.dll", SetLastError = true)]
        private static extern bool MiniDumpWriteDump(IntPtr process, uint processId,
            IntPtr file, uint type, IntPtr exception, IntPtr user, IntPtr callbacks);
    }
}
