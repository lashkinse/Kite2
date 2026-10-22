// Свой клиент TWAIN для драйвера Kyocera.
//
// Зачем он нужен рядом с NAPS2: этот драйвер (Kyocera TWAIN(2), протокол 1.8)
// в тихом режиме сканирование не начинает — движок ждёт, пока человек нажмёт
// «Скан» в собственном окне драйвера. NAPS2 такого окна для TWAIN не
// показывает, поэтому для Kyocera захват ведёт этот клиент.
//
// Рецепт проверен обследователем на втором компьютере пользователя:
//   OPENDSM (родитель — наше скрытое окно верхнего уровня)
//   → OPENDS → регистрация DAT_CALLBACK2
//   → SET ICAP_XFERMECH = native (контейнер OneValue)
//   → ENABLEDS с ShowUI=1 (родное окно драйвера)
//   → ждём MSG_XFERREADY (приходит в колбэк диспетчера)
//   → DAT_IMAGEINFO → DAT_PENDINGXFERS → DAT_IMAGENATIVEXFER
//   → MSG_ENDXFER → MSG_DISABLEDS → MSG_CLOSEDS → MSG_CLOSEDSM.
//
// Важные подробности, найденные по следам:
//   * контейнеры кап пишутся как OneValue (в DSM 2.x OneValue = 5, Range = 6);
//     при значении 6 драйвер принимал буфер за TW_RANGE и писал в него мимо
//     границы — процесс падал;
//   * хэндл DIB приходит и при TWRC_XFERDONE, а не только при TWRC_SUCCESS;
//   * готовность приходит в колбэк, а не в DAT_EVENT: на ветке legacy диспетчер
//     отвечает DSEVENT с TwMessage=NULL почти на каждое оконное сообщение.
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using NLog;

namespace Kite2.Scan.Twain
{
    internal static class KyoceraTwainScanner
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // Имя источника по каталогу: если задан, берём именно его.
        private static string _wantedSource;

        // Страницы, полученные этим клиентом.
        public static System.Collections.Generic.List<string> Pages { get; } =
            new System.Collections.Generic.List<string>();

