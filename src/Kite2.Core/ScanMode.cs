namespace Kite2.Core
{
    // Режим задаёт кодировку страницы: цветной скан остаётся цветным, чёрно-белый
    // уходит в PDF оттенками серого, без отсечки по порогу.
    public enum ScanMode
    {
        Colour,
        Greyscale
    }

    public enum CaptureColour
    {
        BlackAndWhite,
        Colour
    }
}