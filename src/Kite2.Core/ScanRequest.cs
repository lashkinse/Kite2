namespace Kite2.Core
{
    public sealed class ScanRequest
    {
        // Запрос не описывает ни dpi, ни цвет, ни режим: всё это человек задаст
        // сам в родном окне драйвера. Программа только называет источник и
        // каталог, куда забирать готовые страницы.
        //
        // Каталог очищается при создании запроса: иначе в новый документ попали бы
        // сканы прошлого запуска.
        public int SourceIndex { get; set; } = AppSettings.NotSelected;

        public string Directory { get; }

        public ScanRequest()
        {
            Directory = AppPaths.Scan;

            AppPaths.ClearFolder(Directory);
        }
    }
}
