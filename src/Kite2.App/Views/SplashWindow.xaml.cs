using System.Windows;

namespace Kite2.App.Views
{
    // Сплеш на время старта: тяжёлая инициализация идёт в фоне, окно программы
    // мелькает раньше готовности. Прячется сразу после показа главного окна.
    public partial class SplashWindow : Window
    {
        public SplashWindow(string version)
        {
            InitializeComponent();

            VersionText.Text = "версия " + version;
        }

        // Панель хода: «Очистка временных файлов…» и подобные строки старта.
        public void SetStatus(string text) => StatusText.Text = text;
    }
}
