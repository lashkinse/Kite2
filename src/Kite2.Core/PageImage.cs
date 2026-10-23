using System;
using System.IO;
using System.Linq;
using System.Threading;
using ImageMagick;

namespace Kite2.Core
{
    // Драйвер может отдать файлы PPM и PGM — они читаются здесь, средствами
    // ImageMagick, без внешних инструментов рядом с программой.
    public static class PageImage
    {
        // Лист A4 при 300 dpi — в него вписывается скан любого размера.
        private const uint PageA4Width = 2480;

        private const uint PageA4Height = 3508;

        public static string[] ComposeDirectory(ScanRequest request, int quality)
        {
            ImageMagickSetup.EnsureStarted();

            string[] sources = Directory
                .GetFiles(request.Directory)
                .Where(file => IsRawScan(file))
                .OrderBy(file => new FileInfo(file).CreationTime)
                .ToArray();

            var pages = new string[sources.Length];

            for (int i = 0; i < sources.Length; i++)
            {
                string source = sources[i];
                string page = Path.ChangeExtension(source, ".jpg");

                // Режим не задаётся: что отдал драйвер, то и сохраняем.
                ComposePage(source, page, quality);

                File.Delete(source);
                pages[i] = page;
            }

            return pages;
        }

        // BMP записывает сервис захвата, PPM и PGM — если драйвер отдал файлы сам.
        private static bool IsRawScan(string file)
        {
            string extension = Path.GetExtension(file);

            return extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".ppm", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".pgm", StringComparison.OrdinalIgnoreCase);
        }

        // Копия страницы уходит в постоянное хранилище: следующая серия очистит
        // каталог сканирования, а документ обязан остаться целым.
        private static int _storeSequence;

        public static string Store(string file)
        {
            int sequence = Interlocked.Increment(ref _storeSequence);
            string target = Path.Combine(
                AppPaths.Pages,
                $"page-{DateTime.Now:HHmmss}-{sequence}.jpg");

            File.Copy(file, target, true);

            return target;
        }

        // Прежний режим «оттенками серого» убран вместе с выбором «Ч/Б» у
        // приложения: драйвер сам отдаёт то, что выбрал человек, и принудительная
        // перекраска только портила цветные сканы.
        private static void ComposePage(string source, string target, int quality)
        {
            using (var scan = new MagickImage(source))
            using (MagickImage sheet = FitOntoSheet(scan))
            {
                sheet.Quality = (uint)quality;
                sheet.Format = MagickFormat.Jpeg;
                sheet.Write(target);
            }
        }

        private static MagickImage FitOntoSheet(MagickImage scan)
        {
            if (scan.Width > PageA4Width || scan.Height > PageA4Height)
            {
                uint width;
                uint height;

                FitInside(scan.Width, scan.Height, PageA4Width, PageA4Height, out width, out height);

                scan.Resize(width, height);
            }

            var sheet = new MagickImage(new MagickColor(255, 255, 255), PageA4Width, PageA4Height);
            sheet.Composite(scan, Gravity.Center, CompositeOperator.SrcOver);

            return sheet;
        }

        public static void Rotate(string filePath, double degrees)
        {
            ImageMagickSetup.EnsureStarted();

            using (var image = new MagickImage(filePath))
            {
                image.Rotate(degrees);
                image.Write(filePath);
            }
        }

        public static void SaveFragment(string sourcePath, int left, int top, int width, int height, string targetPath)
        {
            if (width < 1 || height < 1)
                throw new ArgumentOutOfRangeException(nameof(width), "Фрагмент нулевой ширины или высоты.");

            ImageMagickSetup.EnsureStarted();

            using (var image = new MagickImage(sourcePath))
            {
                Density density = image.Density;

                // Рамка, вычерченная мышью, может уйти за лист: фрагмент урезается
                // по краю страницы.
                int x = Math.Max(0, Math.Min(left, (int)image.Width - 1));
                int y = Math.Max(0, Math.Min(top, (int)image.Height - 1));

                int w = Math.Max(1, Math.Min(width, (int)image.Width - x));
                int h = Math.Max(1, Math.Min(height, (int)image.Height - y));

                image.Crop(new MagickGeometry(x, y, (uint)w, (uint)h));

                image.Density = density;

                // PNG без потерь: фрагмент обычно идёт в документ, где JPEG
                // ухудшил бы мелкий текст.
                image.Format = MagickFormat.Png;
                image.Write(targetPath);
            }
        }

        public static void FitInside(uint width, uint height, uint boundWidth, uint boundHeight, out uint resultWidth, out uint resultHeight)
        {
            float scale = Math.Min(boundWidth / (float)width, boundHeight / (float)height);

            resultWidth = (uint)(width * scale);
            resultHeight = (uint)(height * scale);
        }
    }
}