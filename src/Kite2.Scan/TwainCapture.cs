using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Kite2.ScanProtocol;
using NLog;
using Saraff.Twain;

namespace Kite2.Scan
{
    // Всё общение с драйвером живёт здесь, в отдельном 32-разрядном процессе.
    // Прерывания изнутри нет: драйвер умеет не возвращаться вообще, и ждать его с
    // таймаутом — значит прервать захват нельзя. Программа закрывает помощника, и
    // ожидание заканчивается вместе с ним.
    internal static class TwainCapture
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static List<string> GetSources()
        {
            var sources = new List<string>();

            using (var twain = new Twain32())
            {
                OpenSession(twain);

                try
                {
                    for (int i = 0; i < twain.SourcesCount; i++)
                        sources.Add(twain.GetSourceProductName(i));
                }
                finally
                {
                    twain.CloseDSM();
                }
            }

            return sources;
        }

        public static List<string> Capture(int sourceIndex, int dpi, int colour, string directory)
        {
            var pages = new List<string>();
            var clock = Stopwatch.StartNew();

            var twain = new Twain32
            {
                ShowUI = false,
                // Отключение после серии обязательно, а не на выбор: без него цикл
                // сообщений Saraff не заканчивается никогда — источник остаётся
                // активным, хотя отсканированы все страницы, и Acquire не возвращает
                // управление, пока кто-то не отключит источник.
                DisableAfterAcquire = true
            };

            try
            {
                OpenSession(twain);
                Log.Info("Диспетчер открыт за {0:N1} с", clock.Elapsed.TotalSeconds);

                try
                {
                    // Индекс принимается только у открытого диспетчера: до OpenDSM
                    // библиотека отвечает «менеджер источников данных не открыт».
                    twain.SourceIndex = sourceIndex;
                    twain.OpenDataSource();
                    Log.Info("Источник открыт: {0}", twain.GetSourceProductName(sourceIndex));

                    bool uiNeeded = ApplySettings(twain, dpi, colour);
                    twain.ShowUI = uiNeeded;

                    if (uiNeeded)
                    {
                        // Драйвер откажет и захвату целиком, если настройки не подошли:
                        // единственный способ договориться — его собственное окно.
                        Log.Info("Настройки не приняты, драйвер покажет своё окно");
                    }
                    else
                    {
                        Log.Info("Настройки приняты: dpi={0}, цвет={1}", dpi, colour == ScanExchange.BlackAndWhite ? "чёрно-белый" : "цветной");
                    }

                    var assembler = new PageAssembler(twain, directory, pages);

                    // Слушатели ставятся на все механизмы сразу: какой из них включится,
                    // решает драйвер своим ICAP_XFERMECH.
                    twain.SetupMemXferEvent += assembler.OnSetupMemXfer;
                    twain.MemXferEvent += assembler.OnMemXfer;
                    twain.EndXfer += assembler.OnEndXfer;
                    twain.FileXferEvent += assembler.OnFileXfer;
                    twain.XferDone += assembler.OnXferDone;

                    // Драйвер сводит свои отказы в это событие и без подписки они
                    // теряются: захват возвращается с нулём страниц без объяснений.
                    twain.AcquireError += (sender, args) =>
                        Log.Error("Драйвер сообщил об ошибке: {0}", args.Exception?.Message);

                    Log.Info("Захват передан драйверу, жду страницы или сигнал о конце серии");
                    twain.Acquire();
                    clock.Stop();
                    Log.Info("Драйвер вернул управление за {0:N1} с, страниц получено: {1}", clock.Elapsed.TotalSeconds, pages.Count);
                }
                finally
                {
                    twain.CloseDataSource();
                    Log.Info("Источник закрыт");
                }
            }
            finally
            {
                CloseQuietly(twain);
                Log.Info("Диспетчер закрыт");
            }

            return pages;
        }

