// Тонкий слой TWAIN для обследователя: константы, структуры и точка входа
// DSM_Entry. Раскладка структур повторяет вендоренный NTwain (тот бинарь,
// что работает рядом с NAPS2.Sdk и живой связкой подтверждён с драйвером
// Kyocera): LayoutKind.Sequential + Pack = 2, строки ByValTStr 34,
// TW_MEMORY с флагами AppOwns | Pointer. Менять «по вкусу» нельзя.
using System;
using System.Runtime.InteropServices;

namespace Kite2.Probe
{
    internal static class Dg
    {
        public const ushort Control = 1;
        public const ushort Image = 2;
    }

    internal static class Dat
    {
        public const ushort Capability = 1;
        public const ushort Event = 2;
        public const ushort Identity = 3;
        public const ushort Parent = 4;
        public const ushort PendingXfers = 5;
        public const ushort SetupMemXfer = 6;
        public const ushort Status = 8;
        public const ushort UserInterface = 9;
        public const ushort XferGroup = 10;
        public const ushort Callback = 16;
        public const ushort Callback2 = 18;
        public const ushort ImageInfo = 257;
        public const ushort ImageMemXfer = 259;
        public const ushort ImageNativeXfer = 260;
    }

    internal static class Msg
    {
        public const ushort Null = 0;
        public const ushort Get = 1;
        public const ushort GetCurrent = 2;
        public const ushort GetDefault = 3;
        public const ushort GetFirst = 4;
        public const ushort GetNext = 5;
        public const ushort Set = 6;
        public const ushort QuerySupport = 8;
        public const ushort XferReady = 257;
        public const ushort CloseDSReq = 258;
        public const ushort CloseDSOK = 259;
        public const ushort CloseDSCancel = 260;
        public const ushort OpenDSM = 769;
        public const ushort CloseDSM = 770;
        public const ushort OpenDS = 1025;
        public const ushort CloseDS = 1026;
        public const ushort DisableDS = 1281;
        public const ushort EnableDS = 1282;
        public const ushort ProcessEvent = 1537;
        public const ushort EndXfer = 1793;
        public const ushort RegisterCallback = 2306;
    }

    internal static class TwRc
    {
        public const ushort Success = 0;
        public const ushort Failure = 1;
        public const ushort CheckStatus = 2;
        public const ushort Cancel = 3;
        public const ushort DSEvent = 4;
        public const ushort XferDone = 6;
        public const ushort EndOfList = 7;
        public const ushort InfoNotSupported = 8;
        public const ushort DataNotAvailable = 9;
        public const ushort Busy = 10;

        public static string Name(ushort rc)
        {
            switch (rc)
            {
                case Success: return "TWRC_SUCCESS";
                case Failure: return "TWRC_FAILURE";
                case CheckStatus: return "TWRC_CHECKSTATUS";
                case Cancel: return "TWRC_CANCEL";
                case DSEvent: return "TWRC_DSEVENT";
                case XferDone: return "TWRC_XFERDONE";
                case EndOfList: return "TWRC_ENDOFLIST";
                case InfoNotSupported: return "TWRC_INFO_NOT_SUPPORTED";
                case DataNotAvailable: return "TWRC_DATA_NOT_AVAILABLE";
                case Busy: return "TWRC_BUSY";
                default: return "TWRC " + rc.ToString("X4");
            }
        }
    }

    internal static class TwCc
    {
        // Частые коды состояния; остальное печатается сырым числом.
        public static string Name(ushort cc)
        {
            switch (cc)
            {
                case 0: return "TWCC_SUCCESS";
                case 1: return "TWCC_BUMMER";
                case 2: return "TWCC_NOMEM";
                case 3: return "TWCC_NODS";
                case 4: return "TWCC_MAXCONNECTIONS";
                case 5: return "TWCC_CAPUNSUPPORTED";
                case 6: return "TWCC_CAPBADOPERATION";
                case 7: return "TWCC_CAPBADVALUE";
                case 8: return "TWCC_CAPSEQUENCEERROR";
                case 9: return "TWCC_CAPBADDESTINATION";
                case 10: return "TWCC_CAPSUNSUPPORTED";
                case 11: return "TWCC_BADPROTOCOL";
                case 12: return "TWCC_BADVALUE";
                case 13: return "TWCC_BADDESTINATION";
                case 14: return "TWCC_NOSUPPORT";
                case 15: return "TWCC_OPERATIONERROR";
                case 16: return "TWCC_DEVICEERROR";
                case 17: return "TWCC_SUCCESS";
                case 18: return "TWCC_SEQERROR";
                case 19: return "TWCC_BADDEST";
                case 20: return "TWCC_CAPSEQERROR";
                default: return "TWCC " + cc.ToString("X4");
            }
        }
    }

