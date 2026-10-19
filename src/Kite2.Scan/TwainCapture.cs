using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
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
                ModalUI = false,
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

                    ApplySettings(twain, dpi, colour);
                    Log.Info("Настройки применены: dpi={0}, цвет={1}", dpi, colour == ScanExchange.BlackAndWhite ? "чёрно-белый" : "цветной");

                    int index = 0;

                    twain.EndXfer += (sender, args) =>
                    {
                        string path = Path.Combine(
                            directory,
                            "scan" + (++index).ToString("D3") + ".bmp");

                        Save(args.Image, path);

                        pages.Add(path);

                        Log.Info("Страница {0} за {1:N1} с: {2}", index, clock.Elapsed.TotalSeconds, path);
                    };

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

        // Отсутствие сканера — не ошибка, а отсутствие устройств: драйвер
        // недоступен, значит возвращается пустой список.
        private static void OpenSession(Twain32 twain)
        {
            if (!twain.OpenDSM())
                throw new InvalidOperationException(
                    "Не удалось открыть менеджер TWAIN. Проверьте, установлен ли драйвер сканера.");
        }

        // Возможности, которых нет у конкретного устройства, пропускаются: у сканера
        // без автоподачи нет счётчика страниц, и из-за него захват срывался целиком.
        private static void ApplySettings(Twain32 twain, int dpi, int colour)
        {
            TwPixelType pixelType = colour == ScanExchange.BlackAndWhite ? TwPixelType.BW : TwPixelType.RGB;

            var capabilities = twain.Capabilities;

            TrySet(() => capabilities.PixelType.Set(pixelType));
            TrySet(() => capabilities.XResolution.Set(dpi));
            TrySet(() => capabilities.YResolution.Set(dpi));

            // Минус один означает «сколько отдаст устройство»: у плосконастольных
            // сканеров это обычно одна страница, у автоподачи — пачка.
            TrySet(() => capabilities.XferCount.Set(-1));
        }

        // Значение передаётся лениво: у неподдерживаемой возможности обращение к ней
        // уже вызывает у драйвера ошибку, и до проверки доходить не должно.
        private static void TrySet(Action apply)
        {
            try
            {
                apply();
            }
            catch (Exception)
            {
                // Возможность у этого устройства есть не всякая, и это не сбой:
                // страница всё равно придёт, только с настройками по умолчанию.
            }
        }

        // Страница сначала пишется во временный файл и переименовывается: помощника
        // можно закрыть в любой момент, и незаконченный файл в каталог попасть не должен.
        // Формат BMP, а не JPEG: страница идёт дальше в JPEG один раз, и сжимать её
        // дважды с разными настройками только добавило бы артефактов.
        private static void Save(System.Drawing.Image image, string path)
        {
            string temporary = path + ".part";

            using (image)
                image.Save(temporary, ImageFormat.Bmp);

            if (File.Exists(path))
                File.Delete(path);

            File.Move(temporary, path);
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
