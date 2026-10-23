// Окно в нативный мир KMUSBSCN3.dll (базовый модуль Kyocera): глобалы
// состояния и вызов экспорта KmUsbScnAccept directly. Реверс (Ghidra,
// образ 0x10000000): KmUsbScnAccept — __cdecl без формальных параметров,
// он читает 20 int прямо со стека под адресом возврата (та же структура,
// что у KmUsbScnStart: см. лог «=== KmUsbScnStart() is Called» с полями
// nImgSize/nTxtOrient/nImgPos/nImgType/nScanModed/nDuplexMode/nDuplexEdge/
// nResizeImg/nClrMode/nRez/nInvert/nAutoBright/nBright/nContrast/
// nJpegCompRate/rctScanArea/nSharpen), ставит g_nCurrentMode = 2
// (SCAN_MODE_ACCEPT), копирует карту в глобал параметров и будит потоки.
// Проблемы двух поколений драйверов решены выбором профиля RVAs по
// TimeDateStamp PE-заголовка загруженного модуля:
//   0x5088C72C (2012-10-25, старый, C:\Windows\TWAIN_32\KMTWAIN) — old;
//   0x66DAC1D8 (2024-09-06, новый пакет, ...KMTWAIN_GDI) — new.
// У нового местная механика дословно та же, что и у старого, только
// сместились глобалы (проверено декомпиляцией Nd-функций).
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
        public const int ParamCount = 20;

        // Профиль RVAs конкретной сборки модуля.
        internal sealed class Profile
        {
            public uint Stamp;      // TimeDateStamp PE-заголовка
            public string Name;     // для журнала
            public int Params;      // карта 20 int (как у KmUsbScnStart)
            public int Mode;        // g_nCurrentMode: 0/1 скан, 2 ACCEPT
            public int Machine;     // покоящийся образ DAT×2004C/23048
            public int Preview;     // флаг preview
            public int State
            {
                get { return Params + 4; } // DAT_…54/…6c — пакопия параметров[1]
            }
        }

        private static readonly Profile[] Profiles =
        {
            new Profile
            {
                Stamp = 0x5088C72Cu, Name = "old (2012-10-25)",
                Params = 0x21050, Mode = 0x210F4, Machine = 0x2004C, Preview = 0x21128
            },
            new Profile
            {
                Stamp = 0x66DAC1D8u, Name = "new (2024-09-06)",
                Params = 0x23F68, Mode = 0x24044, Machine = 0x23048, Preview = 0x24078
            }
        };

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

        // Опознание загруженной сборки: TimeDateStamp из заголовка PE
        // (e_lfanew + 8) прямо из памяти процесса.
        public static Profile Detect(IntPtr dll, LineLog log)
        {
            try
            {
                int lfa = Marshal.ReadInt32(dll, 0x3C);
                uint stamp = unchecked((uint)Marshal.ReadInt32(dll, lfa + 8));
                foreach (var p in Profiles)
                {
                    if (p.Stamp == stamp)
                    {
                        log.Info("сборка KMUSBSCN3: {0} (stamp 0x{1:X8}) — карта RVAs есть",
                            p.Name, stamp);
                        return p;
                    }
                }
                log.Info("сборка KMUSBSCN3 НЕОПОЗНАНА (stamp 0x{0:X8}) — чтение глобалов и вмешательства отключены",
                    stamp);
                return null;
            }
            catch (Exception ex)
            {
                log.Info("чтение PE-заголовка не удалось: {0}", ex.Message);
                return null;
            }
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
            var prof = Detect(m, log);
            if (prof == null)
            {
                log.Info("<{0}> снимок состояния недоступен: RVAs неизвестны", tag);
                return;
            }
            int mode = Marshal.ReadInt32(m, prof.Mode);
            int state = Marshal.ReadInt32(m, prof.State);
            int konst = Marshal.ReadInt32(m, prof.Machine);
            int preview = Marshal.ReadInt32(m, prof.Preview);
            var sb = new StringBuilder();
            for (int i = 0; i < ParamCount; i++)
            {
                if (i > 0)
                    sb.Append(',');
                sb.Append("0x");
                sb.Append(Marshal.ReadInt32(m, prof.Params + i * 4).ToString("X"));
            }
            log.Info("<{0}> mode={1} DAT(параметр[1])={2} покой={3} preview={4}",
                tag, mode, state, konst, preview);
            log.Info("<{0}> параметры: {1}", tag, sb.ToString());
        }

        public static int[] ReadParams(IntPtr dll, Profile prof)
        {
            var p = new int[ParamCount];
            for (int i = 0; i < ParamCount; i++)
                p[i] = Marshal.ReadInt32(dll, prof.Params + i * 4);
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
        public static int CallAccept(IntPtr dll, Profile prof, int[] p, LineLog log)
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
