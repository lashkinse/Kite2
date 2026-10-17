using System;
using System.IO;
using Microsoft.Win32;
using Kite2.Core;

namespace Kite2.App.Services
{
    /// <summary>
    /// Окно сохранения документа.
    /// </summary>
    /// <remarks>
    /// Список форматов и расширения описаны в ядре: расширение определяет, как именно
    /// соберётся документ, и разрыв его тут означал бы, что интерфейс и ядро
    /// расходятся во мнении о том, что вообще можно сохранить.
    /// </remarks>
    public static class SaveFilePicker
    {
        /// <summary>
        /// Спрашивает имя файла под указанный формат.
        /// </summary>
        /// <param name="filter">Фильтр окна сохранения с расширением, например «PDF|*.pdf».</param>
        /// <returns>Путь к файлу или null, если пользователь отказался.</returns>
        public static string Ask(string filter)
        {
            var dialog = new SaveFileDialog
            {
                AddExtension = true,
                RestoreDirectory = true,
                Filter = filter,
                DefaultExt = Path.GetExtension(filter.Split('|')[1].Trim('*'))
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        /// <summary>Фильтр «Документ PDF» для окна сохранения.</summary>
        public const string FilterPdf = "Документ PDF|*.pdf";

        /// <summary>Фильтр «Архив ZIP» для окна сохранения.</summary>
        public const string FilterZip = "Архив ZIP|*.zip";

        /// <summary>Фильтр «Изображение JPEG» для окна сохранения.</summary>
        public const string FilterJpg = "Изображение JPEG|*.jpg";

        /// <summary>Фильтр «Изображение TIFF» для окна сохранения.</summary>
        public const string FilterTiff = "Изображение TIFF|*.tif";
    }
}