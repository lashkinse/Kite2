using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Kite2.Core;

namespace Kite2.App.ViewModels
{
    /// <summary>
    /// Одна отсканированная страница в колонке страниц.
    /// </summary>
    /// <remarks>
    /// Миниатюра грузится сразу и в нужном размере: тянуть в память лист A4 ради
    /// прямоугольника 110 на 156 пикселей незачем, а колонка перерисовывается при
    /// каждом щелчке.
    /// </remarks>
    public sealed class PageViewModel : ObservableObject
    {
        /// <summary>Ширина миниатюры в колонке страниц.</summary>
        private const int ThumbWidth = 110;

        /// <summary>Путь к файлу страницы на диске.</summary>
        public string FilePath { get; }

        private int _number;

        /// <summary>
        /// Номер страницы, начиная с единицы. Принадлежит позиции в документе, а не
        /// самой странице: после удаления или перестановки номера должны сойтись
        /// снова, иначе в колонке остаются пропуски и повторы.
        /// </summary>
        public int Number
        {
            get => _number;
            set
            {
                if (SetProperty(ref _number, value))
                    OnPropertyChanged(nameof(Caption));
            }
        }

        /// <summary>Номер страницы подписью.</summary>
        public string Caption => Number.ToString();

        /// <summary>Миниатюра для колонки страниц.</summary>
        public ImageSource Thumbnail { get; private set; }

        /// <summary>Страница целиком, для области просмотра.</summary>
        public ImageSource Page { get; private set; }

        public PageViewModel(string filePath, int number)
        {
            FilePath = filePath;
            Number = number;
            Thumbnail = Load(filePath, ThumbWidth);
            Page = Load(filePath, 0);
        }

        /// <summary>
        /// Перечитывает картинки после изменения файла на диске. После поворота
        /// страница прежняя только по пути, а на экране уже другая: без перечитывания
        /// колонка и просмотр показали бы старое изображение.
        /// </summary>
        public void Reload()
        {
            Thumbnail = Load(FilePath, ThumbWidth);
            Page = Load(FilePath, 0);

            OnPropertyChanged(nameof(Thumbnail));
            OnPropertyChanged(nameof(Page));
        }

        /// <summary>
        /// Читает страницу из файла. Файл копируется в память и сразу закрывается:
        /// страницы перезаписываются при повороте, и пока файл открыт, его нельзя
        /// менять на диске.
        /// </summary>
        private static ImageSource Load(string filePath, int decodeWidth)
        {
            var image = new BitmapImage();
            image.BeginInit();

            // Кэширование на диске выключено: файл перезаписывается на месте, и
            // закэшированная версия показала бы старую страницу после поворота.
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.None;

            if (decodeWidth > 0)
                image.DecodePixelWidth = decodeWidth;

            image.UriSource = new Uri(filePath, UriKind.Absolute);
            image.EndInit();
            image.Freeze();

            return image;
        }

        /// <summary>Освобождает страницу с диска: она больше не нужна программе.</summary>
        public void Delete()
        {
            try
            {
                File.Delete(FilePath);
            }
            catch (IOException)
            {
                // Файл занят просмотром: удалится при следующей очистке каталога.
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}