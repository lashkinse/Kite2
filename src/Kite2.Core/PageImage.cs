using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageMagick;

namespace Kite2.Core
{
    /// <summary>
    /// Превращает то, что отдал TWAIN-драйвер, в страницы документа.
    /// </summary>
    /// <remarks>
    /// Драйвер кладёт рядом файлы PPM (цветные) и PGM (чёрно-белые). Читаются они
    /// прямо здесь, средствами ImageMagick: в прежней версии программы между
    /// драйвером и страницей стоял внешний инструмент pnmtojpeg, и приложение
    /// зависело от папки с чужими программами рядом с собой.
    /// </remarks>
    public static class PageImage
    {
        /// <summary>Лист A4 при 300 dpi — в него вписывается скан любого размера.</summary>
        private const uint PageA4Width = 2480;

        private const uint PageA4Height = 3508;

        /// <summary>
        /// Превращает все результаты захвата в страницы JPEG, вписывая каждый скан по
        /// центру белого листа A4. Исходные PPM и PGM удаляются.
        /// </summary>
        /// <param name="request">Запрос, у которого каталог уже очищен.</param>
        /// <param name="quality">Качество JPEG.</param>
        /// <returns>Пути к страницам в порядке сканирования. Пустой массив, если сканов не было.</returns>
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

                ComposePage(source, page, request.Mode, quality);

                File.Delete(source);
                pages[i] = page;
            }

            return pages;
        }

        /// <summary>
        /// Файлы, которые получаются от драйвера и несут скан без обработки: BMP
        /// записывает сам сервис захвата, PPM и PGM — если драйвер отдал файлы сам.
        /// </summary>
        private static bool IsRawScan(string file)
        {
            string extension = Path.GetExtension(file);

            return extension.Equals(".bmp", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".ppm", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".pgm", StringComparison.OrdinalIgnoreCase);
        }

        private static void ComposePage(string source, string target, ScanMode mode, int quality)
        {
            using (var scan = new MagickImage(source))
            using (MagickImage sheet = FitOntoSheet(scan))
            {
                // Оттенки серого, а не порог: отсечка убивала светлые линии и тонкий
                // текст, а в режиме «Ч/Б» пользователь ждёт именно чёрно-белый скан
                // с различимыми полутонами.
                if (mode == ScanMode.Greyscale)
                    sheet.ColorType = ColorType.Grayscale;

                sheet.Quality = (uint)quality;
                sheet.Format = MagickFormat.Jpeg;
                sheet.Write(target);
            }
        }

        /// <summary>
        /// Вписывает скан в белый лист A4 по центру. Скан больше листа уменьшается,
        /// меньший остаётся как есть и просто центрируется.
        /// </summary>
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

        /// <summary>Поворачивает страницу на заданный угол и перезаписывает файл.</summary>
        public static void Rotate(string filePath, double degrees)
        {
            ImageMagickSetup.EnsureStarted();

            using (var image = new MagickImage(filePath))
            {
                image.Rotate(degrees);
                image.Write(filePath);
            }
        }

        /// <summary>
        /// Сохраняет выделенный участок страницы отдельным файлом PNG. Сама
        /// страница не меняется: фрагмент уходит рядом, документ остаётся прежним.
        /// </summary>
        /// <param name="sourcePath">Файл страницы-источника.</param>
        /// <param name="left">Отступ слева, в пикселях изображения.</param>
        /// <param name="top">Отступ сверху, в пикселях изображения.</param>
        /// <param name="width">Ширина фрагмента, в пикселях.</param>
        /// <param name="height">Высота фрагмента, в пикселях.</param>
        /// <param name="targetPath">Файл фрагмента.</param>
        public static void SaveFragment(string sourcePath, int left, int top, int width, int height, string targetPath)
        {
            if (width < 1 || height < 1)
                throw new ArgumentOutOfRangeException(nameof(width), "Фрагмент нулевой ширины или высоты.");

            ImageMagickSetup.EnsureStarted();

            using (var image = new MagickImage(sourcePath))
            {
                Density density = image.Density;

                // Рамка, вычерченная мышью с одного пикселя, уходит за лист:
                // фрагмент урезается по краю, а не выходит за границы страницы.
                int x = Math.Max(0, Math.Min(left, (int)image.Width - 1));
                int y = Math.Max(0, Math.Min(top, (int)image.Height - 1));

                int w = Math.Max(1, Math.Min(width, (int)image.Width - x));
                int h = Math.Max(1, Math.Min(height, (int)image.Height - y));

                image.Crop(new MagickGeometry(x, y, (uint)w, (uint)h));

                image.Density = density;

                // PNG хранит то, что отдали, без потерь: фрагмент обычно идёт в
                // документ или в сообщение, где JPEG ухудшил бы мелкий текст.
                image.Format = MagickFormat.Png;
                image.Write(targetPath);
            }
        }

        /// <summary>
        /// Размер, в который изображение уменьшается целиком, чтобы поместиться в рамку.
        /// </summary>
        public static void FitInside(uint width, uint height, uint boundWidth, uint boundHeight, out uint resultWidth, out uint resultHeight)
        {
            float scale = Math.Min(boundWidth / (float)width, boundHeight / (float)height);

            resultWidth = (uint)(width * scale);
            resultHeight = (uint)(height * scale);
        }

        /// <summary>Удаляет файлы страниц: ими владеет программа, а не документ на диске.</summary>
        public static void Delete(IEnumerable<string> pages)
        {
            foreach (string page in pages)
            {
                try
                {
                    File.Delete(page);
                }
                catch (IOException)
                {
                    // Файл занят просмотром: он удалится при следующей очистке каталога.
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }
    }
}