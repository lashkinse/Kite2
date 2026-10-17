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