using Kite2.Core;

namespace Kite2.Tests
{
    /// <summary>
    /// Проверки разбора файла настроек.
    /// </summary>
    public static class AppSettingsTests
    {
        [Test("Индекс источника читается из строки настроек")]
        public static void ReadsSourceIndex()
        {
            AppSettings.ScanSourceIndex = 3;

            AppSettings.Apply("ScanSourceIndex=5");

            Check.Equal(5, AppSettings.ScanSourceIndex, "индекс источника");
        }

        [Test("Регистр ключа и пробелы вокруг знака не важны")]
        public static void IgnoresCaseAndSpaces()
        {
            AppSettings.ScanSourceIndex = 0;

            AppSettings.Apply("  scansourceindex  =  7 ");

            Check.Equal(7, AppSettings.ScanSourceIndex, "индекс источника");
        }

        [Test("Комментарии и заголовки разделов пропускаются")]
        public static void SkipsCommentsAndSections()
        {
            AppSettings.ScanSourceIndex = 1;

            AppSettings.Apply("; комментарий");
            AppSettings.Apply("# тоже комментарий");
            AppSettings.Apply("[Scan]");
            AppSettings.Apply(string.Empty);

            Check.Equal(1, AppSettings.ScanSourceIndex, "индекс не изменился");
        }

        [Test("Чужие ключи и строки без знака не трогают настройки")]
        public static void IgnoresForeignLines()
        {
            AppSettings.ScanSourceIndex = 2;

            AppSettings.Apply("ДругойКлюч=9");
            AppSettings.Apply("просто текст");

            Check.Equal(2, AppSettings.ScanSourceIndex, "индекс не изменился");
        }

        [Test("Нечисловое значение оставляет прежний индекс")]
        public static void KeepsValueOnGarbage()
        {
            AppSettings.ScanSourceIndex = 4;

            AppSettings.Apply("ScanSourceIndex=не число");

            Check.Equal(4, AppSettings.ScanSourceIndex, "индекс не изменился");
        }
    }
}