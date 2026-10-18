using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ImageMagick;
using Kite2.Core;

namespace Kite2.Tests
{
    public static class DocumentExporterTests
    {
        [Test("Документ сохраняется в PDF под запрошенным именем")]
        public static void PdfKeepsRequestedName()
        {
            using (var folder = new TemporaryFolder())
            {
                string target = folder.File("письмо.pdf");

                DocumentExporter.Export(Pages(folder, 3), target, ScanMode.Greyscale);

                Check.FileExists(target, "PDF");
                Check.True(new FileInfo(target).Length > 0, "PDF не пустой");
            }
        }

        [Test("Черновик не остаётся рядом с готовым документом")]
        public static void DraftDoesNotStay()
        {
            using (var folder = new TemporaryFolder())
            {
                string target = folder.File("письмо.pdf");

                DocumentExporter.Export(Pages(folder, 2), target, ScanMode.Colour);

                // Черновик с меткой в середине имени не должен остаться: в папке
                // лежит ровно один PDF — под запрошенным именем.
                Check.Equal(1, Directory.GetFiles(folder.Path, "*.pdf").Length, "файлов PDF в папке");
            }
        }

        [Test("Цветной документ собирается в PDF")]
        public static void ColourPdf()
        {
            using (var folder = new TemporaryFolder())
            {
                string target = folder.File("цветной.pdf");

                DocumentExporter.Export(Pages(folder, 2), target, ScanMode.Colour);

                Check.FileExists(target, "PDF");
            }
        }

        [Test("Страницы режима Ч/Б идут в PDF серыми")]
        public static void GreyscalePdfKeepsHalfTones()
        {
            using (var folder = new TemporaryFolder())
            {
                string target = folder.File("серый.pdf");

                DocumentExporter.Export(Pages(folder, 1, gradient: true), target, ScanMode.Greyscale);

                Check.FileExists(target, "PDF");
                Check.True(new FileInfo(target).Length > 0, "PDF не пустой");
            }
        }

        [Test("Архив ZIP содержит все страницы с нумерацией по порядку")]
        public static void ZipHoldsEveryPage()
        {
            using (var folder = new TemporaryFolder())
            {
                string target = folder.File("архив.zip");

                DocumentExporter.Export(Pages(folder, 4), target, ScanMode.Colour);

                Check.FileExists(target, "ZIP");

                using (var archive = ZipFile.OpenRead(target))
                {
                    var names = new List<string>();

                    foreach (ZipArchiveEntry entry in archive.Entries)
                        names.Add(entry.FullName);

                    Check.Equal(4, names.Count, "число файлов в архиве");
                    Check.Equal("IMAGE00000000.jpg", names[0], "имя первой страницы");
                    Check.Equal("IMAGE00000003.jpg", names[3], "имя последней страницы");
                }
            }
        }

        [Test("Несколько страниц сохраняются отдельными файлами JPEG")]
        public static void SeveralJpegsGetNumbers()
        {
            using (var folder = new TemporaryFolder())
            {
                string target = folder.File("письмо.jpg");

                DocumentExporter.Export(Pages(folder, 3), target, ScanMode.Colour);

                // При нескольких страницах номер получает каждая, включая первую:
                // так файлы не различаются только порядком создания в проводнике.
                Check.FileExists(folder.File("письмо (1).jpg"), "первая страница");
                Check.FileExists(folder.File("письмо (2).jpg"), "вторая страница");
                Check.FileExists(folder.File("письмо (3).jpg"), "третья страница");
            }
        }

        [Test("Многостраничный TIFF собирается в один файл")]
        public static void TiffKeepsEveryPage()
        {
            using (var folder = new TemporaryFolder())
            {
                string target = folder.File("письмо.tif");

                DocumentExporter.Export(Pages(folder, 3), target, ScanMode.Colour);

                Check.FileExists(target, "TIFF");

                using (var collection = new MagickImageCollection(target))
                    Check.Equal(3, collection.Count, "страниц в TIFF");
            }
        }

        [Test("Одна страница сохраняется PNG под запрошенным именем")]
        public static void SinglePngKeepsRequestedName()
        {
            using (var folder = new TemporaryFolder())
            {
                string target = folder.File("лист.png");

                DocumentExporter.Export(Pages(folder, 1), target, ScanMode.Greyscale);

                Check.FileExists(target, "PNG");

                // Под именем .png должен лежать настоящий PNG, а не переименованный
                // JPEG: часть просмотрщиков такие файлы не открывает.
                using (var image = new MagickImage(target))
                    Check.Equal(MagickFormat.Png, image.Format, "формат файла");
            }
        }

        [Test("Несколько страниц сохраняются отдельными файлами PNG")]
        public static void SeveralPngsGetNumbers()
        {
            using (var folder = new TemporaryFolder())
            {
                DocumentExporter.Export(Pages(folder, 2), folder.File("письмо.png"), ScanMode.Greyscale);

                Check.FileExists(folder.File("письмо (1).png"), "первая страница");
                Check.FileExists(folder.File("письмо (2).png"), "вторая страница");
            }
        }

        [Test("Список поддерживаемых форматов совпадает с тем, что сохраняет ядро")]
        public static void SupportedFormatsAreAllWritable()
        {
            using (var folder = new TemporaryFolder())
            {
                foreach (string extension in DocumentExporter.SupportedExtensions)
                {
                    string target = folder.File("документ" + extension);

                    DocumentExporter.Export(Pages(folder, 1), target, ScanMode.Colour);

                    string produced = Directory.GetFiles(folder.Path, "*" + extension.TrimStart('.'))
                        .OrderBy(Path.GetFileName)
                        .FirstOrDefault();

                    Check.FileExists(produced ?? target, "формат " + extension);
                }
            }
        }

        [Test("Неизвестное расширение отвергается с понятной ошибкой")]
        public static void UnknownExtensionRejected()
        {
            using (var folder = new TemporaryFolder())
            {
                Check.Throws<NotSupportedException>(
                    () => DocumentExporter.Export(Pages(folder, 1), folder.File("письмо.doc"), ScanMode.Colour),
                    "расширение .doc");
            }
        }

        [Test("Пустой документ не создаёт файл")]
        public static void EmptyDocumentWritesNothing()
        {
            using (var folder = new TemporaryFolder())
            {
                string target = folder.File("пусто.pdf");

                DocumentExporter.Export(new string[0], target, ScanMode.Colour);

                Check.False(File.Exists(target), "файл создан, хотя страниц не было");
            }
        }

        // --- вспомогательное ---------------------------------------------------

        private static IReadOnlyList<string> Pages(TemporaryFolder folder, int count, bool gradient = false)
        {
            var pages = new string[count];

            for (int i = 0; i < count; i++)
            {
                string path = folder.File($"page{i}.jpg");

                using (var image = gradient
                    ? new MagickImage("gradient:black-white", 400, 560)
                    : new MagickImage(new MagickColor(255, 255, 255), 400, 560))
                {
                    image.Quality = 80;
                    image.Write(path);
                }

                pages[i] = path;
            }

            return pages;
        }
    }
}
