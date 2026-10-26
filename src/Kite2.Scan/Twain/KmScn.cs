// Окно в нативный мир KMUSBSCN3.dll (базовый модуль Kyocera): вызов экспорта
// KmUsbScnAccept напрямую. Порт из обследователя Kite2.Probe.
//
// Реверс (Ghidra, образ 0x10000000): KmUsbScnAccept — __cdecl без формальных
// параметров, читает 20 int прямо со стека под адресом возврата (та же
// структура, что у KmUsbScnStart: nImgSize/nTxtOrient/nImgPos/nImgType/
// nScanMode/nDuplexMode/nDuplexEdge/nResizeImg/nClrMode/nRez/nInvert/
// nAutoBright/nBright/nContrast/nJpegCompRate/rctScanArea/nSharpen),
// ставит g_nCurrentMode = 2 (SCAN_MODE_ACCEPT), копирует карту в глобал
// параметров и будит потоки.
//
// Именно этот звонок — недостающая часть проверенной схемы оконной пачки:
//   * с ним обследователь жил с драйвером 4 минуты после передачи страниц
//     без единой смерти (эксп. 21: Accept после листа 1 → mode=2);
//   * без него драйвер роняет сам себя через ~1,3 с после передачи страницы
//     (прогон клиента 1.7.38: 0xC000041D, звонков от приложения не было);
// Разбор обоих поколений драйверов решён выбором профиля RVAs по
// TimeDateStamp PE-заголовка загруженного модуля:
//   0x5088C72C (2012-10-25, старый KMTWAIN) — old;
//   0x66DAC1D8 (2024-09-06, пакет KMTWAIN_GDI) — new.
// У нового механика дословно та же, только сместились глобалы.
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NLog;

namespace Kite2.Scan.Twain
{
    internal static class KmScn
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public const string ModuleName = "KMUSBSCN3.dll";
        public const int ParamCount = 20;

        // Профиль RVAs конкретной сборки модуля.
        internal sealed class Profile
        {
            public uint Stamp;      // TimeDateStamp PE-заголовка
            public string Name;     // для журнала
            public int Params;      // карта 20 int (как у KmUsbScnStart)
        }

        private static readonly Profile[] Profiles =
        {
            new Profile
            {
                Stamp = 0x5088C72Cu, Name = "old (2012-10-25)",
                Params = 0x21050
            },
            new Profile
            {
                Stamp = 0x66DAC1D8u, Name = "new (2024-09-06)",
                Params = 0x23F68
            }
        };

        public static IntPtr GetModule(int waitMs)
        {
            var wait = Stopwatch.StartNew();
            IntPtr m = GetModuleHandleW(ModuleName);
            while (m == IntPtr.Zero && wait.ElapsedMilliseconds < waitMs)
            {
                System.Threading.Thread.Sleep(50);
                m = GetModuleHandleW(ModuleName);
            }
            if (m == IntPtr.Zero)
                Log.Info("Kyocera: KMUSBSCN3.dll в процессе не загружен (ждали {0:N1} с)", wait.Elapsed.TotalSeconds);
            else
                Log.Info("Kyocera: KMUSBSCN3.dll @ 0x{0:X8} (найден за {1:N1} с)", m.ToInt64(), wait.Elapsed.TotalSeconds);
            return m;
        }

        // Опознание загруженной сборки: TimeDateStamp из заголовка PE
        // (e_lfanew + 8) прямо из памяти процесса. null — модуль не наш.
        public static Profile Detect(IntPtr dll)
        {
            try
            {
                int lfa = Marshal.ReadInt32(dll, 0x3C);
                uint stamp = unchecked((uint)Marshal.ReadInt32(dll, lfa + 8));
                foreach (var p in Profiles)
                {
                    if (p.Stamp == stamp)
                    {
                        Log.Info("Kyocera: сборка KMUSBSCN3: {0} (stamp 0x{1:X8})", p.Name, stamp);
                        return p;
                    }
                }
                Log.Info("Kyocera: сборка KMUSBSCN3 неопознана (stamp 0x{0:X8}) — вмешательства отключены", stamp);
                return null;
            }
            catch (Exception ex)
            {
                Log.Info("Kyocera: чтение PE-заголовка не удалось: {0}", ex.Message);
                return null;
            }
        }

