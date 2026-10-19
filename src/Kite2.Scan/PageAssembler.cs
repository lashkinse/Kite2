using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NLog;
using Saraff.Twain;

namespace Kite2.Scan
{
    // Драйвер отдаёт страницу не цельным файлом, а кусками буферов: SetupMemXfer
    // открывает страницу, MemXfer даёт строки, XferDone её закрывает. BMP собирается
    // из этих кусков здесь: именно памятью отдаёт страницы драйвер Kyocera (по этому
    // же пути идёт работающий с ним инструмент прежней программы). Натурный и
    // файловый механизмы остаются запасными.
    internal sealed class PageAssembler
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly Twain32 _twain;
        private readonly string _directory;
        private readonly List<string> _pages;

        private readonly List<byte[]> _chunks = new List<byte[]>();

        private Twain32.ImageInfo _info;
        private int _rows;
        private int _columns;
        private int _bytesPerRow;
        private bool _msbFirst;
        private int _index;

        public PageAssembler(Twain32 twain, string directory, List<string> pages)
        {
            _twain = twain;
            _directory = directory;
            _pages = pages;
        }

        // Новая страница начинается: буферы предыдущей уже ушли на диск.
        public void OnSetupMemXfer(object sender, Twain32.SetupMemXferEventArgs args)
        {
            _info = args.ImageInfo;
            _chunks.Clear();
            _rows = 0;
            _columns = 0;
            _bytesPerRow = 0;

            try
            {
                _msbFirst = _twain.Capabilities.BitOrder.GetCurrent() == TwBO.MsbFirst;
            }
            catch (Exception)
            {
                // Драйвер вправе и не отвечать о порядке байтов: ожидаем обычный.
                _msbFirst = false;
            }
        }

        public void OnMemXfer(object sender, Twain32.MemXferEventArgs args)
        {
            var piece = args.ImageMemXfer;

            if (_bytesPerRow == 0)
                _bytesPerRow = (int)piece.BytesPerRow;
            else if ((int)piece.BytesPerRow != _bytesPerRow)
                Log.Warn("Драйвер меняет разбивку строки: было {0}, стало {1}", _bytesPerRow, piece.BytesPerRow);

            if (piece.ImageData == null)
            {
                Log.Warn("Кусок страницы пуст, строк пропущено: {0}", piece.Rows);
                return;
            }

            _chunks.Add(piece.ImageData);
            _rows += (int)piece.Rows;
            _columns = Math.Max(_columns, (int)piece.Columns);
        }

        // Сборка страницы для передачи памятью. Натурный и файловый механизмы
        // проходят через этот обработчик без накопленных кусков.
        public void OnXferDone(object sender, Twain32.XferDoneEventArgs args)
        {
            if (_chunks.Count > 0)
                FlushMemoryPage();
        }

        // Запасной натурный механизм: драйвер отдал страницу одним целым.
        public void OnEndXfer(object sender, Twain32.EndXferEventArgs args)
        {
            if (args.Image == null)
            {
                Log.Warn("Страница не сохранена: драйвер отдал пустой образ");
                return;
            }

            string path = NextPath();
            SaveDrawing(args.Image, path);
            _pages.Add(path);
        }

        // Запасной файловый механизм: страницу писал сам драйвер, файл остаётся
        // забрать и удалить.
        public void OnFileXfer(object sender, Twain32.FileXferEventArgs args)
        {
            string source = args.ImageFileXfer.FileName;
            string path = NextPath();

            if (File.Exists(path))
                File.Delete(path);

            File.Copy(source, path, true);
            File.Delete(source);

            _pages.Add(path);
            Log.Info("Страница принята от драйвера целиком: {0}", path);
        }

