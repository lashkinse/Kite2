// Собственно обследование: один процесс — один сценарий. Хроника агрессивно
// честная: ПЕРЕД каждым триплетом в журнал пишется «→ DAT_…», поэтому после
// повода видно, в каком нативном вызове ребёнок висит навсегда.
using System;
using System.Diagnostics;
using System.IO;
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

        // Что происходит между листами серии (сценарии 11–16):
        // Plain — ничего, сырой ENDXFER (baseline) или MSG_RESET по Strat;
        // AcceptMid — вызов экспорта KmUsbScnAccept ПОСЛЕ листа 1;
        // AcceptEarly — вызов KmUsbScnAccept СРАЗУ после ENABLEDS;
        // PokeMode — прямая запись 2 в g_nCurrentMode (DAT_100210f4);
        // ResetMsg — DAT_PENDINGXFERS MSG_RESET вместо ENDXFER.
        public enum Strategy
        {
            Plain = 0,
            AcceptMid = 1,
            AcceptEarly = 2,
            PokeMode = 3,
            ResetMsg = 4
        }

        public int Index;
        public string Title;
        public bool NewDsm;
        public bool Memory;
        public Kind Handshake;
        public bool ShowUi;   // ENABLEDS с ShowUI=1 (родной диалог драйвера)
        public int Sheets;    // сколько листов тянуть в одной серии (1 — как раньше)
        public Strategy Strat;
        public bool ReSession;  // сценарий 20: своя сессия DS на каждый лист
        public int EndXferCount; // что писать в Count у TWPendingXfers между листами
    }

    internal static class Cases
    {
        private const int Total = 22;

        // Прогон (run): сценарий 21 — пассивная пачка. НИ ОДНОГО ENDXFER
        // между листами: после передачи листа-application ждёт XFERREADY
        // от драйвера (фантомы Ready устранены Reset-ом), затем
        // NATIVEXFER следующего листа. ENDXFER — один, в уборке Play.
        // Готовность приходит сама → клиент 1.7.38 реализуем по этой
        // схеме; тишина → на TWAIN пачки нет и поднимаем вопрос WIA.
        internal static readonly int[] Battery = { 22 };

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
                case 10:
                    return Make(true, false, Case.Kind.ImageInfoThenPending,
                        "Native: ShowUI=1 (диалог драйвера), IMAGEINFO → PENDINGXFERS → NATIVEXFER",
                        showUi: true);
                case 11:
                    return Make(true, false, Case.Kind.ImageInfoThenPending,
                        "Native: KmUsbScnAccept после листа 1, между листами ENDXFER (пачка до 6)",
                        Case.Strategy.AcceptMid, 6);
                case 13:
                    return Make(true, false, Case.Kind.ImageInfoThenPending,
                        "Native: пок g_nCurrentMode=2 после листа 1, между листами ENDXFER (пачка до 6)",
                        Case.Strategy.PokeMode, 6);
                case 14:
                    return Make(true, false, Case.Kind.ImageInfoThenPending,
                        "Native: baseline — ENDXFER между листами без ACCEPT (пачка до 6)",
                        Case.Strategy.Plain, 6);
                case 16:
                    return Make(true, false, Case.Kind.ImageInfoThenPending,
                        "Native: ShowUI=1 (нажмите «Скан» в диалоге), KmUsbScnAccept после листа 1, ENDXFER между листами, мини-дамп",
                        Case.Strategy.AcceptMid, 6, true);
                case 21:
                    return Make(true, false, Case.Kind.ImageInfoThenPending,
                        "Native: ShowUI=1 («Скан»), ACCEPT после листа 1, далее пассивная пачка: XFERREADY сам → NATIVEXFER, без ENDXFER",
                        Case.Strategy.AcceptMid, 8, true);
                case 22:
                    // Проверка тихой пачки: окно не открываем, драйвер стартует
                    // сам (в прогоне 1.7.40 XferReady пришёл без кнопки через
                    // 14 с). Если после Accept в тихом сеансе приходят честные
                    // готовности следующих листов — пачка собирается без окна.
                    // Податчик загрузить 3–4 листами, НЕ нажимать ничего.
                    return Make(true, false, Case.Kind.ImageInfoThenPending,
                        "Native: ShowUI=0 (без окна, драйвер стартует сам), ACCEPT после листа 1, далее пассивная пачка",
                        Case.Strategy.AcceptMid, 8, false);
                default:
                    return null;
            }
        }

        private static Case Make(bool newDsm, bool memory, Case.Kind handshake, string title,
            Case.Strategy strat = Case.Strategy.Plain, int sheets = 1, bool showUi = false,
            int endXferCount = 1, bool reSession = false)
        {
            return new Case
            {
                NewDsm = newDsm,
                Memory = memory,
                Handshake = handshake,
                Title = title,
                ShowUi = showUi,
                Strat = strat,
                Sheets = sheets,
                EndXferCount = endXferCount,
                ReSession = reSession
            };
        }

        internal static int Count
        {
            get { return Total; }
        }
    }

    internal static class Experiment
    {
        // Путь журнала и номер сценария — нужны, чтобы сохранить полученную
        // страницу рядом с журналом. PageSold — сколько страниц получено
        // за всю серию (для имени файлов pageXX_NN.bmp и вердикта).
        internal static string LogPath = string.Empty;
        internal static int LogIndex;
        internal static int PageSold;

        // Аргументы exp: idx, wallMs, logPath; необязательный фильтр источника.
        public static int Run(string[] args)
        {
            int idx = int.Parse(args[1]);
            int wall = int.Parse(args[2]);
            string logPath = args[3];
            string filter = args.Length > 4 ? args[4] : string.Empty;
            LogPath = logPath;
            LogIndex = idx;
            PageSold = 0;

            // Мини-дамп при нативном падении: без него о смерти в коде
            // драйвера остаётся только код выхода (0xC000041D и подобие).
            try
            {
                string dir = Path.GetDirectoryName(Path.GetFullPath(logPath));
                MiniDump.Install(dir, "exp" + idx.ToString("00"));
            }
            catch { }

            var caseInfo = Cases.Get(idx);
            if (caseInfo == null)
                return 4;

            var log = new LineLog(logPath);
            // Ловим необработанные сбои управляемого кода: иначе ребёнок
            // молча умирает, и в отчёте остаётся только «код выхода».
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                try
                {
                    var ex = e.ExceptionObject as Exception;
                    log.Info("НЕОБРАБОТАННЫЙ СБОЙ: {0}", ex != null ? ex.ToString() : e.ExceptionObject);
                    if (ex != null && ex.InnerException != null)
                        log.Info("внутренний сбой: {0}", ex.InnerException);
                }
                catch { }
            };
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

            // Канал legacy-DSM: регистрация того же имени, каким старый
            // twain_32.dll шлёт XferReady/ClosedDsReq приложению.
            uint dsmAppMsg = 0;
            try { dsmAppMsg = Native32.RegisterWindowMessageA("DSMAPPMESSAGE32"); }
            catch { }
            if (dsmAppMsg != 0)
                log.Info("канал legacy-DSM «DSMAPPMESSAGE32»: 0x{0:X}", dsmAppMsg);
            else
                log.Info("канал legacy-DSM «DSMAPPMESSAGE32»: НЕ ЗАРЕГИСТРИРОВАН");

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
                // Податчик первым: новый драйвер в тихом режиме молчит
                // без него (REPORT 10-23 — четыре сессии без готовности).
                SetFeeder(log, caseInfo.NewDsm, app, ds);
                SetXferMech(log, caseInfo.NewDsm, app, ds, caseInfo.Memory ? Twsx.Memory : Twsx.Native);
                // CAP_XFERCOUNT драйвер Kyocera не принимает (TWRC_FAILURE) —
                // оставляем его собственный счётчик, а не ломаем включение.
                if (caseInfo.Memory)
                {
                    var setup = new TWSetupMemXfer { MinBufSize = 8192, MaxBufSize = 1048576, DesiredBytes = 1048576 };
                    rc = Dsm.Call(caseInfo.NewDsm, app, ds, Dg.Control, Dat.SetupMemXfer, Msg.Set, setup);
                    log.Info("DAT_SETUPMEMXFER SET (min {0}, max {1}): {2}",
                        setup.MinBufSize, setup.MaxBufSize, TwRc.Name(rc));
                    if (rc != TwRc.Success)
                    {
                        LogStatus(log, caseInfo.NewDsm, app, ds);
                        log.Info("   (продолжаем с буфером 1 МБ — драйвер справится по своему дефолту)");
                    }
                }

                var state = new HandshakeState();
                try
                {
                    // 7. Enable. Родитель настоящий, ShowUI — по варианту
                    //    (родной диалог драйвера в exp10, PumpAll для его помпы).
                    pump.PumpAll = caseInfo.ShowUi;
                    var ui = new TWUserInterface { ShowUI = (ushort)(caseInfo.ShowUi ? 1 : 0), ModalUI = 0, HParent = hwnd };
                    sw.Restart();
                    rc = Dsm.Call(caseInfo.NewDsm, app, ds, Dg.Control, Dat.UserInterface, Msg.EnableDS, ui);
                    log.Info("ENABLEDS (ShowUI={0}, родитель {1}): {2} за {3:N0} мс",
                        caseInfo.ShowUi ? 1 : 0, hwnd, TwRc.Name(rc), sw.ElapsedMilliseconds);
                    if (rc != TwRc.Success)
                    {
                        LogStatus(log, caseInfo.NewDsm, app, ds);
                        return 2;
                    }

                    // 7б. Повторное чтение капы после ENABLEDS (состояние 5) —
                    //     сравнить с дожавшимися нулями из состояния 4.
                    ReadCap(log, caseInfo.NewDsm, app, ds, "после ENABLEDS ICAP_XFERMECH", CapId.ICapXferMech);

                    // 7в. Сценарий 12: вызов экспорта KmUsbScnAccept сразу
                    //     после ENABLEDS — включение ACCEPT для самой первой
                    //     страницы, ровно как делает диалог драйвера на «Скан».
                    if (caseInfo.Strat == Case.Strategy.AcceptEarly)
                        EarlyAccept(log);

                    // 8. Готовность.
                    sw.Restart();
                    int left = wall - (int)sw.ElapsedMilliseconds;
                    bool ok = WaitReady(caseInfo.NewDsm, app, ds, pump, log, Math.Max(15000, left), dsmAppMsg);
                    log.Info("итог по оконным сообщениям: {0}", pump.NoiseSummary());
                    if (!ok)
                    {
                        log.Info("XferReady не пришёл за {0:N1} с", sw.Elapsed.TotalSeconds);
                        return 2;
                    }
                    log.Info("XferReady через {0:N1} с после ENABLEDS", sw.Elapsed.TotalSeconds);

                    // 9. Стержень: последовательность триплетов после XferReady.
                    RunHandshake(log, caseInfo.NewDsm, app, ds, state, caseInfo);

                    // 9б. Серийные сценарии 11–16: попытка вытянуть следующие
                    //     листы из той же сессии по схеме, выбранной в Strat.
                    if (state.Delivered && caseInfo.Sheets > 1)
                        RunBatch(log, caseInfo, caseInfo.NewDsm, app, ds, state, pump, sw, wall, dsmAppMsg);
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

                return PageSold > 0 ? 0 : 1;
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

        // Чтение капс «на этапе 4» — ключевые без балласта.
        private static void WatchCaps(LineLog log, bool newDsm, TWIdentity app, TWIdentity ds)
        {
            ReadCap(log, newDsm, app, ds, "ICAP_XFERMECH", CapId.ICapXferMech);
            ReadCap(log, newDsm, app, ds, "ICAP_XRESOLUTION", CapId.ICapXResolution);
            ReadCap(log, newDsm, app, ds, "ICAP_BITDEPTH", CapId.ICapBitDepth);
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
                    log.Info("{0}: сырой контейнер: {1}", name, DumpBytes(p, 48));
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

        // Сырые байты контейнера: показывает, действительно ли диспетчер
        // положил значения в наш буфер или оставил его пустым.
        private static string DumpBytes(IntPtr p, int count)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < count; i += 16)
            {
                sb.Append("  +").Append(i.ToString("00")).Append(": ");
                for (int k = 0; k < 16; k++)
                {
                    byte b = Marshal.ReadByte(p, i + k);
                    sb.Append(b.ToString("X2")).Append(' ');
                }
            }
            return sb.ToString();
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

        private static void SetXferCount(LineLog log, bool newDsm, TWIdentity app, TWIdentity ds)
        {
            // CAP_XFERCOUNT = 1 (TWTY_INT16) — ровно то, что NAPS2/NTwain
            // делает перед ENABLEDS. OneValue-контейнер 6 байт (Pack=2).
            IntPtr h = WinMem.Alloc(6);
            try
            {
                IntPtr p = Native32.GlobalLock(h);
                if (p == IntPtr.Zero)
                {
                    log.Info("CAP_XFERCOUNT = 1: GlobalLock не удался");
                    return;
                }
                Marshal.WriteInt16(p, 0, (short)TwTy.Int16);
                Marshal.WriteInt32(p, 2, 1);
                Native32.GlobalUnlock(h);

                var rc = Dsm.Call(newDsm, app, ds, Dg.Control, Dat.Capability, Msg.Set,
                    new TWCapability(CapId.CapXferCount, TwOn.OneValue, h));
                log.Info("CAP_XFERCOUNT = 1: SET {0}", TwRc.Name(rc));
                if (rc == TwRc.Success)
                    ReadCurrent(log, newDsm, app, ds, "CAP_XFERCOUNT (после SET)", CapId.CapXferCount);
            }
            finally
            {
                WinMem.Free(h);
            }
        }

        private static void SetFeeder(LineLog log, bool newDsm, TWIdentity app, TWIdentity ds)
        {
            // CAP_FEEDERENABLED = 1 (TWTY_INT16/bool) перед ENABLEDS: на
            // новом драйвере тихая сессия не выходит на готовность, пока
            // податчик выключен (REPORT 10-23: CAP_FEEDERENABLED = 0,
            // XferReady не пришёл ни за 300 с в четырёх тихих сценариях).
            IntPtr h = WinMem.Alloc(6);
            try
            {
                IntPtr p = Native32.GlobalLock(h);
                if (p == IntPtr.Zero)
                {
                    log.Info("CAP_FEEDERENABLED = 1: GlobalLock не удался");
                    return;
                }
                Marshal.WriteInt16(p, 0, (short)TwTy.Int16);
                Marshal.WriteInt32(p, 2, 1);
                Native32.GlobalUnlock(h);

                var rc = Dsm.Call(newDsm, app, ds, Dg.Control, Dat.Capability, Msg.Set,
                    new TWCapability(CapId.CapFeederEnabled, TwOn.OneValue, h));
                log.Info("CAP_FEEDERENABLED = 1: SET {0}", TwRc.Name(rc));
                if (rc == TwRc.Success)
                    ReadCurrent(log, newDsm, app, ds, "CAP_FEEDERENABLED (после SET)", CapId.CapFeederEnabled);
            }
            finally
            {
                WinMem.Free(h);
            }
        }

        private static void ReadCurrent(LineLog log, bool newDsm, TWIdentity app, TWIdentity ds, string name, ushort cap)
        {
            IntPtr h = WinMem.Alloc(64);
            try
            {
                var capObj = new TWCapability(cap, TwOn.DontCare16, h);
                var rc = Dsm.Call(newDsm, app, ds, Dg.Control, Dat.Capability, Msg.GetCurrent, capObj);
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
                IntPtr p = Native32.GlobalLock(h);
                if (p == IntPtr.Zero)
                {
                    log.Info("   {0} за {1:N0} мс: хэндл {2:X} есть, но GlobalLock не удался",
                        TwRc.Name(rc), sw.ElapsedMilliseconds, h.ToInt64());
                    LogStatus(log, newDsm, app, ds);
                    return;
                }
                int biSize = Marshal.ReadInt32(p, 0);
                int biWidth = Marshal.ReadInt32(p, 4);
                int biHeight = Marshal.ReadInt32(p, 8);
                ushort biBitCount = (ushort)Marshal.ReadInt16(p, 14);
                log.Info("   {0} за {1:N0} мс: DIB {2}×{3} (biSize {4}, бит {5}) — страница есть",
                    TwRc.Name(rc), sw.ElapsedMilliseconds, biWidth, Math.Abs(biHeight), biSize, biBitCount);
                SaveBmp(log, h, p, biSize, biWidth, biHeight);
                Native32.GlobalUnlock(h);
                try { Native32.GlobalFree(h); } catch { }
                state.Delivered = true;
                return;
            }

            log.Info("   {0} за {1:N0} мс — страницы нет (хэндл {2})",
                TwRc.Name(rc), sw.ElapsedMilliseconds, h.ToInt64());
            LogStatus(log, newDsm, app, ds);
        }

        // Сохраняем полученный DIB в файл рядом с журналом: это главное
        // доказательство, что канал передачи работает.
        private static void SaveBmp(LineLog log, IntPtr handle, IntPtr dib, int biSize, int width, int height)
        {
            try
            {
                int bpp = 24;
                try { bpp = Marshal.ReadInt16(dib, 14); } catch { }
                int stride = ((width * bpp + 31) / 32) * 4;
                int pixels = Math.Abs(height) * stride;
                int total = biSize + pixels;

                string dir = Path.GetDirectoryName(LogPath);
                int page = ++PageSold;
                string path = Path.Combine(dir ?? ".", "page" + LogIndex.ToString("00")
                    + "_" + page.ToString("00") + ".bmp");

                using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                using (var w = new BinaryWriter(fs))
                {
                    w.Write((byte)'B');
                    w.Write((byte)'M');
                    w.Write(14 + total);
                    w.Write((short)0);
                    w.Write((short)0);
                    w.Write(14 + biSize);

                    var buf = new byte[Math.Min(total, 64 * 1024 * 1024)];
                    Marshal.Copy(dib, buf, 0, total);
                    w.Write(buf);
                }
                log.Info("   страница сохранена: {0} ({1:N0} байт)", path, (long)total);
            }
            catch (Exception ex)
            {
                log.Info("   страницу сохранить не удалось: {0}", ex.Message);
            }
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

        private static string MessageName(ushort msg)
        {
            switch (msg)
            {
                case Msg.Null: return "NULL";
                case Msg.XferReady: return "XFERREADY(257)";
                case Msg.CloseDSReq: return "CLOSEDSREQ(258)";
                case Msg.CloseDSOK: return "CLOSEDSOK(259)";
                case Msg.CloseDSCancel: return "CLOSEDSCANCEL(260)";
                default: return "неведомый " + msg;
            }
        }

        // Сценарий 12: включение ACCEPT из приложения, как только
        // KMUSBSCN3.dll появился в процессе (сразу после ENABLEDS).
        private static void EarlyAccept(LineLog log)
        {
            IntPtr dll = KmScn.GetModule(log, 2000);
            if (dll == IntPtr.Zero)
            {
                log.Info("KMUSBSCN3.dll после ENABLEDS не загрузился — ранний Accept пропущен");
                return;
            }
            KmScn.LogState(log, "ранний Accept: до вызова");
            EnsureMode2(log, viaPoke: false, dll);
            KmScn.LogState(log, "ранний Accept: после вызова");
        }

        // Доводим g_nCurrentMode до 2 (SCAN_MODE_ACCEPT): экспортом
        // KmUsbScnAccept с картой параметров (верный путь UI) или прямой
        // записью в глобал (пок, сценарий 13). Профиль RVAs — по штампу PE.
        private static void EnsureMode2(LineLog log, bool viaPoke, IntPtr dll)
        {
            var prof = KmScn.Detect(dll, log);
            if (prof == null)
            {
                log.Info("профиль RVAs не определён — вмешательство в режим пропущено");
                return;
            }
            int mode = Marshal.ReadInt32(dll, prof.Mode);
            log.Info("g_nCurrentMode = {0}", mode);
            if (mode == 2)
            {
                log.Info("режим уже ACCEPT — вмешательство не нужно");
                return;
            }

            if (viaPoke)
            {
                Marshal.WriteInt32(dll, prof.Mode, 2);
                log.Info("пок: g_nCurrentMode ← 2; читаем назад = {0}", Marshal.ReadInt32(dll, prof.Mode));
                return;
            }

            int[] p = KmScn.ReadParams(dll, prof);
            log.Info("карта параметров: {0}",
                string.Join(",", Array.ConvertAll(p, v => "0x" + v.ToString("X"))));
            int ret = KmScn.CallAccept(dll, prof, p, log);
            log.Info("KmUsbScnAccept → {0}; g_nCurrentMode теперь = {1}",
                ret, Marshal.ReadInt32(dll, prof.Mode));
        }

        // Серийный цикл сценария 21 (пассивная пачка): БЕЗ ENDXFER между
        // листами. После передачи листа ждём настоящий XFERREADY от
        // драйвера (_READY теперь Reset после чтения — фантомов нет),
        // статус каждые 5 с. Готовность → NATIVEXFER следующего листа.
        // Тишина до конца повода = драйвер не тянет пачку сам.
        private static void RunBatch(LineLog log, Case caseInfo, bool newDsm, TWIdentity app, TWIdentity ds,
            HandshakeState state, PumpWindow pump, Stopwatch started, int wall, uint dsmAppMsg)
        {
            // Режим ACCEPT после листа 1: механика подачи следующего листа
            // (NextPageReq) живёт в нём.
            if (caseInfo.Strat == Case.Strategy.AcceptMid)
            {
                IntPtr dll0 = KmScn.GetModule(log, 0);
                if (dll0 != IntPtr.Zero)
                    EnsureMode2(log, caseInfo.Strat == Case.Strategy.PokeMode, dll0);
            }

            for (int n = 2; n <= caseInfo.Sheets; n++)
            {
                if (!state.Delivered)
                    break;

                log.Info("── лист {0}: пассивное ожидание готовности (без ENDXFER) ──", n);
                var callSw = Stopwatch.StartNew();
                int waitLimit = Math.Max(5000, wall - (int)started.ElapsedMilliseconds);
                bool ok = false;
                int waited = 0;
                int slice = 500;
                int lastStat = 0;
                while (waited < waitLimit)
                {
                    int step = Math.Min(slice, waitLimit - waited);
                    if (WaitReady(newDsm, app, ds, pump, log, step, dsmAppMsg))
                    {
                        ok = true;
                        break;
                    }
                    waited += step;
                    if (waited - lastStat >= 5000)
                    {
                        KmScn.LogState(log, "пассивное ожидание " + (waited / 1000) + " с");
                        lastStat = waited;
                    }
                }
                log.Info("лист {0}: готовность {1} через {2:N1} с ожидания (всего от старта {3:N1} с)",
                    n, ok ? "ЕСТЬ (драйвер тянет пачку сам!)" : "не пришла — стоп",
                    callSw.Elapsed.TotalSeconds, started.Elapsed.TotalSeconds);
                if (!ok)
                    break;

                // Передача листа сразу, без предварительных GET.
                if (caseInfo.Memory)
                    DoMemoryTransfer(log, newDsm, app, ds, state);
                else
                    DoNativeTransfer(log, newDsm, app, ds, state);
                if (!state.Delivered)
                {
                    log.Info("лист {0}: передача не удалась — стоп", n);
                    break;
                }
            }

            log.Info("итог серии: сохранено страниц {0}", PageSold);
        }

        private static bool WaitReady(bool newDsm, TWIdentity app, TWIdentity ds, PumpWindow pump, LineLog log, int limitMs, uint dsmAppMsg)
        {
            int left = limitMs;
            const int slice = 25;
            while (left > 0)
            {
                if (Dsm.Messages.Wait(slice))
                {
                    Dsm.Messages.Reset(); // сигнал потреблён — фантома не оставить
                    return true; // готовность через колбэк
                }

                int step = Math.Min(slice, left);
                bool stop = false;
                pump.Drain(m =>
                {
                    // Канал legacy-DSM (Microsoft InboxTwainDsm): DSMAPPMESSAGE32
                    // с GlobalAlloc-хэндлом блока из 5 dword в wParam. Наш PROC
                    // на этот канал у нового DSM отвечает TWRC_NOTDSEVENT —
                    // сообщение принадлежит twain_32.dll. Поэтому расшифровываем
                    // блок сами и считаем готовность без PROC.
                    bool channel = dsmAppMsg != 0 && m.Message == dsmAppMsg;
                    if (channel)
                    {
                        log.Info("КАНАЛ DSMAPPMESSAGE32: wp 0x{0:X} lp 0x{1:X}", m.WParam.ToInt64(), m.LParam.ToInt64());
                        if (m.WParam != IntPtr.Zero)
                        {
                            IntPtr p = Native32.GlobalLock(m.WParam);
                            if (p != IntPtr.Zero)
                            {
                                // Нужно успеть до PROC: старый DSM после PROC
                                // освобождает блок (CB_AdvDataHandler передаёт
                                // владение приложению, PROC его съедает).
                                int a = Marshal.ReadInt32(p, 0);
                                int b = Marshal.ReadInt32(p, 4);
                                int c = Marshal.ReadInt32(p, 8);
                                int d = Marshal.ReadInt32(p, 12);
                                int e = Marshal.ReadInt32(p, 16);
                                Native32.GlobalUnlock(m.WParam);
                                log.Info("payload: [{0}, {1}, {2}, {3}, {4}]", a, b, c, d, e);
                                if (a == 257)
                                    log.Info("payload[0] = 257 (MSG_XFERREADY) — канал подтверждён напрямую");
                            }
                            else
                            {
                                log.Info("GlobalLock(wParam) не удался (код {0})",
                                    Marshal.GetLastWin32Error());
                            }
                        }
                        // Сами считаем это готовностью: в эксперименте 5 (v1.0.6)
                        // PROC отвечал NOTDSEVENT, а канал был настоящим.
                        stop = true;
                    }

                    // Каждое сообщение окна превращаем в DAT_EVENT PROC
                    // (эквивалент NTwain.IsTwainMessage): источник решает,
                    // относится ли это к TWAIN. Результат печатаем честно —
                    // это единственный способ увидеть, признал ли драйвер
                    // «своим» поступившее сообщение.
                    var raw = new WinMsg { HWnd = m.HWnd, Message = m.Message, WParam = m.WParam, LParam = m.LParam };
                    IntPtr evBuf = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WinMsg)));
                    try
                    {
                        Marshal.StructureToPtr(raw, evBuf, false);
                        var ev = new TWEvent { PEvent = evBuf };
                        ushort rc = Dsm.Call(newDsm, app, ds, Dg.Control, Dat.Event, Msg.ProcessEvent, ev);
                        // DSM на ветке legacy отвечает DSEVENT с TwMessage=NULL
                        // практически на каждое оконное сообщение. Готовностью
                        // считаем только XferReady/CloseDSReq — иначе пробник
                        // уходил в передачу в неправильном состоянии.
                        bool ready = rc == TwRc.DSEvent
                            && (ev.TwMessage == Msg.XferReady || ev.TwMessage == Msg.CloseDSReq);
                        if (pump.AllowLog(m.Message))
                        {
                            string verdict = rc == TwRc.DSEvent
                                ? "DSEVENT, TwMessage=" + MessageName(ev.TwMessage)
                                : TwRc.Name(rc);
                            log.Info("EVENT PROC msg 0x{0:X4}: {1}", m.Message, verdict);
                        }
                        if (ready)
                        {
                            Dsm.Messages.Push(ev.TwMessage);
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
                {
                    Dsm.Messages.Reset(); // сигнал потреблён
                    return true;
                }

                left -= step;
            }

            return false;
        }
    }
}