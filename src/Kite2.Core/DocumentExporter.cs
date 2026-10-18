using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using ImageMagick;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Kite2.Core
{
    public static class DocumentExporter
    {
        // Лист A4 при 150 dpi — в него масштабируются страницы многостраничного TIFF.
        private const uint PageA4HalfWidth = 1240;

        private const uint PageA4HalfHeight = 1754;

        public static readonly IReadOnlyList<string> SupportedExtensions = new[] { ".pdf", ".zip", ".tif", ".jpg", ".png" };

        // Сначала файл пишется рядом под временным именем и только потом переезжает
        // на место: PDF собирается по частям, и обрывок на середине выглядел бы как
        // готовый документ. Временное имя берётся в той же папке — перенос работает
        // только внутри одного тома.
        public static void Export(IReadOnlyList<string> pages, string filePath, ScanMode mode)
        {
            if (pages == null || pages.Count == 0)
                return;

            string extension = Path.GetExtension(filePath);

            if (!IsKnownExtension(extension))
                throw new NotSupportedException("Формат файла не поддерживается: " + extension);

            string folder = Path.GetDirectoryName(filePath) ?? ".";
            string name = Path.GetFileNameWithoutExtension(filePath);

            // Черновик отличается от настоящего имени только меткой в середине: при
            // нескольких страницах условие экспорта даёт не один файл, а несколько
            // («письмо (1).jpg», «письмо (2).jpg»), и метка должна уехать вместе с ними.
            string mark = "." + Path.GetRandomFileName().Replace(".", string.Empty);
            string draft = Path.Combine(folder, name + mark + extension);

            try
            {
                Write(pages, draft, mode, extension);

                // Проверяется не имя, а то, что переехало хоть что-то.
                if (Promote(folder, name, mark) == 0)
                    throw new IOException("Экспорт не создал ни одного файла: " + filePath);
            }
            finally
            {
                TryDelete(folder, name + mark + "*");
            }
        }

        public static bool IsKnownExtension(string extension)
        {
            foreach (string known in SupportedExtensions)
            {
                if (string.Equals(extension, known, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static int Promote(string folder, string name, string mark)
        {
            string[] draft = Directory.GetFiles(folder, name + mark + "*");

            foreach (string file in draft)
            {
                string finalName = Path.GetFileName(file).Replace(mark, string.Empty);
                string target = Path.Combine(folder, finalName);

                if (File.Exists(target))
                    File.Delete(target);

                File.Move(file, target);
            }

            return draft.Length;
        }

        private static void Write(IReadOnlyList<string> pages, string filePath, ScanMode mode, string extension)
        {
            if (string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
                SaveAsPdf(pages, filePath, mode);
            else if (string.Equals(extension, ".zip", StringComparison.OrdinalIgnoreCase))
                SaveAsZip(pages, filePath);
            else if (string.Equals(extension, ".tif", StringComparison.OrdinalIgnoreCase))
                SaveAsTiff(pages, filePath);
            else if (string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase))
                SaveAsPictures(pages, filePath, extension);
        }

        private static void TryDelete(string folder, string pattern)
        {
            try
            {
                foreach (string file in Directory.GetFiles(folder, pattern))
                    File.Delete(file);
            }
            catch (IOException)
            {
                // Остаток черновика переживёт до очистки папки: документ-то уже записан.
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private static void SaveAsPdf(IReadOnlyList<string> pages, string filePath, ScanMode mode)
        {
            string scratch = CreateScratchFolder();

            try
            {
                using (var document = new Document(PageSize.A4))
                using (var output = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                using (PdfWriter.GetInstance(document, output))
                {
                    document.SetMargins(0f, 0f, 0f, 0f);
                    document.Open();

                    try
                    {
                        for (int i = 0; i < pages.Count; i++)
                        {
                            // Новый лист открывается перед каждой страницей, кроме
                            // первой: иначе в конец документа добавляется пустой.
                            if (i > 0)
                                document.NewPage();

                            if (mode == ScanMode.Greyscale)
                                AddGreyscalePage(document, pages[i], scratch);
                            else
                                AddJpegPage(document, pages[i]);
                        }

                        document.Close();
                    }
                    catch
                    {
                        // При ошибке на первой же странице документ закрывается пустым,
                        // и iText поверх настоящей причины бросает «The document has no
                        // pages»: диагностировать потом нечего.
                        CloseQuietly(document);
                        throw;
                    }
                }
            }
            finally
            {
                RemoveScratch(scratch);
            }
        }

        private static string CreateScratchFolder()
        {
            string folder = Path.Combine(Path.GetTempPath(), "kite2-pdf-" + Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(folder);

            return folder;
        }

        private static void RemoveScratch(string folder)
        {
            try
            {
                Directory.Delete(folder, true);
            }
            catch (IOException)
            {
                // Файлы заняты — экспорт всё равно удался, а папку в системной
                // временной уберёт очистка.
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private static void CloseQuietly(Document document)
        {
            try
            {
                document.Close();
            }
            catch (IOException)
            {
                // Закрытие не удалось — это следствие исходной ошибки.
            }
        }

        // Файл передаётся по имени, а не потоком: iText читает картинку при закрытии
        // документа, и закрытый поток к тому моменту уже нечитаем.
        private static void AddJpegPage(Document document, string filePath)
        {
            iTextSharp.text.Image page = iTextSharp.text.Image.GetInstance(filePath);

            page.ScaleToFit(PageSize.A4);
            page.SetAbsolutePosition(0f, 0f);

            document.Add((IElement)page);
        }

        // Отсечки по порогу быть не должно: она убивала всё светлее 127, и
        // светло-серые линии исчезали из документа. Однобитный CCITT отвергнут вместе
        // с отсечкой, которой он требует: экономия не стоит потери полутонов.
        // Сжатие — Deflate: не портит и понимается всеми просмотрщиками.
        private static void AddGreyscalePage(Document document, string filePath, string scratch)
        {
            ImageMagickSetup.EnsureStarted();

            using (var source = new MagickImage(filePath))
            {
                source.ColorType = ColorType.Grayscale;

                // Страница переписывается в PNG — единственный формат, который iText
                // читает сам. Сырые пиксели в document.Add не берутся: картинка молча
                // не добавляется, и документ закрывается словами «The document has no pages».
                string grey = Path.Combine(
                    scratch,
                    Path.GetFileNameWithoutExtension(filePath) + ".png");

                source.Write(grey);

                iTextSharp.text.Image page = iTextSharp.text.Image.GetInstance(grey);

                page.ScaleToFit(PageSize.A4);
                page.SetAbsolutePosition(0f, 0f);

                document.Add((IElement)page);
            }
        }

        private static void SaveAsZip(IReadOnlyList<string> pages, string filePath)
        {
            using (FileStream stream = File.Open(filePath, FileMode.Create, FileAccess.Write))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                for (int i = 0; i < pages.Count; i++)
                {
                    // Имена задаются явно и с нумерацией: при распаковке страницы
                    // должны идти в исходном порядке.
                    var entry = archive.CreateEntry("IMAGE" + i.ToString("D8") + ".jpg");

                    using (Stream target = entry.Open())
                    using (FileStream source = File.OpenRead(pages[i]))
                        source.CopyTo(target);
                }
            }
        }

        private static void SaveAsTiff(IReadOnlyList<string> pages, string filePath)
        {
            ImageMagickSetup.EnsureStarted();

            using (var collection = new MagickImageCollection())
            {
                foreach (string page in pages)
                {
                    // Картинка не освобождается после добавления в коллекцию: коллекция
                    // записывается в файл только в конце, и освобождённая картинка к
                    // тому моменту уже недоступна.
                    var image = new MagickImage(page);

                    uint width;
                    uint height;

                    PageImage.FitInside(image.Width, image.Height, PageA4HalfWidth, PageA4HalfHeight, out width, out height);

                    image.Format = MagickFormat.Tif;
                    image.Depth = 8;
                    image.Settings.SetDefine("tiff:compression", "jpeg");
                    image.Resize(width, height);

                    collection.Add(image);
                }

                collection.Write(filePath);
            }
        }

        // PNG не принимает переименование JPEG: файл страницы перекодируется, иначе
        // под именем .png лежал бы JPEG, а такие файлы некоторые просмотрщики не открывают.
        private static void SaveAsPictures(IReadOnlyList<string> pages, string filePath, string extension)
        {
            bool convert = !string.Equals(extension, ".jpg", StringComparison.OrdinalIgnoreCase);

            if (pages.Count == 1)
            {
                if (convert)
                    SavePicture(pages[0], filePath);
                else
                    File.Copy(pages[0], filePath, true);

                return;
            }

            string folder = Path.GetDirectoryName(filePath);
            string baseName = Path.GetFileNameWithoutExtension(filePath);

            for (int i = 0; i < pages.Count; i++)
            {
                string target = Path.Combine(folder, baseName + " (" + (i + 1) + ")" + extension);

                if (convert)
                    SavePicture(pages[i], target);
                else
                    File.Copy(pages[i], target, true);
            }
        }

        private static void SavePicture(string source, string target)
        {
            ImageMagickSetup.EnsureStarted();

            using (var image = new MagickImage(source))
            {
                image.Format = string.Equals(Path.GetExtension(target), ".png", StringComparison.OrdinalIgnoreCase)
                    ? MagickFormat.Png
                    : MagickFormat.Jpeg;

                image.Write(target);
            }
        }
    }
}