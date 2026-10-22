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
//   → ENABLEDS с ShowUI=0 (тихий режим работает, окно драйвера не нужно)
//   → ждём MSG_XFERREADY (приходит в колбэк диспетчера)
//   → DAT_IMAGEINFO → DAT_PENDINGXFERS → DAT_IMAGENATIVEXFER
//   → снова ждём готовность и повторяем передачу, пока податчик не опустеет
//   → MSG_DISABLEDS. Закрытие источника (MSG_CLOSEDS)
//     на этом драйвере рвёт процесс, поэтому его не вызываем вовсе.
//
// Важные подробности, найденные по следам:
//   * контейнеры кап пишутся как OneValue (в DSM 2.x OneValue = 5, Range = 6);
//     при значении 6 драйвер принимал буфер за TW_RANGE и писал в него мимо
//     границы — процесс падал;
//   * хэндл DIB приходит и при TWRC_XFERDONE, а не только при TWRC_SUCCESS;
//   * готовность приходит в колбэк, а не в DAT_EVENT: на ветке legacy диспетчер
//     отвечает DSEVENT с TwMessage=NULL почти на каждое оконное сообщение;
//   * податчик опустошается только в той же сессии: оборвать сессию после
//     первой страницы значит оставить следующий лист застрявшим — драйвер
//     держит его и в новой серии молчит.
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

        // Открытый источник и диспетчер: нужны для уборки после записи ответа.
        private static TWIdentity _app;
        private static TWIdentity _ds;

        // Страницы, полученные этим клиентом.
        public static System.Collections.Generic.List<string> Pages { get; } =
            new System.Collections.Generic.List<string>();

        /// <summary>
        /// Захват страницы. Без окна драйвера (тихий режим) сканер на этом
        /// драйвере умеет и сам (проверено: готовность приходит через 12 с,
        /// DAT_IMAGEINFO отдаёт 2480×3507); с окном (showUi = true)
        /// сканирование запускает человек кнопкой «Скан».
        /// skipPending = true - пропустить DAT_PENDINGXFERS: на этом драйвере
        /// звонок иногда срывает процесс, а картинка приходит и без него.
        /// Возвращает путь файла страницы (null — страницы не было).
        /// </summary>
        public static string Capture(string directory, string sourceName, TimeSpan wall, bool showUi, bool skipPending)
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

                // Запомним для уборки: она пойдёт после записи ответа.
                _app = app;
                _ds = ds;

                // 4. Колбэк: в него диспетчер приносит MSG_XFERREADY.
                rc = Dsm.Call(true, app, ds, Dg.Control, Dat.Callback2, Msg.RegisterCallback,
                    new TWCallback2(Dsm.SharedCallback));
                Log.Info("Kyocera: регистрация DAT_CALLBACK2: {0}", TwRc.Name(rc));

                // 5. Опрос возможностей и группы передачи. Обследователь шёл
                //    ровно так: без этих звонков мост legacy-диспетчера к
                //    драйверу Kyocera работает иначе (в приложении на этом
                //    месте затем падал DAT_PENDINGXFERS).
                ReadCaps(app, ds);
                ReadXferGroup(app, ds);

                // 6. Механизм передачи — целиком (память драйвер не берёт:
                //    DAT_SETUPMEMXFER у него отвечает TWRC_FAILURE).
                SetXferMech(app, ds);

                // 7. Включение. С родным окном драйвера (ShowUI=1) сканирование
                //    запускает человек; в тихом режиме (ShowUI=0) драйвер на
                //    Kyocera тоже умеет сам (проверено: готовность пришла через
                //    12 с, DAT_IMAGEINFO отдал 2480×3507).
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

                // 8. Повторное чтение возможностей после включения — так же,
                //    как в обследователе.
                ReadOneCap(app, ds, "ICAP_XFERMECH (после включения)", CapId.ICapXferMech);

                // 9. Ждём готовности: колбэк диспетчера или канал legacy-DSM.
                DateTime deadline = DateTime.UtcNow.Add(wall);

                if (!WaitReady(app, ds, pump, wall))
                {
                    Log.Warn("Kyocera: готовность не пришла за {0:N0} с", wall.TotalSeconds);
                    failure = "Драйвер не сообщил о готовности сканирования.";
                    return null;
                }

                // 10. Передача листов. Первый лист идёт ровно по рецепту
                //     обследователя, следующие — той же сессией и только по
                //     готовности: после передачи любые вопросы драйверу
                //     (счётчик передач) на ветке состояния 7 висят намертво
                //     и убивают серию — проверено журналом, родитель ждал 45
                //     секунд вмороженного звонка. Поэтому между листами мы
                //     молчим и ждём только событие MSG_XFERREADY.
                while (true)
                {
                    if (Pages.Count > 0)
                    {
                        // Лист податчика кормится быстро (готовность доходила
                        // по 12-14 с), стекло нового листа не даёт: не
                        // дождались — в податчике пусто, сессия кончилась.
                        TimeSpan gap = TimeSpan.FromSeconds(30);

                        if (TimeSpan.Zero < deadline - DateTime.UtcNow)
                            gap = Min(gap, deadline - DateTime.UtcNow);
                        else
                            break;

                        if (!WaitReady(app, ds, pump, gap))
                        {
                            Log.Info(
                                "Kyocera: следующей готовности не дождались, страниц {0}",
                                Pages.Count);
                            break;
                        }
                    }

                    if (!TransferOne(app, ds, directory, skipPending, ref failure))
                        break;
                }

                // Уже полученные страницы ценнее текста о неудаче: отдаём их,
                // даже если следующий лист сорвался.
                return Pages.Count > 0 ? Pages[Pages.Count - 1] : null;
            }
            finally
            {
                // Уборку источника делаем НЕ здесь: на этом драйвере
                // MSG_CLOSEDS срывает процесс, а ответ родителю должен быть
                // записан раньше. Поэтому уборка вынесена в Close() и идёт
                // после записи ответа.
            }
        }

        // Меньшее из двух времён.
        private static TimeSpan Min(TimeSpan one, TimeSpan other) =>
            one < other ? one : other;

        // Передача одного листа полным рецептом: DAT_IMAGEINFO,
        // DAT_PENDINGXFERS (если не пропущен) и DAT_IMAGENATIVEXFER.
        // true — страница сохранена и числится в Pages.
        private static bool TransferOne(
            TWIdentity app, TWIdentity ds, string directory, bool skipPending, ref string failure)
        {
            IntPtr handle;
            ushort rc;

            try
            {
                var info = new TWImageInfo { BitsPerSample = new short[8] };
                rc = Dsm.Call(true, app, ds, Dg.Image, Dat.ImageInfo, Msg.Get, info);
                Log.Info(
                    "Kyocera: DAT_IMAGEINFO: {0}, {1}×{2}, {3} бит/пиксель",
                    TwRc.Name(rc), info.ImageWidth, info.ImageLength, info.BitsPerPixel);

                // Счётчик спрашивать не обязательно, а на этом драйвере звонок
                // иногда срывает процесс. Первый лист идёт по рецепту
                // обследователя (звонок до передачи — в состоянии 6 — безвреден);
                // на следующих листах картинка уже в состоянии 7, где вопросы
                // висят намертво, — идём сразу к картинке.
                if (!skipPending && Pages.Count == 0)
                    PendingKnown(app, ds, out _);

                var sw = Stopwatch.StartNew();
                rc = Dsm.ImageNativeXfer(true, app, ds, out handle);
                Log.Info("Kyocera: DAT_IMAGENATIVEXFER: {0} за {1:N0} мс", TwRc.Name(rc), sw.ElapsedMilliseconds);
            }
            catch (Exception error)
            {
                Log.Error("Kyocera: передача сорвалась: {0}", error.Message);
                failure = "Драйвер оборвал передачу изображения.";
                return false;
            }

            if ((rc == TwRc.Success || rc == TwRc.XferDone) && handle != IntPtr.Zero)
            {
                string path = SavePage(directory, handle);
                if (path != null)
                {
                    Pages.Add(path);
                    Log.Info("Kyocera: страница сохранена: {0}", path);
                    return true;
                }

                failure = "Страницу не удалось сохранить на диск.";
                return false;
            }

            failure = "Драйвер не отдал изображение страницы.";
            Log.Warn("Kyocera: {0}", failure);
            return false;
        }

        // Счётчик ожидающих передач. true — вызов прошёл и известно, есть ли
        // ещё листы в податчике (в stillPending).
        private static bool PendingKnown(TWIdentity app, TWIdentity ds, out bool stillPending)
        {
            stillPending = true;

            try
            {
                var pending = new TWPendingXfers();
                ushort rc = Dsm.Call(true, app, ds, Dg.Control, Dat.PendingXfers, Msg.Get, pending);
                stillPending = pending.Count != 0;
                Log.Info("Kyocera: DAT_PENDINGXFERS: {0}, ожидают {1}", TwRc.Name(rc), pending.Count);
                return true;
            }
            catch (Exception error)
            {
                Log.Warn("Kyocera: DAT_PENDINGXFERS сорвал процесс ({0}) - идём по готовности", error.Message);
                return false;
            }
        }

        // Закрыть источник и диспетчер. Вызывается после того, как ответ
        // родителю уже записан: MSG_CLOSEDS на этом драйвере рвёт процесс
        // (сбой доступа к памяти внутри DSM_Entry), поэтому закрытие не
        // вызываем вовсе. Процесс и так завершается сразу после: Windows
        // выгружает драйвер вместе с процессом и освобождает его поименованный
        // мьютекс, а MSG_DISABLEDS заранее останавливает двигатель сканера.
        public static void Close()
        {
            if (_app == null || _ds == null)
                return;

            try
            {
                Dsm.Call(true, _app, _ds, Dg.Control, Dat.UserInterface, Msg.DisableDS, new TWUserInterface());
                Log.Info("Kyocera: источник отключён; закрытие не вызываем — оно рвёт процесс");
            }
            catch (Exception ex)
            {
                Log.Warn("Kyocera: отключение источника не удалось: {0}", ex.Message);
            }
            finally
            {
                _app = null;
                _ds = null;
            }
        }

        // Опрос возможностей источника. Контейнеры — GlobalAlloc-хэндлы
        // (GMEM_ZEROINIT), как в обследователе: мост legacy-диспетчера пишет
        // в них через GlobalLock. Значения у этого драйвера не читаются
        // (диспетчер отдаёт пустые контейнеры), но сам опрос нужен: без него
        // дальнейшие звонки идут по другой ветке состояния.
        private static void ReadCaps(TWIdentity app, TWIdentity ds)
        {
            ReadOneCap(app, ds, "ICAP_XFERMECH", CapId.ICapXferMech);
            ReadOneCap(app, ds, "ICAP_XRESOLUTION", CapId.ICapXResolution);
            ReadOneCap(app, ds, "ICAP_BITDEPTH", CapId.ICapBitDepth);
            ReadOneCap(app, ds, "CAP_FEEDERENABLED", CapId.CapFeederEnabled);
        }

        private static void ReadOneCap(TWIdentity app, TWIdentity ds, string name, ushort cap)
        {
            IntPtr h = WinMem.Alloc(256);
            try
            {
                var capability = new TWCapability(cap, TwOn.DontCare16, h);
                ushort rc = Dsm.Call(true, app, ds, Dg.Control, Dat.Capability, Msg.Get, capability);
                Log.Info("Kyocera: {0}: {1}", name, TwRc.Name(rc));
            }
            finally
            {
                WinMem.Free(h);
            }
        }

        // Группа передачи: в обследователе её опрос шёл до включения.
        private static void ReadXferGroup(TWIdentity app, TWIdentity ds)
        {
            IntPtr m = Marshal.AllocHGlobal(4);
            Marshal.WriteInt32(m, 0);
            try
            {
                ushort rc = Dsm.CallPtr(true, app, ds, Dg.Control, Dat.XferGroup, Msg.Get, m);
                Log.Info("Kyocera: DAT_XFERGROUP: {0} (0x{1:X})", TwRc.Name(rc), Marshal.ReadInt32(m));
            }
            finally
            {
                Marshal.FreeHGlobal(m);
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
                    // Каждое оконное сообщение отдаём диспетчеру через
                    // DAT_EVENT/MSG_PROCESSEVENT — так же, как в обследователе:
                    // на legacy-ветке диспетчер отвечает на это DSEVENT с
                    // TwMessage=NULL, и без этих звонков его состояние идёт
                    // иначе, чем в проверенной последовательности.
                    ProcessEvent(app, ds, m);

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

        // Передать оконное сообщение диспетчеру (DAT_EVENT/MSG_PROCESSEVENT).
        private static void ProcessEvent(TWIdentity app, TWIdentity ds, Native32.MinimumMsg m)
        {
            try
            {
                var raw = new WinMsg
                {
                    HWnd = m.HWnd,
                    Message = m.Message,
                    WParam = m.WParam,
                    LParam = m.LParam
                };

                IntPtr buffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WinMsg)));
                try
                {
                    Marshal.StructureToPtr(raw, buffer, false);
                    Dsm.Call(true, app, ds, Dg.Control, Dat.Event, Msg.ProcessEvent,
                        new TWEvent { PEvent = buffer });
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
            catch (Exception error)
            {
                Log.Debug("Kyocera: DAT_EVENT не прошёл: {0}", error.Message);
            }
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
