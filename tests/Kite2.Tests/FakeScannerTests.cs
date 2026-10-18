using System.IO;
using ImageMagick;
using Kite2.Core;

namespace Kite2.Tests
{
    public static class FakeScannerTests
    {
        [Test("Тестовый режим включается ключом запуска")]
        public static void EnablesFromSwitch()
        {
            // Ключи проверяются в любом порядке и вместе с посторонними.
            Check.True(FakeScanner.EnableFromArguments(new[] { "--test" }), "ключ --test");
            Check.True(FakeScanner.EnableFromArguments(new[] { "--other", "--TEST" }), "другой регистр");
            Check.False(FakeScanner.EnableFromArguments(new[] { "--прочее" }), "посторонний ключ");
            Check.False(FakeScanner.EnableFromArguments(null), "нет ключей");
        }

        [Test("Тестовый режим рисует три страницы")]
        public static void DrawsThreePages()
        {
            string[] pages = CapturePages(CaptureColour.BlackAndWhite);

            Check.Equal(3, pages.Length, "число нарисованных страниц");

            foreach (string page in pages)
                Check.FileExists(page, "файл страницы: " + Path.GetFileName(page));
        }

        [Test("Чёрно-белая тестовая страница серая и в ней есть полутон")]
        public static void GreyscalePageKeepsHalfTones()
        {
            string page = CapturePages(CaptureColour.BlackAndWhite)[0];

            using (var image = new MagickImage(page))
            {
                // Полость градаций: если бы оттенки срезались, осталось бы два.
                Check.True(image.Histogram().Count > 8, "число оттенков на странице: " + image.Histogram().Count);
            }
        }

        [Test("Цветная тестовая страница остаётся цветной")]
        public static void ColourPageStaysColour()
        {
            string page = CapturePages(CaptureColour.Colour)[0];

            using (var image = new MagickImage(page))
                Check.True(image.ColorType != ColorType.Grayscale, "страница не серая");
        }

        [Test("Тестовый режим даёт лист A4 того же размера, что и настоящий скан")]
        public static void PageIsA4()
        {
            string page = CapturePages(CaptureColour.BlackAndWhite)[0];

            using (var image = new MagickImage(page))
            {
                Check.Equal(2480u, image.Width, "ширина листа");
                Check.Equal(3508u, image.Height, "высота листа");
            }
        }

        private static string[] CapturePages(CaptureColour colour)
        {
            FakeScanner.EnableFromArguments(new[] { FakeScanner.Switch });

            var request = new ScanRequest { SourceIndex = 0, Colour = colour };

            ScannerService.Capture(request, System.Threading.CancellationToken.None);

            string[] pages = PageImage.ComposeDirectory(request, 25);

            Check.True(pages.Length > 0, "страницы не собраны");

            return pages;
        }
    }
}
