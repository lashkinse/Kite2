// Окно в нативный мир KMUSBSCN3.dll (базовый модуль Kyocera): глобалы
// состояния и вызов экспорта KmUsbScnAccept directly. Реверс (Ghidra,
// образ 0x10000000): KmUsbScnAccept — __cdecl без формальных параметров,
// он читает 20 int прямо со стека под адресом возврата (та же структура,
// что у KmUsbScnStart), ставит g_nCurrentMode (DAT_100210f4) = 2
// (SCAN_MODE_ACCEPT), копирует карту в DAT_10021050 и будит потоки
// событиями DAT_100210a4/d4. DAT_10021054 — первый элемент той же карты
// (в покое = 0x128 = DAT_1002004c).
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Kite2.Probe
{
    internal static class KmScn
    {
        public const string ModuleName = "KMUSBSCN3.dll";

        // RVAs относительно базы модуля (образ в Ghidra — база 0x10000000).
        public const int RvaParams = 0x21050;  // карта 20 int (как у KmUsbScnStart)
        public const int RvaState  = 0x21054;  // DAT_10021054 — перекрывает параметры[1]
        public const int RvaMachine = 0x2004C; // DAT_1002004c — константный «покой»
        public const int RvaMode   = 0x210F4;  // g_nCurrentMode: 0/1 скан, 2 ACCEPT
        public const int RvaPreview = 0x21128; // флаг preview (_DAT_10021128)
        public const int ParamCount = 20;

        public static IntPtr GetModule(LineLog log, int waitMs)
        {
            var wait = Stopwatch.StartNew();
            IntPtr m = GetModuleHandleW(ModuleName);
            while (m == IntPtr.Zero && wait.ElapsedMilliseconds < waitMs)
            {
                Thread.Sleep(50);
                m = GetModuleHandleW(ModuleName);
            }
            if (m == IntPtr.Zero)
                log.Info("KMUSBSCN3.dll в процессе НЕ загружен (ждали {0:N1} с)", wait.Elapsed.TotalSeconds);
            else
                log.Info("KMUSBSCN3.dll @ 0x{0:X8} (найден за {1:N1} с)", m.ToInt64(), wait.Elapsed.TotalSeconds);
            return m;
        }

        // Снимок состояния модуля: режим, карта параметров, покоящийся образ.
        public static void LogState(LineLog log, string tag)
        {
            IntPtr m = GetModuleHandleW(ModuleName);
            if (m == IntPtr.Zero)
            {
                log.Info("<{0}> KMUSBSCN3.dll не загружен", tag);
                return;
            }
            int mode = Marshal.ReadInt32(m, RvaMode);
            int state = Marshal.ReadInt32(m, RvaState);
            int konst = Marshal.ReadInt32(m, RvaMachine);
            int preview = Marshal.ReadInt32(m, RvaPreview);
            var sb = new StringBuilder();
            for (int i = 0; i < ParamCount; i++)
            {
                if (i > 0)
                    sb.Append(',');
                sb.Append("0x");
                sb.Append(Marshal.ReadInt32(m, RvaParams + i * 4).ToString("X"));
            }
            log.Info("<{0}> mode(DAT_100210f4)={1} DAT_10021054={2} DAT_1002004c={3} preview(DAT_10021128)={4}",
                tag, mode, state, konst, preview);
            log.Info("<{0}> параметры DAT_10021050: {1}", tag, sb.ToString());
        }

        public static int[] ReadParams(IntPtr dll)
        {
            var p = new int[ParamCount];
            for (int i = 0; i < ParamCount; i++)
                p[i] = Marshal.ReadInt32(dll, RvaParams + i * 4);
            return p;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int AcceptDelegate(
            int p01, int p02, int p03, int p04, int p05,
            int p06, int p07, int p08, int p09, int p10,
            int p11, int p12, int p13, int p14, int p15,
            int p16, int p17, int p18, int p19, int p20);

        // Вызываем экспорт KmUsbScnAccept с той же картой 20 int, что UI
        // драйвера кладёт на стек при «Скан». Только для x86-процесса —
        // модуль 32-разрядный, иначе GetModuleHandle его и не найдёт и
        // в чужой архитектуре не загрузится.
        public static int CallAccept(IntPtr dll, int[] p, LineLog log)
        {
            IntPtr proc = GetProcAddress(dll, "KmUsbScnAccept");
            if (proc == IntPtr.Zero)
            {
                log.Info("GetProcAddress(KmUsbScnAccept) = 0 (код {0})", Marshal.GetLastWin32Error());
                return -1;
            }
            log.Info("KmUsbScnAccept @ 0x{0:X8}, карта 20 int со стека — идём", proc.ToInt64());
            var fp = (AcceptDelegate)Marshal.GetDelegateForFunctionPointer(proc, typeof(AcceptDelegate));
            try
            {
                return fp(p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8], p[9],
                          p[10], p[11], p[12], p[13], p[14], p[15], p[16], p[17], p[18], p[19]);
            }
            catch (Exception ex)
            {
                log.Info("вызов KmUsbScnAccept не удался: {0}", ex.Message);
                return -2;
            }
        }

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetModuleHandleW(string name);

        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr module, string name);
    }
}