        private void FlushMemoryPage()
        {
            var chunks = _chunks.ToArray();
            int rows = _rows;
            int columns = _info.ImageWidth;
            int bytesPerRow = _bytesPerRow;

            _chunks.Clear();
            _rows = 0;

            if (columns <= 0)
                columns = _columns;

            if (columns <= 0 || rows <= 0 || bytesPerRow <= 0)
            {
                Log.Error("Страница не сохранена: драйвер не отдал размеров ({0}x{1}, строка {2})", columns, rows, bytesPerRow);
                return;
            }

            // Сжатые данные не раскрываются: сборка знает только сырые строки.
            if (_info.Compression != TwCompression.None)
            {
                Log.Error("Страница не сохранена: драйвер отдал сжатые данные ({0})", _info.Compression);
                return;
            }

            int bitsPerPixel = _info.BitsPerPixel;

            if (bitsPerPixel != 1 && bitsPerPixel != 8 && bitsPerPixel != 24)
            {
                Log.Error("Страница не сохранена: глубина {0} бит не поддерживается", bitsPerPixel);
                return;
            }

            // Значащие байты строки без выравнивания; до двойного слова выравнивает
            // сам BMP, а драйвер выравнивает по-своему.
            int payload = bitsPerPixel == 1 ? (columns + 7) / 8 : columns * (bitsPerPixel / 8);
            int targetStride = ((columns * bitsPerPixel + 31) / 32) * 4;
            var data = new byte[rows * targetStride];

            int cursor = 0;

            foreach (byte[] chunk in chunks)
            {
                int chunkRows = chunk.Length / bytesPerRow;

                if (chunk.Length % bytesPerRow != 0)
                    Log.Warn("Кусок не доложил целую строку: {0} байт при строке {1}", chunk.Length, bytesPerRow);

                for (int row = 0; row < chunkRows; row++)
                {
                    int source = row * bytesPerRow;
                    // BMP без сжатия хранит строки снизу вверх, а драйвер отдаёт
                    // сверху вниз: строки укладываются с конца.
                    int target = (rows - 1 - cursor) * targetStride;
                    cursor++;

                    if (bitsPerPixel == 1)
                    {
                        // BMP без сжатия всегда читает пиксели от старшего бита,
                        // а драйвер отдаёт строки по своему ICAP_BITORDER.
                        for (int i = 0; i < payload; i++)
                        {
                            byte value = chunk[source + i];
                            data[target + i] = _msbFirst ? value : MirrorBits(value);
                        }
                    }
                    else if (bitsPerPixel == 24)
                    {
                        // У TWAIN цвет идёт как R,G,B, у BMP — как B,G,R.
                        for (int pixel = 0; pixel < columns; pixel++)
                        {
                            data[target + pixel * 3] = chunk[source + pixel * 3 + 2];
                            data[target + pixel * 3 + 1] = chunk[source + pixel * 3 + 1];
                            data[target + pixel * 3 + 2] = chunk[source + pixel * 3];
                        }
                    }
                    else
                    {
                        Array.Copy(chunk, source, data, target, payload);
                    }
                }
            }

            byte[] palette = bitsPerPixel == 1 ? BlackAndWhitePalette() : bitsPerPixel == 8 ? BuildPalette() : new byte[0];
            int headerSize = 14 + 40 + palette.Length;

            string path = SaveFile(headerSize, data, bitsPerPixel, columns, rows, palette);

            _pages.Add(path);
            Log.Info("Страница {0}: {1}x{2}, {3} бит", _pages.Count, columns, rows, bitsPerPixel);
        }

        private string SaveFile(int headerSize, byte[] data, int bitsPerPixel, int columns, int rows, byte[] palette)
        {
            string path = NextPath();

            if (File.Exists(path))
                File.Delete(path);

            string temporary = path + ".part";

            using (var stream = File.Create(temporary))
            {
                // Стандартный BMP: строки снизу вверх, высота положительная.
                byte[] header = new byte[headerSize];

                header[0] = (byte)'B';
                header[1] = (byte)'M';
                PutInt(header, 2, headerSize + data.Length);
                PutInt(header, 10, headerSize);

                PutInt(header, 14, 40);
                PutInt(header, 18, columns);
                PutInt(header, 22, rows);
                header[26] = 1;
                PutShort(header, 28, (ushort)bitsPerPixel);
                PutInt(header, 30, 0);
                PutInt(header, 34, data.Length);
                PutInt(header, 46, palette.Length / 4);

                if (palette.Length > 0)
                    Array.Copy(palette, 0, header, 54, palette.Length);

                stream.Write(header, 0, header.Length);
                stream.Write(data, 0, data.Length);
            }

            File.Move(temporary, path);

            return path;
        }

        // Порядок в BMP: B, G, R, резерв; нулевой индекс — чёрный, единичный — белый.
        private static byte[] BlackAndWhitePalette()
        {
            var palette = new byte[8];
            palette[4] = 255;
            palette[5] = 255;
            palette[6] = 255;

            return palette;
        }

        private byte[] BuildPalette()
        {
            var palette = new byte[1024];

            if (_info.PixelType == TwPixelType.Palette)            {
                try
                {
                    var colors = _twain.Palette.Get().Colors;

                    for (int i = 0; i < colors.Length && i < 256; i++)
                    {
                        palette[i * 4] = colors[i].B;
                        palette[i * 4 + 1] = colors[i].G;
                        palette[i * 4 + 2] = colors[i].R;
                    }

                    return palette;
                }
                catch (Exception ex)
                {
                    Log.Warn("Палитра драйвера не получена, заменяю серой: {0}", ex.Message);
                }
            }

            // Оттенок серого: у чёрно-белых сканов драйвер нередко отдаёт 8 бит.
            for (int i = 0; i < 256; i++)
            {
                palette[i * 4] = (byte)i;
                palette[i * 4 + 1] = (byte)i;
                palette[i * 4 + 2] = (byte)i;
            }

            return palette;
        }

        private static byte MirrorBits(byte value)
        {
            byte mirrored = 0;

            for (int i = 0; i < 8; i++)
                mirrored = (byte)((mirrored << 1) | ((value >> i) & 1));

            return mirrored;
        }

        private static void PutInt(byte[] data, int offset, int value)
        {
            uint bits = unchecked((uint)value);

            data[offset] = (byte)bits;
            data[offset + 1] = (byte)(bits >> 8);
            data[offset + 2] = (byte)(bits >> 16);
            data[offset + 3] = (byte)(bits >> 24);
        }

        private static void PutShort(byte[] data, int offset, ushort value)
        {
            data[offset] = (byte)value;
            data[offset + 1] = (byte)(value >> 8);
        }

        private string NextPath()
        {
            return Path.Combine(_directory, "scan" + (++_index).ToString("D3") + ".bmp");
        }

        private static void SaveDrawing(Image image, string path)
        {
            string temporary = path + ".part";

            using (image)
                image.Save(temporary, ImageFormat.Bmp);

            File.Move(temporary, path);
        }
    }
}
