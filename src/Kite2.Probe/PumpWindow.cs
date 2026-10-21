// С bfsida window: создаётся один раз на эксперимент, используется и как
// «родитель» OPENDSM/ENABLEDS, и как очередь оконных сообщений драйвера
// (DAT_EVENT MSG_PROCESSEVENT — тот же путь, что NTwain.IsTwainMessage).
// Окно message-only (HWND_MESSAGE): не попадает в Alt-Tab и не мигает.
using System;
using System.Runtime.InteropServices;

namespace Kite2.Probe
{
    internal sealed class PumpWindow
    {
        private readonly LineLog _log;
        private IntPtr _hwnd;
        private ushort _registeredClass; // полезна для DestroyClass

        public PumpWindow(LineLog log)
        {
            this._log = log;
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

            _hwnd = Native32.CreateWindowExW(
                0, atom != 0 ? cls.Name : "STATIC", "Kite2.Probe",
                0 /* WS_OVERLAPPED без показа */, 0, 0, 0, 0,
                Native32.HwndMessage, IntPtr.Zero, inst, IntPtr.Zero);

            if (_hwnd == IntPtr.Zero)
            {
                int err = Marshal.GetLastWin32Error();
                _log.Info("CreateWindowExW не удался (код {0}) — ENABLEDS пойдёт с Zero-родителем", err);
                return IntPtr.Zero;
            }

            _log.Info("окно программы: {0}", _hwnd);
            return _hwnd;
        }

        // Прокачать очередь сообщений окна; колбэк на каждое: может
        // остановить выгрузку (готовность пришла).
        public void Drain(Func<Native32.MinimumMsg, bool> visitor)
        {
            uint race = 0; // PM_NOREMOVE не нужен: выгребаем всё живое.
            while (Native32.PeekMessageW(out var m, _hwnd, 0, 0, race | 1))
            {
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
