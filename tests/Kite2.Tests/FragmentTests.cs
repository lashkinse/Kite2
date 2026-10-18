using System;
using System.IO;
using ImageMagick;
using Kite2.Core;

namespace Kite2.Tests
{
    public static class FragmentTests
    {
        [Test("Фрагмент получает размер выделенного участка")]
        public static void FragmentKeepsSelectedSize()
        {
            using (var folder = new TemporaryFolder())
            {
                string page = WritePage(folder, 800, 1000);
                string target = folder.File("фрагмент.png");

                PageImage.SaveFragment(page, 100, 200, 300, 400, target);

                using (var image = new MagickImage(target))
                {
                    Check.Equal(300u, image.Width, "ширина фрагмента");
                    Check.Equal(400u, image.Height, "высота фрагмента");
                }
            }
        }

        [Test("Фрагмент сохраняется именно в PNG")]
        public static void FragmentIsReallyPng()
        {
            using (var folder = new TemporaryFolder())
            {
                string page = WritePage(folder, 400, 400);
                string target = folder.File("фрагмент.png");

                PageImage.SaveFragment(page, 10, 10, 100, 100, target);

                using (var image = new MagickImage(target))
                    Check.Equal(MagickFormat.Png, image.Format, "формат файла");
            }
        }

        [Test("Фрагмент берёт пиксели из выделенного места страницы")]
        public static void FragmentTakesRightPixels()
        {
            using (var folder = new TemporaryFolder())
            {
                string page = MarkedPage(folder, "метка.jpg");
                string target = folder.File("красный.png");

                PageImage.SaveFragment(page, 0, 0, 200, 200, target);

                using (var actual = new MagickImage(target))
                using (var red = new MagickImage(new MagickColor(255, 0, 0), 200, 200))
                {
                    // Отсчёт идёт от левого верхнего угла: взятый участок должен
                    // совпасть с красным квадратом, а не с белым полем.
                    double difference = actual.Compare(red, ErrorMetric.Absolute);

                    Check.True(difference < 1.0, "расхождение с красным квадратом: " + difference);
                }
            }
        }

        [Test("Фрагмент не захватывает лишнего за своей границей")]
        public static void FragmentKeepsOutsideOut()
        {
            using (var folder = new TemporaryFolder())
            {
                string page = MarkedPage(folder, "метка2.jpg");
                string target = folder.File("белый.png");

                // Участок, где метки нет: в фрагмент не должно попасть ничего
                // постороннего — ни пикселя лишнего.
                PageImage.SaveFragment(page, 200, 200, 200, 200, target);

                using (var actual = new MagickImage(target))
                using (var white = new MagickImage(new MagickColor(255, 255, 255), 200, 200))
                {
                    double difference = actual.Compare(white, ErrorMetric.Absolute);

                    Check.True(difference < 1.0, "расхождение с белым полем: " + difference);
                }
            }
        }

        [Test("Страница-источник остаётся прежней")]
        public static void SourcePageStaysWhole()
        {
            using (var folder = new TemporaryFolder())
            {
                string page = MarkedPage(folder, "целая.jpg");
                string target = folder.File("кусок.png");

                PageImage.SaveFragment(page, 0, 0, 200, 200, target);

                using (var image = new MagickImage(page))
                {
                    // Фрагмент сохраняется рядом, документ не меняется: иначе
                    // отмеченный кусок забрался бы в документ и вырезал бы из
                    // него остальное.
                    Check.Equal(400u, image.Width, "ширина страницы");
                    Check.Equal(400u, image.Height, "высота страницы");
                }
            }
        }

        [Test("Фрагмент не выходит за края страницы")]
        public static void FragmentClampsToPage()
        {
            using (var folder = new TemporaryFolder())
            {
                string page = WritePage(folder, 400, 500);
                string target = folder.File("у края.png");

                // Рамка, вычерченная с одного пикселя, уходит за лист: фрагмент
                // урезается по краю, а не выходит за границы страницы.
                PageImage.SaveFragment(page, 350, 450, 500, 500, target);

                using (var image = new MagickImage(target))
                {
                    Check.Equal(50u, image.Width, "ширина");
                    Check.Equal(50u, image.Height, "высота");
                }
            }
        }

        [Test("Фрагмент сохраняет плотность страницы")]
        public static void FragmentKeepsResolution()
        {
            using (var folder = new TemporaryFolder())
            {
                string page = WritePage(folder, 600, 800);
                string target = folder.File("плотный.png");

                PageImage.SaveFragment(page, 10, 10, 200, 200, target);

                using (var image = new MagickImage(target))
                {
                    // PNG хранит плотность в пикселях на сантиметр, а не в точках
                    // на дюйм: 300 dpi — это 118,11 px/cm. Проверяется, что
                    // плотность не потерялась, а не то, как она записана.
                    double pixelsPerCentimetre = image.Density.X;

                    Check.True(
                        Math.Abs(pixelsPerCentimetre - 300 / 2.54) < 1,
                        "плотность фрагмента: " + pixelsPerCentimetre);
                }
            }
        }

        [Test("Пустой выделенный участок отвергается")]
        public static void EmptyFragmentRejected()
        {
            using (var folder = new TemporaryFolder())
            {
                string page = WritePage(folder, 200, 200);
                string target = folder.File("пустой.png");

                Check.Throws<ArgumentOutOfRangeException>(
                    () => PageImage.SaveFragment(page, 10, 10, 0, 100, target),
                    "нулевая ширина");

                Check.Throws<ArgumentOutOfRangeException>(
                    () => PageImage.SaveFragment(page, 10, 10, 100, 0, target),
                    "нулевая высота");

                Check.False(File.Exists(target), "файл создан, хотя выделять было нечего");
            }
        }

        private static string WritePage(TemporaryFolder folder, int width, int height)
        {
            string path = folder.File("страница.jpg");

            using (var image = new MagickImage(new MagickColor(255, 255, 255), (uint)width, (uint)height))
            {
                image.Density = new Density(300, 300);
                image.Quality = 80;
                image.Format = MagickFormat.Jpeg;
                image.Write(path);
            }

            return path;
        }

        private static string MarkedPage(TemporaryFolder folder, string name)
        {
            string path = folder.File(name);

            using (var sheet = new MagickImage(new MagickColor(255, 255, 255), 400u, 400u))
            using (var mark = new MagickImage(new MagickColor(255, 0, 0), 200u, 200u))
            {
                sheet.Composite(mark, Gravity.Northwest, CompositeOperator.SrcOver);

                sheet.Format = MagickFormat.Jpeg;
                sheet.Write(path);
            }

            return path;
        }
    }
}
