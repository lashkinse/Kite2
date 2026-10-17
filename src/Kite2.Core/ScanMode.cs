namespace Kite2.Core
{
    /// <summary>
    /// Как страница попадает в документ. Режим задаёт кодировку: цветной скан
    /// остаётся цветным, чёрно-белый уходит в PDF оттенками серого.
    /// </summary>
    public enum ScanMode
    {
        /// <summary>Цветной скан, JPEG без потерь.</summary>
        Colour,

        /// <summary>Чёрно-белое сканирование, оттенки серого без отсечки по порогу.</summary>
        Greyscale
    }

    /// <summary>Цветовой режим TWAIN-драйвера, каким он понимает запрос.</summary>
    public enum CaptureColour
    {
        /// <summary>Чёрно-белый режим.</summary>
        BlackAndWhite,

        /// <summary>Цветной режим.</summary>
        Colour
    }
}