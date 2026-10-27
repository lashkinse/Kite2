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

        // Файл пишется рядом под временным именем и переезжает на место после записи:
        // обрывок посреди сборки PDF не должен выглядеть готовым документом.
        public static void Export(IReadOnlyList<string> pages, string filePath, ScanMode mode)
        {
            if (pages == null || pages.Count == 0)
                return;

            string extension = Path.GetExtension(filePath);

            if (!IsKnownExtension(extension))
                throw new NotSupportedException("Формат файла не поддерживается: " + extension);

            string folder = Path.GetDirectoryName(filePath) ?? ".";
            string name = Path.GetFileNameWithoutExtension(filePath);

            // Метка в середине имени: у нескольких картинок («письмо (1).jpg»)
            // она должна уехать из черновика вместе с файлами.
            string mark = "." + Path.GetRandomFileName().Replace(".", string.Empty);
            string draft = Path.Combine(folder, name + mark + extension);

            try
            {
                Write(pages, draft, mode, extension);

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
            else
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
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        // PDF собирает форк PDFsharp 1.50 — NAPS2.PdfSharp: сам PDFsharp 6-й версии
        // AGPL, как и iTextSharp, а этот форк под MIT.
        private static void SaveAsPdf(IReadOnlyList<string> pages, string filePath, ScanMode mode)
        {
            var document = new PdfDocument();

            foreach (string pageFile in pages)
            {
                PdfPage page = document.AddPage();
                page.Size = PdfSharpCore.PageSize.A4;

                // Вписывание с сохранением пропорций и выравниванием по центру.
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

        // Поставщик страниц под контракт IImageSource форка PDFsharp.
        private sealed class BitmapPageSource : IImageSource
        {
            private readonly byte[] _bytes;
            private readonly string _path;
            private readonly bool _jpeg;

            // flatGrey запрещает JPEG-путь: серая страница — однокомпонентный JPEG,
            // а сборщик кладёт JPEG в PDF только как DeviceRGB.
            public BitmapPageSource(string path, bool flatGrey)
            {
                _path = path;
                _bytes = File.ReadAllBytes(path);

                using (var stream = new MemoryStream(_bytes))
                using (var image = new Bitmap(stream))
                {
                    Width = image.Width;
                    Height = image.Height;

                    // Формат определяется по содержимому, а не по расширению.
                    // Полное имя типа — без него ImageFormat указывает на
                    // одноимённое свойство интерфейса.
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
                    // Без пересжатия: байты уезжают в документ как есть.
                    stream.Write(_bytes, 0, _bytes.Length);
                    return;
                }

                using (var source = new MemoryStream(_bytes))
                using (var image = new Bitmap(source))
                    image.Save(stream, JpegEncoder(), JpegParameters(92));
            }

            public void SaveAsPdfBitmap(MemoryStream stream)
            {
                // Контракт сборщика: буфер как у LockBits в Format24bppRgb —
                // BGR-байты, строки сверху, каждая добита до 4 байт.
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
                // Палитровые страницы не производятся: страница в PDF либо JPEG,
                // либо плоский RGB.
                throw new NotSupportedException("Однобитные страницы в PDF не вкладываются.");
            }

            // Кодек ищется один раз: страницы PDF не должны на каждом листе
            // перечислять все кодеки сначала.
            private static ImageCodecInfo jpegCodec;

            private static ImageCodecInfo JpegEncoder()
            {
                if (jpegCodec == null)
                {
                    foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
                    {
                        if (codec.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid)
                        {
                            jpegCodec = codec;
                            break;
                        }
                    }
                }

                if (jpegCodec == null)
                    throw new InvalidOperationException("Кодек JPEG не найден.");

                return jpegCodec;
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
                    // Нумерация в имени даёт при распаковке исходный порядок.
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
                    // Картинка не освобождается после добавления: коллекция пишется
                    // в файл только в конце.
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

        // Под именем .png нельзя оставить JPEG: некоторые просмотрщики такие
        // файлы не открывают, поэтому страница перекодируется.
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