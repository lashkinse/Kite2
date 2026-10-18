using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Kite2.Core;

namespace Kite2.App.ViewModels
{
    // Миниатюра грузится сразу и в нужном размере: тянуть в память лист A4 ради
    // прямоугольника 110 на 156 пикселей незачем, а колонка перерисовывается при
    // каждом щелчке.
    public sealed class PageViewModel : ObservableObject
    {
        private const int ThumbWidth = 110;

        public string FilePath { get; }

        private int _number;

        // Номер принадлежит позиции в документе, а не самой странице: после удаления
        // или перестановки номера должны сойтись снова, иначе в колонке остаются
        // пропуски и повторы.
        public int Number
        {
            get => _number;
            set
            {
                if (SetProperty(ref _number, value))
                    OnPropertyChanged(nameof(Caption));
            }
        }

        public string Caption => Number.ToString();

        public ImageSource Thumbnail { get; private set; }

        public ImageSource Page { get; private set; }

        public PageViewModel(string filePath, int number)
        {
            FilePath = filePath;
            Number = number;
            Thumbnail = Load(filePath, ThumbWidth);
            Page = Load(filePath, 0);
        }

        // После поворота страница прежняя только по пути, а на экране уже другая:
        // без перечитывания колонка и просмотр показали бы старое изображение.
        public void Reload()
        {
            Thumbnail = Load(FilePath, ThumbWidth);
            Page = Load(FilePath, 0);

            OnPropertyChanged(nameof(Thumbnail));
            OnPropertyChanged(nameof(Page));
        }

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

        // Файл может быть занят просмотром — тогда он удалится при следующей очистке
        // каталога.
        public void Delete()
        {
            try
            {
                File.Delete(FilePath);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}