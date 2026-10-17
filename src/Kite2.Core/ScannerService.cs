using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using NLog;
using Saraff.Twain;

namespace Kite2.Core
{
    /// <summary>
    /// Работа со сканером через TWAIN: перечисление источников и захват страниц.
    /// </summary>
    /// <remarks>
    /// Захват всегда идёт в фоновом потоке, а прерывание делается закрытием сессии
    /// из другого потока: TWAIN-драйвер умеет не возвращаться вообще, и ждать его
    /// с таймаутом — значит прервать захват нельзя. Пользователь нажимает «Отмена»,
    /// сессия закрывается, и ожидание заканчивается.
    /// </remarks>
    public static class ScannerService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>Расширение файла, в который складываются полученные сканы.</summary>
        public const string RawExtension = ".bmp";

        /// <summary>
        /// Названия всех доступных TWAIN-источников. Пустой список, если драйвер
        /// недоступен: отсутствие сканера — не ошибка, а отсутствие устройств.
        /// </summary>
        public static IReadOnlyList<string> GetSources()
        {
            // В тестовом режиме драйвер не открывается вовсе: источник один и он
            // выдуманный, а настоящий сканер под рукой может и не оказаться.
            if (FakeScanner.Enabled)
                return FakeScanner.GetSources();

            var sources = new List<string>();

            try
            {
                using (var twain = new Twain32())
                {
                    if (!twain.OpenDSM())
                        return sources;

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
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Не удалось получить список TWAIN-источников");
                sources.Clear();
            }

            return sources;
        }

        /// <summary>
        /// Проверяет, что источник с таким индексом по-прежнему доступен. Индекс хранится
        /// в настройках, а порядок устройств после переподключения меняется, поэтому
        /// сохранённое значение может перестать соответствовать реальному устройству.
        /// </summary>
        public static bool IsSourceAvailable(int sourceIndex)
        {
            return sourceIndex >= 0 && sourceIndex < GetSources().Count;
        }

        /// <summary>
        /// Забирает страницы со сканера в каталог запроса.
        /// </summary>
        /// <param name="request">Запрос с выбранным источником и режимом цвета.</param>
        /// <param name="cancellationToken">Позволяет прервать ожидание драйвера.</param>
        /// <returns>Пути к полученным сканам в порядке захвата.</returns>
        public static string[] Capture(ScanRequest request, CancellationToken cancellationToken)
        {
            if (request.SourceIndex == AppSettings.NotSelected)
                throw new InvalidOperationException(Messages.ChooseSource);

            // В тестовом режиме страницы рисует программа: драйвер не запускается,
            // иначе проверка пути от захвата до просмотра требовала бы сканера.
            if (FakeScanner.Enabled)
            {
                FakeScanner.Capture(request, cancellationToken);
                return new string[0];
            }

            var pages = new List<string>();

            var twain = new Twain32
            {
                SourceIndex = request.SourceIndex,
                ShowUI = false,
                ModalUI = false,
                DisableAfterAcquire = false
            };

            try
            {
                // Прерывание извне: драйвер завис, сессию закрывает другой поток.
                using (cancellationToken.Register(() => CloseQuietly(twain)))
                {
                    if (!twain.OpenDSM())
                        throw new InvalidOperationException(Messages.NoSources);

                    try
                    {
                        twain.OpenDataSource();

                        ApplySettings(twain, request);

                        int index = 0;

                        twain.EndXfer += (sender, args) =>
                        {
                            string path = Path.Combine(
                                request.Directory,
                                "scan" + (++index).ToString("D3") + RawExtension);

                            Save(args.Image, path);
                            pages.Add(path);
                        };

                        twain.Acquire();
                    }
                    finally
                    {
                        twain.CloseDataSource();
                    }
                }
            }
            finally
            {
                CloseQuietly(twain);
            }

            Log.Debug("Захват вернул {0} страниц", pages.Count);

            return pages.ToArray();
        }

        /// <summary>
        /// Выставляет разрешение, режим цвета и число страниц.
        /// </summary>
        /// <remarks>
        /// Возможности, которых нет у конкретного устройства, пропускаются: у сканера без
        /// автоподачи нет счётчика страниц, и из-за него захват срывался целиком.
        /// </remarks>
        private static void ApplySettings(Twain32 twain, ScanRequest request)
        {
            TwPixelType pixelType = request.Colour == CaptureColour.BlackAndWhite
                ? TwPixelType.BW
                : TwPixelType.RGB;

            var capabilities = twain.Capabilities;

            TrySet(() => capabilities.PixelType.Set(pixelType));
            TrySet(() => capabilities.XResolution.Set(request.Dpi));
            TrySet(() => capabilities.YResolution.Set(request.Dpi));

            // Минус один означает «сколько отдаст устройство»: у плосконастольных
            // сканеров это обычно одна страница, у автоподачи — пачка.
            TrySet(() => capabilities.XferCount.Set(-1));
        }

        /// <summary>
        /// Выставляет возможность, если устройство её поддерживает. Само значение
        /// передаётся лениво: у неподдерживаемой возможности обращение к ней уже
        /// вызывает у драйвера ошибку, и до проверки доходить не должно.
        /// </summary>
        private static void TrySet(Action apply)
        {
            try
            {
                apply();
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Возможность источника не выставлена");
            }
        }

        /// <summary>
        /// Пишет полученный скан на диск. Формат BMP, а не JPEG: страница идёт дальше
        /// в JPEG один раз, и сжимать её дважды с разными настройками только добавило бы
        /// артефактов.
        /// </summary>
        private static void Save(System.Drawing.Image image, string path)
        {
            using (image)
                image.Save(path, ImageFormat.Bmp);
        }

        private static void CloseQuietly(Twain32 twain)
        {
            try
            {
                twain.CloseDSM();
            }
            catch (Exception ex)
            {
                // Закрытие из потока отмены может совпасть с завершением захвата.
                Log.Debug(ex, "Сессия TWAIN уже закрыта");
            }
        }
    }
}