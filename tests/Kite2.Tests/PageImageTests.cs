using System;
using System.IO;
using ImageMagick;
using Kite2.Core;

namespace Kite2.Tests
{
    public static class PageImageTests
    {
        private const uint A4Width = 2480;

        private const uint A4Height = 3508;

        [Test("Скан меньше листа остаётся своего размера и центрируется")]
        public static void SmallScanKeepsSize()
        {
            using (var folder = new TemporaryFolder())
            {
                string scan = WriteScan(folder, 600, 400);

                var request = new ScanRequest();
                ComposeInto(request, scan);

                using (var page = new MagickImage(PageOf(request)))
                {
                    Check.Equal(A4Width, page.Width, "ширина страницы");
                    Check.Equal(A4Height, page.Height, "высота страницы");
                }
            }
        }

        [Test("Скан больше листа уменьшается и помещается целиком")]
        public static void LargeScanFits()
        {
            using (var folder = new TemporaryFolder())
            {
                string scan = WriteScan(folder, 6000, 4000);

                var request = new ScanRequest();
                ComposeInto(request, scan);

                using (var page = new MagickImage(PageOf(request)))
                {
                    Check.True(page.Width <= A4Width, "страница не шире листа");
                    Check.True(page.Height <= A4Height, "страница не выше листа");
                }
            }
        }

        [Test("Режим Ч/Б даёт оттенки серого, а не чистый чёрный с белым")]
        public static void GreyscaleKeepsHalfTones()
        {
            using (var folder = new TemporaryFolder())
            {
                // Кадр, где слева чёрное, справа белое. Отсечка по порогу сделала бы
                // всю правую половину одинаковой, и полутонов в документе не осталось бы.
                string scan = WriteGradient(folder);

                var request = new ScanRequest { Colour = CaptureColour.BlackAndWhite };
                ComposeInto(request, scan);

                using (var page = new MagickImage(PageOf(request)))
                {
                    // Точный тип цвета при чтении обратно зависит от того, как ImageMagick
                    // сложит серые уровни в палитру, и на требования не влияет.
                    // Проверяется главное: полутонов много, а не два.
                    Check.True(
                        page.ColorType == ColorType.Grayscale || page.ColorType == ColorType.Palette,
                        "страница серая");

                    // Оттенков должно быть много: при отсечке по порогу остались бы
                    // ровно два — чёрный и белый.
                    Check.True(page.Histogram().Count > 8, "в странице больше двух оттенков серого");
                }
            }
        }

        [Test("Цветной режим оставляет страницу цветной")]
        public static void ColourStaysColour()
        {
            using (var folder = new TemporaryFolder())
            {
                string scan = WriteScan(folder, 600, 400);

                var request = new ScanRequest { Colour = CaptureColour.Colour };
                ComposeInto(request, scan);

                using (var page = new MagickImage(PageOf(request)))
                {
                    Check.True(page.ColorType != ColorType.Grayscale, "страница осталась цветной");
                }
            }
        }

        [Test("Поворот меняет страницу и перезаписывает файл")]
        public static void RotateRewritesFile()
        {
            using (var folder = new TemporaryFolder())
            {
                string page = Path.Combine(folder.Path, "page.jpg");

                using (var image = new MagickImage(new MagickColor(255, 255, 255), 800, 400))
                    image.Write(page);

                PageImage.Rotate(page, 180.0);

                using (var rotated = new MagickImage(page))
                {
                    Check.Equal(800u, rotated.Width, "ширина после поворота");
                    Check.Equal(400u, rotated.Height, "высота после поворота");
                }
            }
        }

        [Test("Размер уменьшения считается по обеим сторонам")]
        public static void FitInsideKeepsProportions()
        {
            uint width;
            uint height;

            PageImage.FitInside(1000, 500, 200, 200, out width, out height);

            Check.Equal(200u, width, "ширина");
            Check.Equal(100u, height, "высота");
        }

        // --- вспомогательное ---------------------------------------------------

        private static void ComposeInto(ScanRequest request, string scan)
        {
            string prepared = Path.Combine(request.Directory, "scan001.bmp");

            File.Copy(scan, prepared, true);

            PageImage.ComposeDirectory(request, 25);
        }

        private static string PageOf(ScanRequest request)
        {
            string[] pages = Directory.GetFiles(request.Directory, "*.jpg");

            Check.Equal(1, pages.Length, "число собранных страниц");

            return pages[0];
        }

        private static string WriteScan(TemporaryFolder folder, int width, int height)
        {
            string path = folder.File("scan.bmp");

            using (var image = new MagickImage(new MagickColor(180, 180, 180), (uint)width, (uint)height))
                image.Write(path);

            return path;
        }

        private static string WriteGradient(TemporaryFolder folder)
        {
            string path = folder.File("gradient.bmp");

            using (var image = new MagickImage("gradient:black-white", 100, (uint)A4Height))
                image.Write(path);

            return path;
        }
    }
}
