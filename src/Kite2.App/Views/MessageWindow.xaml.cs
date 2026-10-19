using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using NLog;

namespace Kite2.App.Views
{
    public enum MessageKind
    {
        Error,
        Question
    }

    // Отдельное окно, а не MessageBox: у MessageBox из WPF нет привязки к окну,
    // он всегда встаёт по центру экрана, а не программы.
    public partial class MessageWindow : Window
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private MessageWindow()
        {
            InitializeComponent();
        }

        private bool Answer { get; set; }

        public static void Show(Window owner, string title, string text, MessageKind kind)
        {
            var window = Create(owner, title);

            window.Fill(text, kind);

            window.OkButton.Visibility = Visibility.Visible;

            window.ShowDialog();
        }

        public static bool Ask(Window owner, string title, string text, string yes, string no)
        {
            var window = Create(owner, title);

            window.Fill(text, MessageKind.Question);

            window.YesButton.Content = yes + "  [Enter]";
            window.NoButton.Content = no + "  [Esc]";
            window.YesButton.Visibility = Visibility.Visible;
            window.NoButton.Visibility = Visibility.Visible;

            window.ShowDialog();

            return window.Answer;
        }

        private static MessageWindow Create(Window owner, string title)
        {
            var window = new MessageWindow { Title = title };

            if (owner != null && owner.IsVisible)
            {
                window.Owner = owner;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            return window;
        }

        private void Fill(string text, MessageKind kind)
        {
            Caption.Text = text;

            if (kind == MessageKind.Error)
            {
                BadgeText.Data = (Geometry)FindResource("IconAlert");
                Badge.Fill = (Brush)FindResource("Danger");
            }
            else
            {
                BadgeText.Data = (Geometry)FindResource("IconQuestion");
                Badge.Fill = (Brush)FindResource("Accent");
            }
        }

        private void OnOkClick(object sender, RoutedEventArgs e) => Accept(true);

        private void OnYesClick(object sender, RoutedEventArgs e) => Accept(true);

        private void OnNoClick(object sender, RoutedEventArgs e) => Accept(false);

        private void Accept(bool answer)
        {
            // Ответ пишется в журнал: видно, на что согласился пользователь.
            Log.Info("Ответ в окне «{0}»: {1}", Title, answer ? "да" : "нет");

            Answer = answer;

            DialogResult = true;

            Close();
        }
    }
}
