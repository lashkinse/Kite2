using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using ImageMagick;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

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

        // PDF собирает форк PDFsharp 1.50 — NAPS2.PdfSharp (сам PDFsharp 6-й версии
        // уже AGPL, и iTextSharp был AGPL тоже, а этот форк — под MIT). Вкладка
        // страниц происходит как у прежнего сборщика: без полей и с вписыванием
        // пропорций; серые страницы уже серые с этапа композиции, поэтому
        // отдельной конвертации, как раньше перед сборкой PDF, больше нет.
        private static void SaveAsPdf(IReadOnlyList<string> pages, string filePath, ScanMode mode)
        {
            var document = new PdfDocument();

            foreach (string pageFile in pages)
            {
                PdfPage page = document.AddPage();
                page.Size = PdfSharpCore.PageSize.A4;

                // Вписывание с сохранением пропорций: обычный лист совпадает с A4
                // ровно, а перевёрнутый или нестандартный скан лёг без вытягивания,
                // и выравнивается по центру листа.
                using (XGraphics graphics = XGraphics.FromPdfPage(page))
                {
                    var source = new BitmapPageSource(pageFile, mode == ScanMode.Greyscale);

                    double scale = Math.Min(
                        page.Width.Point / source.Width,
                        page.Height.Point / source.Height);

                    double width = source.Width * scale;
                    double height = source.Height * scale;

                    graphics.DrawImage(
                        XImage.FromImageSource(source),
                        (page.Width.Point - width) / 2,
                        0,
                        width,
                        height);
                }
            }

            document.Save(filePath);
            document.Close();
        }

        // Поставщик страниц для сборщика PDF: форк PDFsharp тянет картинку своим
        // контрактом IImageSource (в NAPS2 под то же место своя реализация), а у нас
        // всё рисуется на System.Drawing.
        private sealed class BitmapPageSource : IImageSource
        {
            private readonly byte[] _bytes;
            private readonly string _path;
            private readonly bool _jpeg;

            // flatGrey запрещает JPEG-путь: серая страница — однокомпонентный JPEG,
            // а сборщик объявляет JPEG внутри PDF только как DeviceRGB, и
            // обсуждение формата зависело бы от снисходительности просмотрщиков.
            public BitmapPageSource(string path, bool flatGrey)
            {
                _path = path;
                _bytes = File.ReadAllBytes(path);

                using (var stream = new MemoryStream(_bytes))
                using (var image = new Bitmap(stream))
                {
                    Width = image.Width;
                    Height = image.Height;

                    // Определяется формат по содержимому, а не по расширению:
                    // внутри документа расхождение имени и содержимого недопустимо.
                    // Имя типа уточняется полностью: без него слово ImageFormat
                    // указывает на одноимённое свойство интерфейса.
                    _jpeg = image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg) && !flatGrey;
                }

                ImageFormat = _jpeg ? XImageFormat.Jpeg : XImageFormat.Rgb24;
            }

            public int Width { get; }

            public int Height { get; }

            public string Name => _path;

            public XImageFormat ImageFormat { get; }

            public void SaveAsJpeg(MemoryStream stream)
            {
                if (_jpeg)
                {
                    // Пересжатие ухудшило бы качество и копило артефакты: байты
                    // уезжают в документ как есть.
                    stream.Write(_bytes, 0, _bytes.Length);
                    return;
                }

                using (var source = new MemoryStream(_bytes))
                using (var image = new Bitmap(source))
                    image.Save(stream, JpegEncoder(), JpegParameters(92));
            }

            public void SaveAsPdfBitmap(MemoryStream stream)
            {
                // Контракт сборщика (PdfSharpCore.Pdf.Advanced.PdfImage): буфер
                // как у LockBits в Format24bppRgb — BGR-байты, верхние строки
                // первыми, каждая строка добита до 4 байт. Переворот строк и
                // порядок каналов сборщик разбирает сам.
                using (var source = new MemoryStream(_bytes))
                using (var image = new Bitmap(source))
                {
                    var rectangle = new Rectangle(0, 0, image.Width, image.Height);
                    var data = image.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                    try
                    {
                        var buffer = new byte[data.Stride * image.Height];

                        Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);
                        stream.Write(buffer, 0, buffer.Length);
                    }
                    finally
                    {
                        image.UnlockBits(data);
                    }
                }
            }

            public void SaveAsPdfIndexedBitmap(MemoryStream stream)
            {
                // Палитровые страницы не производятся: страница в документе либо
                // JPEG, либо плоский RGB.
                throw new NotSupportedException("Однобитные страницы в PDF не вкладываются.");
            }

            private static ImageCodecInfo JpegEncoder()
            {
                foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
                {
                    if (codec.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid)
                        return codec;
                }

                throw new InvalidOperationException("Кодек JPEG не найден.");
            }

            private static EncoderParameters JpegParameters(int quality)
            {
                var parameters = new EncoderParameters(1);
                parameters.Param[0] = new EncoderParameter(Encoder.Quality, (long)quality);

                return parameters;
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