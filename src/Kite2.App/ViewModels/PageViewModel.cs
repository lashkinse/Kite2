using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Kite2.Core;

namespace Kite2.App.ViewModels
{
    // Миниатюра грузится сразу и в маленьком размере; полная страница — при первом
    // показе в области просмотра. Держать в памяти десяток листов A4 ради колонки
    // миниатюр незачем.
    public sealed class PageViewModel : ObservableObject
    {
        private const int ThumbWidth = 110;

        public string FilePath { get; }

        private ImageSource _page;

        private int _number;

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

        public ImageSource Page
        {
            get
            {
                if (_page == null)
                {
                    _page = Load(FilePath, 0);
                    OnPropertyChanged();
                }

                return _page;
            }
        }

        public PageViewModel(string filePath, int number)
        {
            FilePath = filePath;
            Number = number;
            Thumbnail = Load(filePath, ThumbWidth);
        }

        // После поворота файл прежний, а картинка уже другая: без перечитывания
        // колонка и просмотр показали бы старое изображение.
        public void Reload()
        {
            Thumbnail = Load(FilePath, ThumbWidth);
            _page = null;

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
