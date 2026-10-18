namespace Kite2.Core
{
    public sealed class ScanRequest
    {
        // Драйвер пишет файлы с именами scan1, scan2 и так далее, поэтому каталог
        // очищается при создании запроса: иначе в новый документ попали бы сканы
        // прошлого запуска.
        public int SourceIndex { get; set; } = AppSettings.NotSelected;

        public int Dpi { get; set; } = 300;

        public CaptureColour Colour { get; set; } = CaptureColour.BlackAndWhite;

        public ScanMode Mode =>
            Colour == CaptureColour.BlackAndWhite ? ScanMode.Greyscale : ScanMode.Colour;

        public string Directory { get; }

        public ScanRequest()
        {
            Directory = AppPaths.Scan;

            AppPaths.EmptyFolder(Directory);
        }
    }
}