    internal static class CapId
    {
        public const ushort CapXferCount = 1;
        public const ushort ICapCompression = 256;
        public const ushort ICapPixelType = 257;
        public const ushort ICapUnits = 258;
        public const ushort ICapXferMech = 259;
        public const ushort CapFeederEnabled = 4098;
        public const ushort ICapXResolution = 4376;
        public const ushort ICapYResolution = 4377;
        public const ushort ICapSupportedSizes = 4386;
        public const ushort ICapThreshold = 4387;
        public const ushort ICapBitDepth = 4395;   // не 4387 — это ICapThreshold
    }

    internal static class TwTy
    {
        public const ushort Int8 = 0;
        public const ushort Int16 = 1;
        public const ushort Int32 = 2;
        public const ushort UInt8 = 3;
        public const ushort UInt16 = 4;
        public const ushort UInt32 = 5;
        public const ushort Bool = 6;
        public const ushort Fix32 = 8;
    }

    internal static class TwOn
    {
        // Значения TWON в том виде, в каком их понимает наш DSM (2.x) и
        // вендоренный NTwain: Array=3, Enum=4, OneValue=5, Range=6.
        // Раньше здесь стояло OneValue=6 — драйвер принимал наш контейнер
        // за TW_RANGE и писал в него 22 байта вместо 6 (порча кучи).
        public const ushort Array = 3;
        public const ushort Enum = 4;
        public const ushort OneValue = 5;
        public const ushort Range = 6;
        public const ushort DontCare8 = 255;
        public const ushort DontCare16 = 65535;
    }

    internal static class TwMf
    {
        public const uint AppOwns = 1;
        public const uint Pointer = 8;
        public const uint Handle = 0x10;
    }

    internal static class Twsx
    {
        public const ushort Native = 0;
        public const ushort File = 1;
        public const ushort Memory = 2;
    }

