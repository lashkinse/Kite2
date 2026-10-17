using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using ImageMagick;
using ImageMagick.Drawing;

namespace Kite2.Core
{
    /// <summary>
    /// Тестовый режим: страницы рисует программа, а не сканер.
    /// </summary>
    /// <remarks>
    /// Нужен там, где сканера под рукой нет: без него нельзя проверить ни путь от
    /// захвата до просмотра, ни сохранение, ни колонку страниц. На настоящем
    /// устройстве эти проверки делаются вручную и не каждый день.
    /// Включается ключом запуска <c>--test</c>, а не настройкой: режим должен быть
    /// виден в заголовке окна, иначе тестовый лист можно принять за настоящий.
    /// </remarks>
    public static class FakeScanner
    {
        /// <summary>Ключ запуска, включающий тестовый режим.</summary>
        public const string Switch = "--test";

        /// <summary>
        /// Ключ запуска, заставляющий задавать вопрос «Сканировать ещё?» и в тестовом
        /// режиме: иначе окно остановит и прогон, и проверку вручную.
        /// </summary>
        public const string AskSwitch = "--ask";

        /// <summary>Столько страниц возвращает один захват.</summary>
        private const int PageCount = 3;

        /// <summary>Тестовый лист: A4 при 150 dpi.</summary>
        private const double PageWidth = 1240;

        private const double PageHeight = 1754;

        private static int _counter;

        /// <summary>Работает ли программа в тестовом режиме.</summary>
        public static bool Enabled { get; private set; }

        /// <summary>
        /// Задавать ли вопрос «Сканировать ещё?» и в тестовом режиме. Включается
        /// ключом <c>--ask</c>: без него вопрос пропускается, иначе окно остановит и
        /// прогон, и проверку вручную.
        /// </summary>
        public static bool AlwaysAsk { get; private set; }

        /// <summary>
        /// Включает тестовый режим по ключам запуска. Возвращает признак того, что
        /// режим включён, — окно дописывает его в заголовок.
        /// </summary>
        public static bool EnableFromArguments(string[] arguments)
        {
            if (arguments == null)
                return false;

            bool enabled = false;

            foreach (string argument in arguments)
            {
                if (string.Equals(argument, Switch, StringComparison.OrdinalIgnoreCase))
                    enabled = true;
                else if (string.Equals(argument, AskSwitch, StringComparison.OrdinalIgnoreCase))
                    AlwaysAsk = true;
            }

            Enabled = enabled;

            return enabled;
        }

        /// <summary>Названия источников в тестовом режиме.</summary>
        public static IReadOnlyList<string> GetSources()
        {
            return new[] { "Тестовый сканер" };
        }

        /// <summary>
        /// Рисует страницы в каталог запроса вместо захвата со сканера.
        /// </summary>
        /// <param name="request">Запрос с уже очищенным каталогом.</param>
        /// <param name="cancellationToken">Позволяет прервать ожидание, как и при настоящем захвате.</param>
        /// <returns>Пути к нарисованным сканам.</returns>
        public static string[] Capture(ScanRequest request, CancellationToken cancellationToken)
        {
            var written = new string[PageCount];

            // Номера продолжаются с прошлых запусков, чтобы страницы не выглядели
            // одинаковыми при повторном сканировании.
            int first = Interlocked.Add(ref _counter, PageCount);

            ImageMagickSetup.EnsureStarted();

            for (int i = 0; i < PageCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string path = Path.Combine(
                    request.Directory,
                    "scan" + (first + i).ToString("D3", CultureInfo.InvariantCulture) + ScannerService.RawExtension);

                DrawSheet(first + i, request.Colour, path);

                written[i] = path;
            }

            return written;
        }

        /// <summary>
        /// Рисует лист: рамку, номер страницы и полосу градаций. Чёрно-белый лист
        /// рисуется градациями серого, чтобы по нему было видно, что полутонов не
        /// осталось два.
        /// </summary>
        private static void DrawSheet(int number, CaptureColour colour, string path)
        {
            using (var sheet = new MagickImage(MagickColors.White, (uint)PageWidth, (uint)PageHeight))
            {
                var draws = new List<IDrawable>
                {
                    // Рамка рисуется обводкой, а не заливкой: прямоугольник с заливкой
                    // закрывал бы белый лист целиком.
                    new DrawableFillColor(MagickColors.White),
                    new DrawableStrokeColor(new MagickColor("#BFBFBF")),
                    new DrawableStrokeWidth(2),
                    new DrawableRectangle(40, 40, PageWidth - 82, PageHeight - 82),
                    new DrawableFont("Segoe UI"),
                    new DrawableFontPointSize(72),
                    new DrawableFillColor(new MagickColor("#1A1A1A")),
                    new DrawableText(80, 190, "Тестовая страница " + number.ToString(CultureInfo.InvariantCulture))
                };

                sheet.Draw(draws.ToArray());

                ToneBar(sheet, colour);

                sheet.Format = MagickFormat.Bmp;
                sheet.Write(path);
            }
        }

        /// <summary>
        /// Полоса градаций: в чёрно-белом режиме от белого к чёрному, в цветном —
        /// цветовой переход. Ступени отличаются друг от друга заметно: соседние
        /// оттенки после сжатия в JPEG сливались, и полоса читалась как сплошная.
        /// </summary>
        private static void ToneBar(MagickImage sheet, CaptureColour colour)
        {
            const int Left = 80;
            const int Top = 280;
            const int Height = 420;
            const int Width = 1000;
            const int Steps = 12;

            int step = Width / Steps;

            var draws = new List<IDrawable>();

            for (int i = 0; i < Steps; i++)
            {
                double position = (double)i / (Steps - 1);

                // Прямоугольник задаётся углами, а не размерами: при размерах каждая
                // ступень перекрывала предыдущую по правому краю, и полосу заливал
                // последний, самый тёмный прямоугольник.
                draws.Add(new DrawableFillColor(Tone(colour, position)));
                draws.Add(new DrawableRectangle(
                    Left + i * step,
                    Top,
                    Left + (i + 1) * step,
                    Top + Height));
            }

            sheet.Draw(draws.ToArray());
        }

        /// <summary>Цвет ступени для указанной доли шкалы.</summary>
        private static MagickColor Tone(CaptureColour colour, double position)
        {
            if (colour == CaptureColour.BlackAndWhite)
            {
                int level = 255 - (int)(position * 255);

                return new MagickColor((byte)level, (byte)level, (byte)level);
            }

            return new MagickColor(
                (byte)(40 + position * 215),
                (byte)(90 - position * 60),
                (byte)(200 - position * 160));
        }

    }
}