        /// <summary>
        /// Захват страницы. Без окна драйвера (тихий режим) сканер на этом
        /// драйвере часто не начинает сканирование; с окном (showUi = true)
        /// сканирование запускает человек кнопкой «Скан».
        /// Возвращает путь файла страницы (null — страницы не было).
        /// </summary>
        public static string Capture(string directory, string sourceName, TimeSpan wall, bool showUi)
        {
            _wantedSource = sourceName;
            string failure = null;

            string dllNote = Dsm.LoadLibraries();
            if (dllNote != null)
            {
                Log.Error("Диспетчер TWAIN не загрузился: {0}", dllNote);
                return null;
            }

            var pump = new PumpWindow(Log) { PumpAll = true };
            IntPtr hwnd = pump.Create();
            if (hwnd == IntPtr.Zero)
            {
                Log.Error("Окно-родитель не создалось — захват невозможен");
                return null;
            }

            var app = TWIdentity.App(true);
            var ds = TWIdentity.Empty();

            // 1. Диспетчер: в OPENDSM передаётся указатель на наше окно.
            ushort rc = CallWithHwnd(app, Msg.OpenDSM, hwnd);
            Log.Info("Kyocera: MSG_OPENDSM: {0}", TwRc.Name(rc));
            if (rc != TwRc.Success)
                return null;

            try
            {
                // 2. Источник по умолчанию (Kyocera TWAIN(2)).
                rc = Dsm.Call(true, app, null, Dg.Control, Dat.Identity, Msg.GetDefault, ds);
                if (rc != TwRc.Success)
                {
                    Log.Error("Kyocera: источник по умолчанию не получен: {0}", TwRc.Name(rc));
                    return null;
                }

                Log.Info(
                    "Kyocera: источник «{0}», протокол {1}.{2}",
                    ds.ProductName, ds.ProtocolMajor, ds.ProtocolMinor);

                // 3. Открыть источник.
                rc = Dsm.Call(true, app, null, Dg.Control, Dat.Identity, Msg.OpenDS, ds);
                Log.Info("Kyocera: MSG_OPENDS: {0}", TwRc.Name(rc));
                if (rc != TwRc.Success)
                    return null;

                // 4. Колбэк: в него диспетчер приносит MSG_XFERREADY.
                rc = Dsm.Call(true, app, ds, Dg.Control, Dat.Callback2, Msg.RegisterCallback,
                    new TWCallback2(Dsm.SharedCallback));
                Log.Info("Kyocera: регистрация DAT_CALLBACK2: {0}", TwRc.Name(rc));

                // 5. Механизм передачи — целиком (память драйвер не берёт:
                //    DAT_SETUPMEMXFER у него отвечает TWRC_FAILURE).
                SetXferMech(app, ds);

                // 6. Включение. С родным окном драйвера (ShowUI=1) сканирование
                //    запускает человек; в тихом режиме (ShowUI=0) драйвер на
                //    Kyocera часто молчит — тогда попытка уходит по стенке.
                var ui = new TWUserInterface
                {
                    ShowUI = (ushort)(showUi ? 1 : 0),
                    ModalUI = 0,
                    HParent = hwnd
                };
                var sw = Stopwatch.StartNew();
                rc = Dsm.Call(true, app, ds, Dg.Control, Dat.UserInterface, Msg.EnableDS, ui);
                Log.Info(
                    "Kyocera: MSG_ENABLEDS (ShowUI={0}): {1} за {2:N0} мс",
                    showUi ? 1 : 0, TwRc.Name(rc), sw.ElapsedMilliseconds);
                if (rc != TwRc.Success)
                    return null;

                // 7. Ждём готовности: колбэк диспетчера или канал legacy-DSM.
                if (!WaitReady(app, ds, pump, wall))
                {
                    Log.Warn("Kyocera: готовность не пришла за {0:N0} с", wall.TotalSeconds);
                    failure = "Драйвер не сообщил о готовности сканирования.";
                    return null;
                }

                // 8. Подготовка и передача.
                var info = new TWImageInfo { BitsPerSample = new short[8] };
                rc = Dsm.Call(true, app, ds, Dg.Image, Dat.ImageInfo, Msg.Get, info);
                Log.Info(
                    "Kyocera: DAT_IMAGEINFO: {0}, {1}×{2}, {3} бит/пиксель",
                    TwRc.Name(rc), info.ImageWidth, info.ImageLength, info.BitsPerPixel);

                var pending = new TWPendingXfers();
                rc = Dsm.Call(true, app, ds, Dg.Control, Dat.PendingXfers, Msg.Get, pending);
                Log.Info("Kyocera: DAT_PENDINGXFERS: {0}, ожидают {1}", TwRc.Name(rc), pending.Count);

                IntPtr handle;
                sw.Restart();
                rc = Dsm.ImageNativeXfer(true, app, ds, out handle);
                Log.Info("Kyocera: DAT_IMAGENATIVEXFER: {0} за {1:N0} мс", TwRc.Name(rc), sw.ElapsedMilliseconds);

                if ((rc == TwRc.Success || rc == TwRc.XferDone) && handle != IntPtr.Zero)
                {
                    string path = SavePage(directory, handle);
                    if (path != null)
                    {
                        Pages.Add(path);
                        Log.Info("Kyocera: страница сохранена: {0}", path);
                        failure = null;
                    }
                    else
                    {
                        failure = "Страницу не удалось сохранить на диск.";
                    }
                }
                else
                {
                    failure = "Драйвер не отдал изображение страницы.";
                    Log.Warn("Kyocera: {0}", failure);
                }

                return failure == null ? Pages[Pages.Count - 1] : null;
            }
            finally
            {
                // Уборка: закрыть источник и диспетчер, чтобы драйвер не остался
                // включённым (иначе следующая попытка стартует с заблокированного
                // состояния).
                try
                {
                    Dsm.Call(true, app, ds, Dg.Control, Dat.UserInterface, Msg.DisableDS, new TWUserInterface());
                    Dsm.Call(true, app, null, Dg.Control, Dat.Identity, Msg.CloseDS, ds);
                    CallWithHwnd(app, Msg.CloseDSM, IntPtr.Zero);
                }
                catch (Exception ex)
                {
                    Log.Warn("Kyocera: уборка не удалась: {0}", ex.Message);
                }
            }
        }

        // Триплет, в котором данные — указатель на наше окно-родитель.
        private static ushort CallWithHwnd(TWIdentity app, ushort msg, IntPtr hwnd)
        {
            IntPtr holder = Marshal.AllocHGlobal(8);
            Marshal.WriteIntPtr(holder, hwnd);
            try
            {
                return Dsm.CallPtr(true, app, null, Dg.Control, Dat.Parent, msg, holder);
            }
            finally
            {
                Marshal.FreeHGlobal(holder);
            }
        }

