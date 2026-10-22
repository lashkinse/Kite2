// Окно-родитель: создаётся один раз на эксперимент, используется и как
// «родитель» OPENDSM/ENABLEDS, и как очередь оконных сообщений драйвера
// (DAT_EVENT MSG_PROCESSEVENT — тот же путь, что NTwain.IsTwainMessage).
// Раньше это было message-only окно (HWND_MESSAGE); на Kyocera готовность
// не приходила ни через колбэк, ни через DAT_EVENT, а капы читались мусором.
// NTwain в работающей связке держит настоящее скрытое окно верхнего
// уровня (HwndSource) — теперь так же: WS_OVERLAPPED без показа, parent = 0.
using System;
using System.Runtime.InteropServices;

namespace Kite2.Probe
{
    internal sealed class PumpWindow
    {
        private readonly LineLog _log;
        private IntPtr _hwnd;
        private ushort _registeredClass; // полезна для DestroyClass

        /// <summary>
        /// Качать сообщения всей очереди потока, а не только нашего окна.
        /// Для ShowUI=1: диалог драйвера шлёт сообщения своим окнам,
        /// фильтр по hwnd их не возьмёт.
        /// </summary>
        public bool PumpAll { get; set; }

        // Счётчик однотипных сообщений: окно драйвера шлёт сотни WM_PAINT и
        // WM_MOUSEMOVE, из-за них журнал разрастался на тысячи строк.
        private readonly System.Collections.Generic.Dictionary<uint, int> _msgCounts =
            new System.Collections.Generic.Dictionary<uint, int>();
        private int _msgTotal;
        private int _msgLogged;

        /// <summary>
        /// Стоит ли печатать это сообщение в журнал: первые три однотипных —
        /// да, дальше молчим (итог пишется в конце).
        /// </summary>
        public bool AllowLog(uint msg)
        {
            _msgTotal++;
            int n;
            _msgCounts.TryGetValue(msg, out n);
            n++;
            _msgCounts[msg] = n;
            if (n <= 3)
            {
                _msgLogged++;
                return true;
            }
            return false;
        }

        /// <summary>Итог по подавленному шуму оконных сообщений.</summary>
        public string NoiseSummary()
        {
            if (_msgTotal == 0)
                return "оконных сообщений не было";
            return "оконных сообщений всего " + _msgTotal + ", в журнал попало " + _msgLogged;
        }

        public PumpWindow(LineLog log)
        {
            _log = log;
        }

        public IntPtr Create()
        {
            IntPtr inst = Marshal.GetHINSTANCE(typeof(PumpWindow).Module);
            var cls = new Native32.WndClass
            {
                CbSize = (uint)Marshal.SizeOf(typeof(Native32.WndClass)),
                Style = 0,
                Proc = DefProc,
                Instance = inst,
                Name = "Kite2ProbeWnd" + Guid.NewGuid().ToString("N"),
                Menu = null
            };
            ushort atom = Native32.RegisterClassExW(ref cls);
            _registeredClass = atom;
            if (atom == 0)
            {
                int err = Marshal.GetLastWin32Error();
                _log.Info("RegisterClassW не удался (код {0}) — окно всё равно попробуем", err);
            }

            // Настоящее окно верхнего уровня: parent = 0, стиль WS_OVERLAPPED
            // без WS_VISIBLE — скрытый, как окно NTwain.
            _hwnd = Native32.CreateWindowExW(
                0, atom != 0 ? cls.Name : "STATIC", "Kite2.Probe",
                0 /* WS_OVERLAPPED без показа */, 0, 0, 0, 0,
                IntPtr.Zero, IntPtr.Zero, inst, IntPtr.Zero);

            if (_hwnd == IntPtr.Zero)
            {
                int err = Marshal.GetLastWin32Error();
                _log.Info("CreateWindowExW не удался (код {0}) — ENABLEDS пойдёт с Zero-родителем", err);
                return IntPtr.Zero;
            }

            _log.Info("окно программы: {0} (верхнего уровня, скрытое)", _hwnd);
            return _hwnd;
        }

        // Прокачать очередь сообщений окна; колбэк на каждое: может
        // остановить выгрузку (готовность пришла). Каждое сообщение пишется
        // в журнал — сообщений немного, а их список и есть карта TWAIN-канала.
        public void Drain(Func<Native32.MinimumMsg, bool> visitor)
        {
            uint race = 0; // PM_NOREMOVE не нужен: выгребаем всё живое.
            IntPtr filter = PumpAll ? IntPtr.Zero : _hwnd;
            while (Native32.PeekMessageW(out var m, filter, 0, 0, race | 1))
            {
                if (AllowLog(m.Message))
                    _log.Info("окно ← msg 0x{0:X4} wp 0x{1:X} lp 0x{2:X}", m.Message, m.WParam.ToInt64(), m.LParam.ToInt64());
                Native32.TranslateMessage(ref m);
                Native32.DispatchMessage(ref m);
                if (visitor(m))
                    return; // «хватит»: вызывающий сам решает, что это значит
            }
        }

        private static IntPtr DefProc(IntPtr hwnd, uint msg, IntPtr wp, IntPtr lp)
        {
            return Native32.DefWindowProcW(hwnd, msg, wp, lp);
        }
    }
}
