using System.IO;
using Microsoft.Win32;

namespace Kite2.App.Services
{
    /// <summary>
    /// Окно сохранения документа.
    /// </summary>
    /// <remarks>
    /// Все форматы перечислены в одном окне. Отдельный список под кнопкой «Сохранить»
    /// повторял то, что окно и так спрашивает, и требовал второго щелчка; теперь
    /// Ctrl+S сразу приводит сюда. Расширение определяет, как соберётся документ,
    /// и список расширений берётся из ядра — иначе интерфейс и ядро разошлись бы
    /// во мнении о том, что вообще можно сохранить.
    /// </summary>
    public static class SaveFilePicker
    {
        /// <summary>Человеческие названия форматов в порядке, показанном в окне.</summary>
        private static readonly string[] Formats =
        {
            "Документ PDF|*.pdf",
            "Архив ZIP|*.zip",
            "Изображение TIFF|*.tif",
            "Изображение JPEG|*.jpg",
            "Изображение PNG|*.png"
        };

        /// <summary>Фильтр окна сохранения со всеми форматами.</summary>
        public static string Filter => string.Join("|", Formats);

        /// <summary>
        /// Спрашивает имя файла под указанный формат.
        /// </summary>
        /// <returns>Путь к файлу или null, если пользователь отказался.</returns>
        public static string Ask(string defaultExtension)
        {
            return Show(Filter, defaultExtension);
        }

        /// <summary>
        /// Спрашивает имя файла для фрагмента страницы. Формат здесь один — PNG:
        /// фрагмент обычно идёт в документ или в сообщение, где сжатие JPEG
        /// испортило бы мелкий текст, а остальные форматы для куска страницы
        /// не годятся вовсе.
        /// </summary>
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

        /// <summary>
        /// Расширение из выбранного фильтра. Нужно, чтобы имя нового документа
        /// получило расширение заранее, а не дописывалось молча.
        /// </summary>
        public static string ExtensionOf(string fileName) => Path.GetExtension(fileName);
    }
}