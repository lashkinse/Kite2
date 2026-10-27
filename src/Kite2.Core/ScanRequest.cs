namespace Kite2.Core
{
    public sealed class ScanRequest
    {
        // Ни dpi, ни цвета: всё задаёт окно драйвера. Каталог очищается при
        // создании: иначе в новый документ попали бы сканы прошлой серии.
        public int SourceIndex { get; set; } = AppSettings.NotSelected;

        public string Directory { get; }

        public ScanRequest()
        {
            Directory = AppPaths.Scan;

            AppPaths.ClearFolder(Directory);
        }
    }
}
