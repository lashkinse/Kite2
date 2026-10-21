// Собственно обследование: один процесс — один сценарий. Хроника агрессивно
// честная: ПЕРЕД каждым триплетом в журнал пишется «→ DAT_…», поэтому после
// повода видно, в каком нативном вызове ребёнок висит навсегда.
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Kite2.Probe
{
    internal sealed class Case
    {
        public enum Kind
        {
            ImageInfoThenPending = 0,
            PendingOnly = 1,
            Direct = 2
        }

        public int Index;
        public string Title;
        public bool NewDsm;
        public bool Memory;
        public Kind Handshake;
    }

    internal static class Cases
    {
        private const int Total = 9;

        internal static Case Get(int idx)
        {
            switch (idx)
            {
                case 1:
                    return Make(true, false, Case.Kind.ImageInfoThenPending,
                        "Native: IMAGEINFO → PENDINGXFERS → NATIVEXFER (полностью по спецификации)");
                case 2:
                    return Make(true, false, Case.Kind.PendingOnly,
                        "Native: PENDINGXFERS → NATIVEXFER (как делает NAPS2)");
                case 3:
                    return Make(true, false, Case.Kind.Direct,
                        "Native: прямой NATIVEXFER без подготовительных звонков");
                case 4:
                    return Make(true, true, Case.Kind.ImageInfoThenPending,
                        "Memory: IMAGEINFO → PENDINGXFERS → MEMXFER");
                case 5:
                    return Make(true, true, Case.Kind.PendingOnly,
                        "Memory: PENDINGXFERS → MEMXFER (как NAPS2)");
                case 6:
                    return Make(true, true, Case.Kind.Direct,
                        "Memory: прямой MEMXFER");
                case 7:
                    return Make(false, false, Case.Kind.PendingOnly,
                        "Old-DSM Native: PENDINGXFERS → NATIVEXFER");
                case 8:
                    return Make(false, false, Case.Kind.ImageInfoThenPending,
                        "Old-DSM Native: IMAGEINFO → PENDINGXFERS → NATIVEXFER");
                case 9:
                    return Make(false, true, Case.Kind.Direct,
                        "Old-DSM Memory: прямой MEMXFER");
                default:
                    return null;
            }
        }

        private static Case Make(bool newDsm, bool memory, Case.Kind handshake, string title)
        {
            return new Case { NewDsm = newDsm, Memory = memory, Handshake = handshake, Title = title };
        }

        internal static int Count
        {
            get { return Total; }
        }
    }

    internal static class Experiment
    {
        // Аргументы exp: idx, wallMs, logPath; необязательный фильтр источника.
        public static int Run(string[] args)
        {
            int idx = int.Parse(args[1]);
            int wall = int.Parse(args[2]);
            string logPath = args[3];
            string filter = args.Length > 4 ? args[4] : string.Empty;

            var caseInfo = Cases.Get(idx);
            if (caseInfo == null)
                return 4;

            var log = new LineLog(logPath);
            log.Info("Эксперимент {0}: {1}", idx, caseInfo.Title);
            log.Info("DSM: {0}; передача: {1}; поводок {2} с",
                caseInfo.NewDsm ? "новый (twaindsm.dll)" : "старый (twain_32.dll)",
                caseInfo.Memory ? "memory" : "native",
                wall / 1000);

            string loadError = Dsm.LoadLibraries();
            if (loadError != null)
            {
                log.Info(loadError);
                return 2;
            }

            try
            {
                return Play(log, caseInfo, wall, filter);
            }
            catch (Exception ex)
            {
                log.Info("сбой исключением: {0}", ex.Message);
                return 1;
            }
        }

        private static int Play(LineLog log, Case caseInfo, int wall, string filter)
        {
            var app = TWIdentity.App(caseInfo.NewDsm);
            var ds = TWIdentity.Empty();

            // 1. Окно программы: его DSM берёт себе «родителем».
            var pump = new PumpWindow(log);
            IntPtr hwnd = pump.Create();

            // 2. MSG_OPENDSM (DAT_PARENT, pData — указатель на HWND).
            ushort rc;
            var dsw = Stopwatch.StartNew();
            rc = CallWithHwnd(caseInfo.NewDsm, app, Msg.OpenDSM, hwnd);
            log.Info("MSG_OPENDSM: {0} (id {1}) за {2:N0} мс", TwRc.Name(rc), app.Id, dsw.ElapsedMilliseconds);
            if (rc != TwRc.Success)
            {
                log.Info("диспетчер не открылся — конец");
                return 2;
            }

            try
            {
                // 4. Источник (по умолчанию или по фильтру).
                rc = Dsm.Call(caseInfo.NewDsm, app, null, Dg.Control, Dat.Identity, Msg.GetDefault, ds);
                if (rc != TwRc.Success)
                {
                    log.Info("источник по умолчанию не получен: {0}", TwRc.Name(rc));
                    return 2;
                }
                log.Info("источник: «{0}», производитель «{1}», версия {2}.{3}, протокол {4}.{5}",
                    ds.ProductName, ds.Manufacturer, ds.Version.MajorNum, ds.Version.MinorNum,
                    ds.ProtocolMajor, ds.ProtocolMinor);

                if (filter.Length > 0 && (ds.ProductName == null
                    || ds.ProductName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0))
                {
                    var found = FindSource(log, caseInfo.NewDsm, app, filter);
                    if (found == null)
                        log.Info("источник по фильтру «{0}» не найден — работаем с «{1}»", filter, ds.ProductName);
                    else
                    {
                        ds = found;
                        log.Info("источник взят по фильтру: «{0}»", ds.ProductName);
                    }
                }

                if (string.IsNullOrEmpty(ds.ProductName))
                {
                    log.Info("источник не отдал имени — конец");
                    return 2;
                }

                // 5. Открыть источник. ВАЖНО: как вендоренный NTwain, OPENDS
                //    идёт с адресатом = NULL (перегрузка DsmEntry(origin, msg,
                //    TWIdentity) передаёт IntPtr.Zero как pDest) — иначе
                //    драйвер отвечает FAILURE (CC5 на новом, CC11 на старом).
                var sw = Stopwatch.StartNew();
                rc = Dsm.Call(caseInfo.NewDsm, app, null, Dg.Control, Dat.Identity, Msg.OpenDS, ds);
                log.Info("MSG_OPENDS: {0} (id {1}) за {2:N0} мс", TwRc.Name(rc), ds.Id, sw.ElapsedMilliseconds);
                if (rc != TwRc.Success)
                {
                    LogStatus(log, caseInfo.NewDsm, app, ds);
                    return 2;
                }

                // 6. Регистрация колбэка — после OPENDS (state 4), адресат — источник,
                //    как в вендоренном NTwain (UpdateCallback): новый DSM первым
                //    получает DAT_CALLBACK2, при отказе — DAT_CALLBACK (1.9).
                if (caseInfo.NewDsm)
                {
                    rc = Dsm.Call(caseInfo.NewDsm, app, ds, Dg.Control, Dat.Callback2, Msg.RegisterCallback, new TWCallback2(Dsm.SharedCallback));
                    log.Info("Регистрация DAT_CALLBACK2: {0}", TwRc.Name(rc));
                    if (rc != TwRc.Success)
                    {
                        rc = Dsm.Call(caseInfo.NewDsm, app, ds, Dg.Control, Dat.Callback, Msg.RegisterCallback, new TWCallback(Dsm.SharedCallback));
                        log.Info("Обратный путь DAT_CALLBACK: {0}", TwRc.Name(rc));
                    }
                }
                else
                {
                    rc = Dsm.Call(caseInfo.NewDsm, app, ds, Dg.Control, Dat.Callback, Msg.RegisterCallback, new TWCallback(Dsm.SharedCallback));
                    log.Info("Регистрация DAT_CALLBACK: {0}", TwRc.Name(rc));
                    if (rc != TwRc.Success)
                        log.Info("старый DSM колбэк не принял — готовность увидим через DAT_EVENT");
                }

                // 7. Капы, XFERGROUP, XFERMECH, SETUPMEM.
                WatchCaps(log, caseInfo.NewDsm, app, ds);
                WatchXferGroup(log, caseInfo.NewDsm, app, ds);
                SetXferMech(log, caseInfo.NewDsm, app, ds, caseInfo.Memory ? Twsx.Memory : Twsx.Native);
                if (caseInfo.Memory)
                {
                    var setup = new TWSetupMemXfer { MinBufSize = 8192, MaxBufSize = 1048576, DesiredBytes = 1048576 };
                    rc = Dsm.Call(caseInfo.NewDsm, app, ds, Dg.Control, Dat.SetupMemXfer, Msg.Set, setup);
                    log.Info("DAT_SETUPMEMXFER SET (min {0}, max {1}): {2}",
                        setup.MinBufSize, setup.MaxBufSize, TwRc.Name(rc));
                }

                var state = new HandshakeState();
                try
                {
                    // 7. Enable.
                    var ui = new TWUserInterface { ShowUI = 0, ModalUI = 0, HParent = hwnd };
                    sw.Restart();
                    rc = Dsm.Call(caseInfo.NewDsm, app, ds, Dg.Control, Dat.UserInterface, Msg.EnableDS, ui);
                    log.Info("ENABLEDS (NoUI, родитель {0}): {1} за {2:N0} мс", hwnd, TwRc.Name(rc), sw.ElapsedMilliseconds);
                    if (rc != TwRc.Success)
                    {
                        LogStatus(log, caseInfo.NewDsm, app, ds);
                        return 2;
                    }

                    // 8. Готовность.
                    sw.Restart();
                    int left = wall - (int)sw.ElapsedMilliseconds;
                    bool ok = WaitReady(caseInfo.NewDsm, app, ds, pump, Math.Max(15000, left));
                    if (!ok)
                    {
                        log.Info("XferReady не пришёл за {0:N1} с", sw.Elapsed.TotalSeconds);
                        return 2;
                    }
                    log.Info("XferReady через {0:N1} с после ENABLEDS", sw.Elapsed.TotalSeconds);

                    // 9. Стержень: последовательность триплетов после XferReady.
                    RunHandshake(log, caseInfo.NewDsm, app, ds, state, caseInfo);
                }
                finally
                {
                    // 10. Уборка: ENDXFER (только после успешной страницы, иначе
                    //     «нельзя»), отключение, закрытие источника.
                    if (state.Delivered)
                    {
                        rc = Dsm.Call(caseInfo.NewDsm, app, ds, Dg.Control, Dat.PendingXfers, Msg.EndXfer, new TWPendingXfers { Count = 1 });
                        log.Info("MSG_ENDXFER: {0}", TwRc.Name(rc));
                    }
                    rc = Dsm.Call(caseInfo.NewDsm, app, ds, Dg.Control, Dat.UserInterface, Msg.DisableDS, new TWUserInterface());
                    log.Info("MSG_DISABLEDS: {0}", TwRc.Name(rc));
                    rc = Dsm.Call(caseInfo.NewDsm, app, null, Dg.Control, Dat.Identity, Msg.CloseDS, ds);
                    log.Info("MSG_CLOSEDS: {0}", TwRc.Name(rc));
                }

                return state.Delivered ? 0 : 1;
            }
            finally
            {
                SwarmClose(log, caseInfo.NewDsm, app);
            }
        }

        private static void SwarmClose(LineLog log, bool newDsm, TWIdentity app)
        {
            var rc = Dsm.CallPtr(newDsm, app, null, Dg.Control, Dat.Parent, Msg.CloseDSM, IntPtr.Zero);
            log.Info("MSG_CLOSEDSM: {0}", TwRc.Name(rc));
        }

        private static ushort CallWithHwnd(bool newDsm, TWIdentity app, ushort msg, IntPtr hwnd)
        {
            IntPtr holder = Marshal.AllocHGlobal(8);
            Marshal.WriteIntPtr(holder, hwnd);
            try
            {
                return Dsm.CallPtr(newDsm, app, null, Dg.Control, Dat.Parent, msg, holder);
            }
            finally
            {
                Marshal.FreeHGlobal(holder);
            }
        }

        private static TWIdentity FindSource(LineLog log, bool newDsm, TWIdentity app, string filter)
        {
            var source = TWIdentity.Empty();
            ushort rc = Dsm.Call(newDsm, app, null, Dg.Control, Dat.Identity, Msg.GetFirst, source);
            while (rc == TwRc.Success)
            {
                log.Info("источник из списка: «{0}»", source.ProductName);
                if (!string.IsNullOrEmpty(source.ProductName)
                    && source.ProductName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                    return source;
                rc = Dsm.Call(newDsm, app, null, Dg.Control, Dat.Identity, Msg.GetNext, source);
            }
            return null;
        }

        // Чтение капс «на этапе 4» — просто все ключевые за одну строку.
        private static void WatchCaps(LineLog log, bool newDsm, TWIdentity app, TWIdentity ds)
        {
            ReadCap(log, newDsm, app, ds, "ICAP_XFERMECH", CapId.ICapXferMech);
            ReadCap(log, newDsm, app, ds, "ICAP_COMPRESSION", CapId.ICapCompression);
            ReadCap(log, newDsm, app, ds, "ICAP_PIXELTYPE", CapId.ICapPixelType);
            ReadCap(log, newDsm, app, ds, "ICAP_UNITS", CapId.ICapUnits);
            ReadCap(log, newDsm, app, ds, "ICAP_BITDEPTH", CapId.ICapBitDepth);
            ReadCap(log, newDsm, app, ds, "ICAP_XRESOLUTION", CapId.ICapXResolution);
            ReadCap(log, newDsm, app, ds, "ICAP_SUPPORTEDSIZES", CapId.ICapSupportedSizes);
            ReadCap(log, newDsm, app, ds, "CAP_XFERCOUNT", CapId.CapXferCount);
            ReadCap(log, newDsm, app, ds, "CAP_FEEDERENABLED", CapId.CapFeederEnabled);
        }

        private static void ReadCap(LineLog log, bool newDsm, TWIdentity app, TWIdentity ds, string name, ushort cap)
        {
            // Контейнер — GlobalAlloc-хэндл (GMEM_ZEROINIT), как у NTwain:
            // драйвер заполняет его через GlobalLock, поэтому нулевая исходная
            // память честно отличает «драйвер не писал» от «мусор».
            IntPtr h = WinMem.Alloc(256);
            try
            {
                var capObj = new TWCapability(cap, TwOn.DontCare16, h);
                var rc = Dsm.Call(newDsm, app, ds, Dg.Control, Dat.Capability, Msg.Get, capObj);
                if (rc != TwRc.Success)
                {
                    log.Info("{0}: {1}", name, TwRc.Name(rc));
                    return;
                }
                IntPtr p = Native32.GlobalLock(h);
                if (p == IntPtr.Zero)
                {
                    log.Info("{0}: GlobalLock не удался — контейнер пуст?", name);
                    return;
                }
                try
                {
                    log.Info("{0}: {1}", name, DescribeCap(p, capObj.ConType));
                }
                finally
                {
                    Native32.GlobalUnlock(h);
                }
            }
            finally
            {
                WinMem.Free(h);
            }
        }

        private static void WatchXferGroup(LineLog log, bool newDsm, TWIdentity app, TWIdentity ds)
        {
            IntPtr m = Marshal.AllocHGlobal(4);
            Marshal.WriteInt32(m, 0);
            try
            {
                var rc = Dsm.CallPtr(newDsm, app, ds, Dg.Control, Dat.XferGroup, Msg.Get, m);
                if (rc == TwRc.Success)
                    log.Info("DAT_XFERGROUP MSG_GET: 0x{0:X} (control=1, image=2, +audio=4)", Marshal.ReadInt32(m));
                else
                    log.Info("DAT_XFERGROUP MSG_GET: {0}", TwRc.Name(rc));
            }
            finally
            {
                Marshal.FreeHGlobal(m);
            }
        }

        private static void SetXferMech(LineLog log, bool newDsm, TWIdentity app, TWIdentity ds, ushort mech)
        {
            // OneValue-контейнер как у вендоренного NTwain (Pack=2):
            // itemType u16 @0, значение u32 @2, всего 6 байт; пишем через
            // GlobalLock прямо в фиксированный GlobalAlloc-блок.
            IntPtr h = WinMem.Alloc(6);
            try
            {
                IntPtr p = Native32.GlobalLock(h);
                if (p == IntPtr.Zero)
                {
                    log.Info("ICAP_XFERMECH: GlobalAlloc/GlobalLock не удался");
                    return;
                }
                Marshal.WriteInt16(p, 0, (short)TwTy.UInt16);
                Marshal.WriteInt32(p, 2, mech);
                Native32.GlobalUnlock(h);

                var rc = Dsm.Call(newDsm, app, ds, Dg.Control, Dat.Capability, Msg.Set,
                    new TWCapability(CapId.ICapXferMech, TwOn.OneValue, h));
                log.Info("ICAP_XFERMECH = {0}: {1}", mech == Twsx.Memory ? "memory" : "native", TwRc.Name(rc));
            }
            finally
            {
                WinMem.Free(h);
            }
        }

        private static string DescribeCap(IntPtr p, ushort containerType)
        {
            ushort itemType = (ushort)Marshal.ReadInt16(p, 0);
            if (containerType == TwOn.OneValue)
            {
                // Раскладка дремучих OneValue двоякая: у NTwain значение
                // пойдёт на 2 (Pack=2), у педантичных драйверов — на 4
                // (Reserved на 2). Печатаем оба сырых чтения, спорить
                // будем по факту.
                return "onevalue itemType " + itemType
                    + " | u32@2 " + ReadValue(p, 2, TwTy.UInt32)
                    + " | u32@4 " + ReadValue(p, 4, TwTy.UInt32);
            }
            if (containerType == TwOn.Enum)
            {
                ushort numItems = (ushort)Marshal.ReadInt16(p, 2);
                short current = (short)Marshal.ReadInt16(p, 4);
                if (numItems > 0 && current >= 0 && current < numItems)
                {
                    int itemSize = ItemSize(itemType);
                    int offset = 8 + (int)(current * itemSize);
                    return "enum[" + numItems + "]" + " itemType " + itemType
                        + " = value on current " + current + ": " + ReadValue(p, offset, itemType);
                }
                return "enum[" + numItems + "] itemType " + itemType + " (индекс " + current + " туповой)";
            }
            if (containerType == TwOn.Range)
            {
                return "range itemType " + itemType + " min " + Marshal.ReadInt32(p, 2)
                    + " max " + Marshal.ReadInt32(p, 6)
                    + " step " + Marshal.ReadInt32(p, 10)
                    + " current " + Marshal.ReadInt32(p, 18);
            }
            ushort count = (ushort)Marshal.ReadInt16(p, 2);
            return "container " + containerType + " itemType " + itemType + " элементы " + count;
        }

        private static int ItemSize(ushort itemType)
        {
            switch (itemType)
            {
                case TwTy.Int8:
                case TwTy.UInt8:
                    return 1;
                case TwTy.Int16:
                case TwTy.UInt16:
                case TwTy.Bool:
                    return 2;
                default:
                    return 4;
            }
        }

        private static double ReadValue(IntPtr c, int offset, ushort itemType)
        {
            switch (itemType)
            {
                case TwTy.Int8:
                case TwTy.UInt8:
                    return Marshal.ReadByte(c, offset);
                case TwTy.Int16:
                case TwTy.UInt16:
                case TwTy.Bool:
                    return Marshal.ReadInt16(c, offset);
                case TwTy.Fix32:
                    {
                        int raw = Marshal.ReadInt32(c, offset);
                        short whole = (short)(raw & 0xFFFF);
                        ushort frac = (ushort)((raw >> 16) & 0xFFFF);
                        return whole + frac / 65_536.0;
                    }
                default:
                    return Marshal.ReadInt32(c, offset);
            }
        }

        private sealed class HandshakeState
        {
            public bool Delivered;
        }

        private static void RunHandshake(LineLog log, bool newDsm, TWIdentity app, TWIdentity ds, HandshakeState state, Case caseInfo)
        {
            switch (caseInfo.Handshake)
            {
                case Case.Kind.ImageInfoThenPending:
                    ProcessImageInfo(log, newDsm, app, ds);
                    ProcessPendingXfers(log, newDsm, app, ds);
                    break;

                case Case.Kind.PendingOnly:
                    ProcessPendingXfers(log, newDsm, app, ds);
                    break;

                case Case.Kind.Direct:
                    break;
            }

            if (caseInfo.Memory)
                DoMemoryTransfer(log, newDsm, app, ds, state);
            else
                DoNativeTransfer(log, newDsm, app, ds, state);
        }

        private static void ProcessImageInfo(LineLog log, bool newDsm, TWIdentity app, TWIdentity ds)
        {
            var info = new TWImageInfo { BitsPerSample = new short[8] };
            var sw = Stopwatch.StartNew();
            log.Info("→ DAT_IMAGEINFO MSG_GET…");
            ushort rc = Dsm.Call(newDsm, app, ds, Dg.Image, Dat.ImageInfo, Msg.Get, info);
            log.Info("   {0} за {1:N0} мс: {2}×{3}, bpp {4}, spp {5}, фото {6}",
                TwRc.Name(rc), sw.ElapsedMilliseconds,
                info.ImageWidth, info.ImageLength, info.BitsPerPixel, info.SamplesPerPixel, info.Photometric);
        }

        private static void ProcessPendingXfers(LineLog log, bool newDsm, TWIdentity app, TWIdentity ds)
        {
            var pending = new TWPendingXfers();
            var sw = Stopwatch.StartNew();
            log.Info("→ DAT_PENDINGXFERS MSG_GET…");
            ushort rc = Dsm.Call(newDsm, app, ds, Dg.Control, Dat.PendingXfers, Msg.Get, pending);
            if (rc == TwRc.Success)
                log.Info("   {0} за {1:N0} мс: ожидают {2}", TwRc.Name(rc), sw.ElapsedMilliseconds, pending.Count);
            else
                log.Info("   {0} за {1:N0} мс", TwRc.Name(rc), sw.ElapsedMilliseconds);
        }

        private static void DoNativeTransfer(LineLog log, bool newDsm, TWIdentity app, TWIdentity ds, HandshakeState state)
        {
            var sw = Stopwatch.StartNew();
            log.Info("→ DAT_IMAGENATIVEXFER MSG_GET…");
            IntPtr h;
            ushort rc = Dsm.ImageNativeXfer(newDsm, app, ds, out h);
            if ((rc == TwRc.Success || rc == TwRc.XferDone) && h != IntPtr.Zero)
            {
                int biSize = Marshal.ReadInt32(h, 0);
                long biWidth = Marshal.ReadInt32(h, 4);
                long biHeight = Marshal.ReadInt32(h, 8);
                log.Info("   {0} за {1:N0} мс: DIB {2}×{3} (biSize {4}) — страница есть",
                    TwRc.Name(rc), sw.ElapsedMilliseconds, biWidth, Math.Abs(biHeight), biSize);
                Native32.GlobalFree(h);
                state.Delivered = true;
                return;
            }

            log.Info("   {0} за {1:N0} мс — страницы нет", TwRc.Name(rc), sw.ElapsedMilliseconds);
            LogStatus(log, newDsm, app, ds);
        }

        private static void DoMemoryTransfer(LineLog log, bool newDsm, TWIdentity app, TWIdentity ds, HandshakeState state)
        {
            const uint size = 1048576; // какой размер согласовали в SETUPMEMXFER
            IntPtr buf = Marshal.AllocHGlobal((int)size);
            try
            {
                var mx = new TWImageMemXfer
                {
                    Memory = new TWMemory { Flags = TwMf.AppOwns | TwMf.Pointer, Length = size, TheMem = buf }
                };

                var sw = Stopwatch.StartNew();
                log.Info("→ DAT_IMAGEMEMXFER MSG_GET (цикл до XFERDONE)…");
                long bytes = 0;
                int steps = 0;
                while (true)
                {
                    ushort rc = Dsm.Call(newDsm, app, ds, Dg.Image, Dat.ImageMemXfer, Msg.Get, mx);
                    if (rc == TwRc.XferDone)
                    {
                        bytes += mx.BytesWritten;
                        log.Info("   шаг {0}: {1} за {2:N0} мс: rows {3}, cols {4}, bytes {5} (всего {6})",
                            steps + 1, TwRc.Name(rc), sw.ElapsedMilliseconds, mx.Rows, mx.Columns, mx.BytesWritten, bytes);
                        log.Info("   страница готова за {0:N1} с, байт всего {1} (шагов {2})",
                            sw.Elapsed.TotalSeconds, bytes, steps + 1);
                        state.Delivered = true;
                        return;
                    }

                    if (rc == TwRc.Success && (mx.BytesWritten > 0 || mx.Rows > 0))
                    {
                        bytes += mx.BytesWritten;
                        if (steps < 4)
                            log.Info("   шаг {0}: {1} за {2:N0} мс: rows {3}, cols {4}, bytes {5} (всего {6})",
                                steps + 1, TwRc.Name(rc), sw.ElapsedMilliseconds, mx.Rows, mx.Columns, mx.BytesWritten, bytes);
                        else if (steps % 200 == 0)
                            log.Info("   шаг {0}: продолжается, всего байт {1}, за {2:N1} с",
                                steps + 1, bytes, sw.Elapsed.TotalSeconds);
                        ++steps;
                        continue;
                    }

                    log.Info("   шаг {0}: {1} за {2:N0} мс — передача не пошла (rows {3}, cols {4}, bytes {5})",
                        steps + 1, TwRc.Name(rc), sw.ElapsedMilliseconds, mx.Rows, mx.Columns, mx.BytesWritten);
                    LogStatus(log, newDsm, app, ds);
                    return;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buf);
            }
        }

        private static void LogStatus(LineLog log, bool newDsm, TWIdentity app, TWIdentity ds)
        {
            var status = new TWStatus();
            var rc = Dsm.Call(newDsm, app, ds, Dg.Control, Dat.Status, Msg.Get, status);
            if (rc == TwRc.Success)
                log.Info("   TW STATUS: код {0} ({1})", status.ConditionCode, TwCc.Name(status.ConditionCode));
        }

        private static bool WaitReady(bool newDsm, TWIdentity app, TWIdentity ds, PumpWindow pump, int limitMs)
        {
            int left = limitMs;
            const int slice = 25;
            while (left > 0)
            {
                if (Dsm.Messages.Wait(slice))
                    return true; // готовность через колбэк

                int step = Math.Min(slice, left);
                bool stop = false;
                pump.Drain(m =>
                {
                    // Каждое сообщение окна превращаем в DAT_EVENT PROC
                    // (эквивалент NTwain.IsTwainMessage): источник решает,
                    // относится ли это к TWAIN.
                    var raw = new WinMsg { HWnd = m.HWnd, Message = m.Message, WParam = m.WParam, LParam = m.LParam };
                    IntPtr evBuf = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WinMsg)));
                    try
                    {
                        Marshal.StructureToPtr(raw, evBuf, false);
                        var ev = new TWEvent { PEvent = evBuf };
                        ushort rc = Dsm.Call(newDsm, app, ds, Dg.Control, Dat.Event, Msg.ProcessEvent, ev);
                        if (rc == TwRc.DSEvent)
                        {
                            Dsm.Messages.Push(ev.TwMessage);
                            if (ev.TwMessage == Msg.XferReady || ev.TwMessage == Msg.CloseDSReq)
                                stop = true; // ход прерывается, стоп прокачки
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(evBuf);
                    }
                    return stop;
                });

                if (stop)
                    return true;

                left -= step;
            }

            return false;
        }
    }
}