        // Сначала старый twain_32, и это не ностальгия: драйвер Kyocera TWAIN(2)
        // на этой машине отдаёт страницы только через него (то же делает отдельный
        // инструмент прежней программы), а TWAIN 2 без обратной связи молчит намертво.
        // TWAIN 2 остаётся запасным для машин, где twain_32 не установлен вовсе.
        private static void OpenSession(Twain32 twain)
        {
            twain.IsTwain2Enable = false;

            if (twain.OpenDSM())
                return;

            try
            {
                twain.IsTwain2Enable = true;

                if (twain.OpenDSM())
                    return;
            }
            catch (Exception)
            {
                // Диспетчера TWAIN 2 на машине может не быть вовсе — остаётся запасной.
            }

            throw new InvalidOperationException(
                "Не удалось открыть менеджер TWAIN. Проверьте, установлен ли драйвер сканера.");
        }

        private static bool ApplySettings(Twain32 twain, int dpi, int colour)
        {
            // Механизм передачи задаётся принудительно, а не на выбор драйвера:
            // Kyocera отдаёт страницы только через передачу памятью (так делает и
            // работающий с этим драйвером инструмент прежней программы), а присвоить
            // механизм на усмотрение драйвера — значит получить захват без страниц.
            TrySet(() => twain.Capabilities.XferMech.Set(TwSX.Memory));

            LogTransferMechanism(twain);

            // Формат файла нужен только запасному файловому механизму: BMP остальным
            // пока понятен композиции страниц.
            TrySet(() => twain.Capabilities.ImageFileFormat.Set(TwFF.Bmp));

            bool uiNeeded = false;

            if (!TrySetResolution(twain, dpi))
                uiNeeded = true;

            TwPixelType pixelType = colour == ScanExchange.BlackAndWhite ? TwPixelType.BW : TwPixelType.RGB;

            if (!TrySetPixelType(twain, pixelType))
                uiNeeded = true;

            return uiNeeded;
        }

        private static void LogTransferMechanism(Twain32 twain)
        {
            try
            {
                Log.Info("Механизм передачи: {0}", twain.Capabilities.XferMech.GetCurrent());
            }
            catch (Exception ex)
            {
                Log.Warn("Механизм передачи не читается: {0}", ex.Message);
            }
        }

        // Значение ставится только из списка поддерживаемых и только с подтверждением:
        // установка наугад у драйвера не проваливается сразу, а ломает захват целиком.
        private static bool TrySetResolution(Twain32 twain, int dpi)
        {
            try
            {
                var enumeration = twain.Capabilities.XResolution.Get();

                for (int i = 0; i < enumeration.Count; i++)
                    if (dpi == (int)(float)enumeration[i])
                    {
                        twain.Capabilities.XResolution.Set((float)dpi);
                        twain.Capabilities.YResolution.Set((float)dpi);
                        break;
                    }

                return dpi == (int)twain.Capabilities.XResolution.GetCurrent()
                    && dpi == (int)twain.Capabilities.YResolution.GetCurrent();
            }
            catch (Exception ex)
            {
                Log.Warn("Разрешение не установлено: {0}", ex.Message);
                return false;
            }
        }

        private static bool TrySetPixelType(Twain32 twain, TwPixelType pixelType)
        {
            try
            {
                var enumeration = twain.Capabilities.PixelType.Get();

                for (int i = 0; i < enumeration.Count; i++)
                    if (pixelType == (TwPixelType)enumeration[i])
                    {
                        twain.Capabilities.PixelType.Set(pixelType);
                        break;
                    }

                return pixelType == (TwPixelType)twain.Capabilities.PixelType.GetCurrent();
            }
            catch (Exception ex)
            {
                Log.Warn("Тип пикселей не установлен: {0}", ex.Message);
                return false;
            }
        }

        // Возможности, которых нет у конкретного устройства, пропускаются.
        private static void TrySet(Action apply)
        {
            try
            {
                apply();
            }
            catch (Exception)
            {
                // Возможность у этого устройства есть не всякая, и это не сбой.
            }
        }

        private static void CloseQuietly(Twain32 twain)
        {
            try
            {
                twain.CloseDSM();
            }
            catch (Exception)
            {
                // Сессия могла закрыться сама — при отмене это обычный порядок вещей.
            }
        }
    }
}
