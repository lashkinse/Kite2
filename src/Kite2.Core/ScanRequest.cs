using System;
using System.IO;

namespace Kite2.Core
{
    /// <summary>
    /// Запрос на захват страниц: источник, разрешение и режим цвета.
    /// </summary>
    /// <remarks>
    /// Каталог результатов создаётся здесь же и перед каждой операцией очищается:
    /// драйвер пишет в него файлы с именем scan1, scan2 и так далее, и старые сканы
    /// прошлого запуска иначе попали бы в новый документ.
    /// </remarks>
    public sealed class ScanRequest
    {
        /// <summary>Индекс TWAIN-источника в списке устройств.</summary>
        public int SourceIndex { get; set; } = AppSettings.NotSelected;

        /// <summary>Разрешение захвата в точках на дюйм.</summary>
        public int Dpi { get; set; } = 300;

        /// <summary>Режим цвета.</summary>
        public CaptureColour Colour { get; set; } = CaptureColour.BlackAndWhite;

        /// <summary>Режим, которым страницы попадут в документ.</summary>
        public ScanMode Mode =>
            Colour == CaptureColour.BlackAndWhite ? ScanMode.Greyscale : ScanMode.Colour;

        /// <summary>
        /// Каталог, куда драйвер складывает сканы. Очищается при создании запроса.
        /// </summary>
        public string Directory { get; }

        public ScanRequest()
        {
            Directory = AppPaths.Scan;

            AppPaths.EmptyFolder(Directory);
        }

        /// <summary>Отбрасывает всё, что накопилось в каталоге результатов.</summary>
        public void ClearResults() => AppPaths.EmptyFolder(Directory);
    }
}