    // ───────────────── структуры ─────────────────

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct TWVersion
    {
        public ushort MajorNum;
        public ushort MinorNum;
        public ushort Language;
        public ushort Country;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]
        public string Info;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class TWIdentity
    {
        public uint Id;
        public TWVersion Version;
        public ushort ProtocolMajor;
        public ushort ProtocolMinor;
        public uint SupportedGroups;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]
        public string Manufacturer;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]
        public string ProductFamily;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]
        public string ProductName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 34)]
        public string Protocol;

        public static TWIdentity App(bool newDsm)
        {
            // Как вендоренный NTwain: одна идентичность на оба диспетчера —
            // протокол 2.3 и бит DF_APP2 в SupportedGroups; строки тихие
            // (не заполнены), драйверы воспринимают её одинаково хорошо.
            return new TWIdentity
            {
                Id = 0,
                Version = new TWVersion
                {
                    MajorNum = 1,
                    MinorNum = 0,
                    Language = 0,
                    Country = 0x0E7,
                    Info = "Kite2.Probe"
                },
                ProtocolMajor = 2,
                ProtocolMinor = 3,
                SupportedGroups = 0x20000000u | Dg.Control | Dg.Image,
                Manufacturer = "Kite2",
                ProductFamily = "Kite2",
                ProductName = "Kite2.Probe",
                Protocol = string.Empty
            };
        }

        public static TWIdentity Empty()
        {
            var id = App(true);
            id.Id = 0;
            id.Manufacturer = string.Empty;
            id.ProductFamily = string.Empty;
            id.ProductName = string.Empty;
            id.Protocol = string.Empty;
            id.Version.Info = string.Empty;
            return id;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class TWCapability
    {
        public ushort Cap;
        public ushort ConType;
        public IntPtr HContainer;

        public TWCapability(ushort cap, ushort conType, IntPtr container)
        {
            Cap = cap;
            ConType = conType;
            HContainer = container;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class TWImageInfo
    {
        public int XResolution;
        public int YResolution;
        public int ImageWidth;
        public int ImageLength;
        public ushort SamplesPerPixel;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public short[] BitsPerSample;

        public ushort BitsPerPixel;
        public ushort Planar;
        public ushort CompressionEnum;
        public ushort Photometric;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class TWPendingXfers
    {
        public ushort Count = ushort.MaxValue;
        public uint Eoj;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class TWSetupMemXfer
    {
        public uint MinBufSize;
        public uint MaxBufSize;
        public uint DesiredBytes;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct TWMemory
    {
        public uint Flags;
        public uint Length;
        public IntPtr TheMem;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class TWImageMemXfer
    {
        public ushort Compression;
        public ushort BytesPerRow;
        public uint Columns;
        public uint Rows;
        public uint XOffset;
        public uint YOffset;
        public uint BytesWritten;
        public TWMemory Memory;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class TWUserInterface
    {
        public ushort ShowUI;
        public ushort ModalUI;
        public IntPtr HParent;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class TWStatus
    {
        public ushort ConditionCode;
        public ushort Reserved;
    }

    // TW_EVENT: pEvent ввода + ответ источника числом (TWMessage).
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public class TWEvent
    {
        public IntPtr PEvent;
        public ushort TwMessage;
    }

    // Форма Windows MSG — как и NTwain.MESSAGE.
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct WinMsg
    {
        public IntPtr HWnd;
        public uint Message;
        public IntPtr WParam;
        public IntPtr LParam;
        public uint Time;
        public int X;
        public int Y;
    }

    public delegate ushort TwainCallback(IntPtr origin, IntPtr dest, uint dg, ushort dat, ushort msg, IntPtr data);

    // TW_CALLBACK2 вендоренного NTwain: proc + refCon + message.
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TWCallback2
    {
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public TwainCallback Proc;

        public UIntPtr RefCon;
        public short Message;

        public TWCallback2(TwainCallback callback)
        {
            Proc = callback;
        }
    }

    // TW_CALLBACK (TWAIN 1.9): proc + clientData.
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal class TWCallback
    {
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public TwainCallback Proc;

        public UIntPtr ClientData;

        public TWCallback(TwainCallback callback)
        {
            Proc = callback;
        }
    }

    internal static class Dsm
    {
        private static IntPtr _new2;
        private static IntPtr _old1;

        // Общий приёмник колбэка источника: возвращает SUCCESS и толкает
        // сообщение в Ready из другого потока (наш вызов возвращает сразу).
        public static readonly TwainCallback SharedCallback = OnSourceMsg;

        private static readonly Ready ReadyEvents = new Ready();

        public static Ready Messages => ReadyEvents;

        // twaindsm.dll лежит рядом с программой; twain_32.dll — в папке
        // Windows (на 32-битном процессе system32 видит 32-битную версию).
        public static string LoadLibraries()
        {
            _new2 = Native32.LoadLibrary(
                AppDomain.CurrentDomain.BaseDirectory + "twaindsm.dll");
            _old1 = Native32.LoadLibrary(
                Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\twain_32.dll");
            if (_new2 == IntPtr.Zero && _old1 == IntPtr.Zero)
                return "ни twaindsm.dll, ни twain_32.dll не загрузились";

            return null;
        }

        public static bool NewAvailable => _new2 != IntPtr.Zero;
        public static bool OldAvailable => _old1 != IntPtr.Zero;

        private static ushort OnSourceMsg(IntPtr origin, IntPtr dest, uint dg, ushort dat, ushort msg, IntPtr data)
        {
            ReadyEvents.Push(msg);
            return TwRc.Success;
        }

        [DllImport("twaindsm.dll", EntryPoint = "DSM_Entry", SetLastError = false, CharSet = CharSet.Unicode)]
        private static extern ushort Raw2(
            [In][Out] TWIdentity origin,
            [In][Out] TWIdentity dest,
            ushort dg, ushort dat, ushort msg, IntPtr pData);

        [DllImport("twain_32.dll", EntryPoint = "DSM_Entry", SetLastError = false, CharSet = CharSet.Unicode)]
        private static extern ushort Raw1(
            [In][Out] TWIdentity origin,
            [In][Out] TWIdentity dest,
            ushort dg, ushort dat, ushort msg, IntPtr pData);

        // Триплет с классовым аргументом: снимок в буфер до вызова,
        // копируем изменённое поле обратно после.
        public static ushort Call<T>(bool useNew, TWIdentity app, TWIdentity dest, ushort dg, ushort dat, ushort msg, T data)
            where T : class
        {
            int size = Marshal.SizeOf(typeof(T));
            IntPtr m = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(data, m, false);
                ushort rc = useNew
                    ? Raw2(app, dest, dg, dat, msg, m)
                    : Raw1(app, dest, dg, dat, msg, m);
                Marshal.PtrToStructure(m, data);
                return rc;
            }
            finally
            {
                Marshal.FreeHGlobal(m);
            }
        }

        // Триплет с IntPtr на данные (глобальные контейнеры, дескрипторы окон).
        public static ushort CallPtr(bool useNew, TWIdentity app, TWIdentity dest, ushort dg, ushort dat, ushort msg, IntPtr pData)
        {
            return useNew
                ? Raw2(app, dest, dg, dat, msg, pData)
                : Raw1(app, dest, dg, dat, msg, pData);
        }

        // DAT_IMAGENATIVEXFER / MSG_GET: драйвер пишет HGLOBAL по pData.
        // Хэндл приходит и при TWRC_SUCCESS, и при TWRC_XFERDONE («передача
        // завершена») — раньше мы читали его только при SUCCESS и выбрасывали
        // готовую страницу.
        public static ushort ImageNativeXfer(bool useNew, TWIdentity app, TWIdentity ds, out IntPtr handle)
        {
            IntPtr m = Marshal.AllocHGlobal(8);
            Marshal.WriteIntPtr(m, IntPtr.Zero);
            try
            {
                ushort rc = useNew
                    ? Raw2(app, ds, Dg.Image, Dat.ImageNativeXfer, Msg.Get, m)
                    : Raw1(app, ds, Dg.Image, Dat.ImageNativeXfer, Msg.Get, m);
                handle = (rc == TwRc.Success || rc == TwRc.XferDone)
                    ? Marshal.ReadIntPtr(m)
                    : IntPtr.Zero;
                return rc;
            }
            finally
            {
                Marshal.FreeHGlobal(m);
            }
        }
    }

    internal class Ready
    {
        private readonly System.Threading.ManualResetEventSlim _go =
            new System.Threading.ManualResetEventSlim(false);
        private readonly object _l = new object();
        private ushort _last;

        public void Push(ushort msg)
        {
            lock (_l)
            {
                _last = msg;
                if (msg == Msg.XferReady)
                    _go.Set();
            }
        }

        public ushort Last
        {
            get
            {
                lock (_l) { return _last; }
            }
        }

        public bool Wait(int ms)
        {
            return _go.Wait(ms);
        }
    }

    internal static class Native32
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string path);

        // Контейнеры капов — GlobalAlloc-хэндлы (в чистом виде копия
        // WinMemoryManager вендоренного NTwain): флаг 64 = GMEM_ZEROINIT
        // без MOVEABLE, т.е. фиксированный блок, чей «хэндл» совпадает
        // с адресом; 1.x-драйверы читают/пишут их через GlobalLock, поэтому
        // сырой AllocHGlobal-указатель наполнить мусором — не выйдет.
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GlobalAlloc(uint flags, UIntPtr bytes);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GlobalLock(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GlobalUnlock(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void GlobalFree(IntPtr h);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern ushort RegisterClassExW(ref WndClass cls);

        // Канал legacy-DSM: Microsoft InboxTwainDsm регистрирует
        // «DSMAPPMESSAGE32» (строковый ресурс twain_32.dll №103) и постит
        // окну приложения PostMessageA(hwnd, msgId, hMem=GlobalAlloc(0x14), 0)
        // при получении DDE-advise (TwDdeCli::CB_AdvDataHandler).
        // А/у формы дают одинаковый идентификатор для одного имени.
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        public static extern uint RegisterWindowMessageA(string name);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateWindowExW(
            uint exStyle,
            [MarshalAs(UnmanagedType.LPWStr)] string cls,
            [MarshalAs(UnmanagedType.LPWStr)] string title,
            uint style,
            int x, int y, int w, int h,
            IntPtr parent, IntPtr menu, IntPtr inst, IntPtr param);

        [DllImport("user32.dll")]
        public static extern IntPtr DefWindowProcW(IntPtr hwnd, uint msg, IntPtr wp, IntPtr lp);

        public static readonly IntPtr HwndMessage = new IntPtr(-3);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool TranslateMessage(ref MinimumMsg msg);

        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessage(ref MinimumMsg msg);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PeekMessageW(
            out MinimumMsg msg, IntPtr hwnd, uint filterMin, uint filterMax, uint flags);

        [StructLayout(LayoutKind.Sequential)]
        public struct MinimumMsg
        {
            public IntPtr HWnd;
            public uint Message;
            public IntPtr WParam;
            public IntPtr LParam;
            public uint Time;
            public int X;
            public int Y;
        }

        public delegate IntPtr WndProc(IntPtr hwnd, uint msg, IntPtr wp, IntPtr lp);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WndClass
        {
            public uint CbSize;
            public uint Style;
            public WndProc Proc;
            public int CbClsExtra;
            public int CbWndExtra;
            public IntPtr Instance;
            public IntPtr Icon;
            public IntPtr Cursor;
            public IntPtr Back;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Menu;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Name;
            public IntPtr IconSm;
        }
    }

    // Обёртка над GlobalAlloc-контейнерами: Alloc → Lock → правка → Unlock → Free.
    internal static class WinMem
    {
        public const uint ZeroInit = 64;

        public static IntPtr Alloc(uint size)
        {
            return Native32.GlobalAlloc(ZeroInit, new UIntPtr(size));
        }

        public static void Free(IntPtr h)
        {
            if (h != IntPtr.Zero) Native32.GlobalFree(h);
        }
    }
}
