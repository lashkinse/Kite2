using System.IO;
using Microsoft.Win32;

namespace Kite2.App.Services
{
    // Все форматы перечислены в одном окне: отдельный список под кнопкой «Сохранить»
    // повторял то, что окно и так спрашивает, и требовал второго щелчка. Расширение
    // берётся из ядра — иначе интерфейс и ядро разошлись бы во мнении о том, что
    // вообще можно сохранить.
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

        // Формат фрагмента один — PNG: он идёт в документ или в сообщение, где сжатие
        // JPEG испортило бы мелкий текст, а остальные форматы для куска страницы не
        // годятся вовсе.
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

            // Выбранный по умолчанию формат задан расширением: порядок строк фильтра
            // выбирает первый, а без заданного расширения окно открывалось с «Все
            // файлы» и про формат приходилось гадать.
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public static string ExtensionOf(string fileName) => Path.GetExtension(fileName);
    }
}