        // KmUsbScnRefresh: окно драйвера зовёт его из своего таймера при каждой
        // смене состояния устройства (twMainWnd → FUN_10006dd0(0,2)) — так оно
        // «услышивает» податчик заново и держит движок в живом разговоре.
        // Карта та же 20 int, что у Accept. Любая ошибка — только в журнал.
        public static int CallRefresh(IntPtr dll, Profile prof, int[] p)
        {
            IntPtr proc = GetProcAddress(dll, "KmUsbScnRefresh");
            if (proc == IntPtr.Zero)
            {
                Log.Info("Kyocera: GetProcAddress(KmUsbScnRefresh) = 0 (код {0})", Marshal.GetLastWin32Error());
                return -1;
            }
            Log.Info("Kyocera: KmUsbScnRefresh @ 0x{0:X8} — идём", proc.ToInt64());
            try { LogManager.Flush(); } catch { }
            var fp = (AcceptDelegate)Marshal.GetDelegateForFunctionPointer(proc, typeof(AcceptDelegate));
            try
            {
                return fp(p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8], p[9],
                          p[10], p[11], p[12], p[13], p[14], p[15], p[16], p[17], p[18], p[19]);
            }
            catch (Exception ex)
            {
                Log.Info("Kyocera: вызов KmUsbScnRefresh не удался: {0}", ex.Message);
                return -2;
            }
        }

        // KmUsbScnCancel — «Сброс» окна драйвера (FUN_1000add0): гасит задание
        // движка, податчик перестаёт моргать, режим движка возвращается из
        // ACCEPT (2) в покой. Звонок без параметров (__cdecl).
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void CancelDelegate();

        public static void CallCancel(IntPtr dll, Profile prof)
        {
            IntPtr proc = GetProcAddress(dll, "KmUsbScnCancel");
            if (proc == IntPtr.Zero)
            {
                Log.Info("Kyocera: GetProcAddress(KmUsbScnCancel) = 0 (код {0})", Marshal.GetLastWin32Error());
                return;
            }
            Log.Info("Kyocera: KmUsbScnCancel @ 0x{0:X8} — гасим задание движка", proc.ToInt64());
            try { LogManager.Flush(); } catch { }
            var fp = (CancelDelegate)Marshal.GetDelegateForFunctionPointer(proc, typeof(CancelDelegate));
            fp();
            Log.Info("Kyocera: KmUsbScnCancel отработал; режим движка теперь {0}", ReadMode(dll, prof));
        }

        public static int[] ReadParams(IntPtr dll, Profile prof)        {
            var p = new int[ParamCount];
            for (int i = 0; i < ParamCount; i++)
                p[i] = Marshal.ReadInt32(dll, prof.Params + i * 4);
            return p;
        }

        private const int ModeOffsetNew = 0x24044; // g_nCurrentMode, сборка new

        // Вызываем экспорт KmUsbScnAccept с той же картой 20 int, которую
        // UI драйвера кладёт на стек при «Скан». Только для x86-процесса —
        // модуль 32-разрядный (иначе GetModuleHandle его не находит).
        public static int CallAccept(IntPtr dll, Profile prof, int[] p)
        {
            IntPtr proc = GetProcAddress(dll, "KmUsbScnAccept");
            if (proc == IntPtr.Zero)
            {
                Log.Info("Kyocera: GetProcAddress(KmUsbScnAccept) = 0 (код {0})", Marshal.GetLastWin32Error());
                return -1;
            }
            Log.Info("Kyocera: KmUsbScnAccept @ 0x{0:X8}, карта 20 int со стека — идём", proc.ToInt64());
            try { LogManager.Flush(); } catch { }
            var fp = (AcceptDelegate)Marshal.GetDelegateForFunctionPointer(proc, typeof(AcceptDelegate));
            try
            {
                return fp(p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8], p[9],
                          p[10], p[11], p[12], p[13], p[14], p[15], p[16], p[17], p[18], p[19]);
            }
            catch (Exception ex)
            {
                Log.Info("Kyocera: вызов KmUsbScnAccept не удался: {0}", ex.Message);
                return -2;
            }
        }

        private const int ModeOffsetOld = 0x210F4; // g_nCurrentMode, сборка old

        // Режим движка: 0/1 — скан, 2 — ACCEPT (подача листов разрешена).
        public static int ReadMode(IntPtr dll, Profile prof)
        {
            int offset = prof.Name.StartsWith("old") ? ModeOffsetOld : ModeOffsetNew;
            return Marshal.ReadInt32(dll, offset);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int AcceptDelegate(
            int p01, int p02, int p03, int p04, int p05,
            int p06, int p07, int p08, int p09, int p10,
            int p11, int p12, int p13, int p14, int p15,
            int p16, int p17, int p18, int p19, int p20);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetModuleHandleW(string name);

        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr module, string name);
    }
}