        // Ставит ICAP_XFERMECH = native контейнером OneValue (шесть байт:
        // тип ushort в нуле, значение uint в двух).
        private static void SetXferMech(TWIdentity app, TWIdentity ds)
        {
            IntPtr h = WinMem.Alloc(6);
            try
            {
                IntPtr p = Native32.GlobalLock(h);
                if (p == IntPtr.Zero)
                {
                    Log.Warn("Kyocera: контейнер для ICAP_XFERMECH не создался");
                    return;
                }

                Marshal.WriteInt16(p, 0, (short)TwTy.UInt16);
                Marshal.WriteInt32(p, 2, Twsx.Native);
                Native32.GlobalUnlock(h);

                ushort rc = Dsm.Call(true, app, ds, Dg.Control, Dat.Capability, Msg.Set,
                    new TWCapability(CapId.ICapXferMech, TwOn.OneValue, h));
                Log.Info("Kyocera: ICAP_XFERMECH = native: {0}", TwRc.Name(rc));
            }
            finally
            {
                WinMem.Free(h);
            }
        }

        // Ожидание готовности. Готовность приходит в колбэк диспетчера
        // (DAT_CALLBACK2 → MSG_XFERREADY). Дополнительно слушаем канал
        // legacy-DSM «DSMAPPMESSAGE32»: старый twain_32.dll шлёт через него
        // то же самое, и на части машин приходит только он.
        private static bool WaitReady(TWIdentity app, TWIdentity ds, PumpWindow pump, TimeSpan wall)
        {
            uint channel = 0;
            try
            {
                channel = Native32.RegisterWindowMessageA("DSMAPPMESSAGE32");
            }
            catch
            {
                // Канала нет — работаем только с колбэком.
            }

            DateTime deadline = DateTime.UtcNow.Add(wall);

            while (DateTime.UtcNow < deadline)
            {
                if (Dsm.Messages.Wait(50))
                {
                    Log.Info("Kyocera: готовность (XferReady) в колбэке диспетчера");
                    return true;
                }

                pump.Drain(m =>
                {
                    if (channel != 0 && m.Message == channel)
                    {
                        Log.Info("Kyocera: готовность в канале DSMAPPMESSAGE32");
                        return true;
                    }
                    return false;
                });
            }

            return false;
        }

        // Сохранение полученного DIB в файл страницы серии.
        private static string SavePage(string directory, IntPtr handle)
        {
            IntPtr dib = Native32.GlobalLock(handle);
            if (dib == IntPtr.Zero)
            {
                Log.Warn("Kyocera: хэндл {0:X} не открылся", handle.ToInt64());
                return null;
            }

            try
            {
                int biSize = Marshal.ReadInt32(dib, 0);
                int width = Marshal.ReadInt32(dib, 4);
                int height = Marshal.ReadInt32(dib, 8);
                int bpp = Marshal.ReadInt16(dib, 14);
                if (biSize <= 0 || width <= 0 || height == 0)
                {
                    Log.Warn("Kyocera: DIB выглядит пустым (размер {0}, {1}×{2})", biSize, width, height);
                    return null;
                }

                int stride = ((width * bpp + 31) / 32) * 4;
                int total = biSize + Math.Abs(height) * stride;

                Directory.CreateDirectory(directory);
                string path = Path.Combine(
                    directory,
                    "scan" + (Pages.Count + 1).ToString("D3") + ".bmp");

                using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                using (var w = new BinaryWriter(fs))
                {
                    w.Write((byte)'B');
                    w.Write((byte)'M');
                    w.Write(14 + total);
                    w.Write((short)0);
                    w.Write((short)0);
                    w.Write(14 + biSize);

                    var buf = new byte[total];
                    Marshal.Copy(dib, buf, 0, total);
                    w.Write(buf);
                }

                Log.Info(
                    "Kyocera: DIB {0}×{1}, {2} бит, {3:N0} байт", width, Math.Abs(height), bpp, total);

                return path;
            }
            catch (Exception ex)
            {
                Log.Warn("Kyocera: страницу сохранить не удалось: {0}", ex.Message);
                return null;
            }
            finally
            {
                Native32.GlobalUnlock(handle);
                try { Native32.GlobalFree(handle); } catch { }
            }
        }
    }
}
