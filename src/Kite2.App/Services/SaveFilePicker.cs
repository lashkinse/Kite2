using Microsoft.Win32;

namespace Kite2.App.Services
{
    // Все форматы перечислены в одном окне; расширения задаёт ядро, чтобы
    // интерфейс и ядро не расходились во мнении о том, что можно сохранить.
    public static class SaveFilePicker
    {
        private static readonly string[] Formats =
        {
            "Документ PDF|*.pdf",
            "Архив ZIP|*.zip",
            "Изображение TIFF|*.tif",
            "Изображение JPEG|*.jpg",
            "Изображение PNG|*.png"
        };

        public static string Filter => string.Join("|", Formats);

        public static string Ask(string defaultExtension)
        {
            return Show(Filter, defaultExtension);
        }

        // Формат фрагмента один — PNG: сжатие JPEG испортило бы мелкий текст.
        public static string AskFragment()
        {
            return Show("Изображение PNG|*.png", ".png");
        }

        private static string Show(string filter, string defaultExtension)
        {
            var dialog = new SaveFileDialog
            {
                AddExtension = true,
                RestoreDirectory = true,
                Filter = filter,
                DefaultExt = defaultExtension
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
