using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using ImageMagick;
using ImageMagick.Drawing;

namespace Kite2.Core
{
    // Тестовый режим: сканера под рукой может не оказаться, а без него не проверить
    // ни путь от захвата до сохранения, ни колонку страниц. Включается ключом
    // запуска, чтобы режим был виден в заголовке окна.
    public static class FakeScanner
    {
        public const string Switch = "--test";

        // Без этого ключа вопрос «Сканировать ещё?» пропускается: иначе он
        // остановит и прогон, и автоматическую проверку.
        public const string AskSwitch = "--ask";

        private const int PageCount = 3;

        // Тестовый лист: A4 при 150 dpi.
        private const double PageWidth = 1240;

        private const double PageHeight = 1754;

        private static int _counter;

        public static bool Enabled { get; private set; }

        public static bool AlwaysAsk { get; private set; }

        // Цвет тестового листа: настоящая цветность осталась в ведении драйвера,
        // поэтому проверка явно назначает его перед захватом.
        public static CaptureColour Colour { get; set; } = CaptureColour.BlackAndWhite;

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

        public static IReadOnlyList<string> GetSources()
        {
            return new[] { "Тестовый сканер" };
        }

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

                DrawSheet(first + i, Colour, path);

                written[i] = path;
            }

            return written;
        }

        // Чёрно-белый лист рисуется градациями серого: по нему видно, что полутонов
        // не осталось два.
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

        // Ступени отличаются друг от друга заметно: соседние оттенки после сжатия
        // в JPEG сливались, и полоса читалась как сплошная.
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
