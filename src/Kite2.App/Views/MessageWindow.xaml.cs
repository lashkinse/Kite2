using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using NLog;

namespace Kite2.App.Views
{
    /// <summary>Что сообщаем пользователю: ошибку или вопрос с двумя ответами.</summary>
    public enum MessageKind
    {
        Error,
        Question
    }

    /// <summary>
    /// Окно сообщения: ошибка или вопрос пользователю.
    /// </summary>
    /// <remarks>
    /// Отдельное окно, а не MessageBox, потому что у MessageBox из WPF нет
    /// привязки к окну: он всегда встаёт по центру экрана, а не программы.
    /// Надписи на кнопках содержат клавиши: Enter и Esc работают и с клавиатуры,
    /// и мышью, а подсказки об этом написаны прямо на кнопке.
    /// </remarks>
    public partial class MessageWindow : Window
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private MessageWindow()
        {
            InitializeComponent();
        }

        /// <summary>Ответ пользователя на вопрос. Для сообщения об ошибке не важно.</summary>
        private bool Answer { get; set; }

        /// <summary>Показывает сообщение с одной кнопкой «ОК».</summary>
        public static void Show(Window owner, string title, string text, MessageKind kind)
        {
            var window = Create(owner, title);

            window.Fill(text, kind);

            window.OkButton.Visibility = Visibility.Visible;

            window.ShowDialog();
        }

        /// <summary>
        /// Задаёт вопрос с двумя ответами. true — согласие пользователя.
        /// Закрытие окна крестиком считается отказом.
        /// </summary>
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

        /// <summary>
        /// Готовит окно. Без главного окна программы привязываться не к чему,
        /// и окно встаёт по центру экрана.
        /// </summary>
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
            // Ответ пишется в журнал: по журналу видно, на что согласился
            // пользователь, даже если потом разбираться будет поздно.
            Log.Info("Ответ в окне «{0}»: {1}", Title, answer ? "да" : "нет");

            Answer = answer;

            DialogResult = true;

            Close();
        }
    }
}