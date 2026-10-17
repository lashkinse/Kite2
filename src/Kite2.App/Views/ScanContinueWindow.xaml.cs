using System.Windows;
using System.Windows.Input;

namespace Kite2.App.Views
{
    /// <summary>
    /// Вопрос после серии сканирования: сканировать ещё или завершить документ.
    /// </summary>
    /// <remarks>
    /// Клавиши работают и кнопками, и с клавиатуры: «+» на цифровой клавиатуре или
    /// рядом с ней — сканировать ещё, Enter или Esc — завершить. В прежней версии
    /// окно ждало ещё и нажатия «ОК», а Esc просто закрывал его, ни о чём не сказав.
    /// </remarks>
    public partial class ScanContinueWindow : Window
    {
        public ScanContinueWindow()
        {
            InitializeComponent();
        }

        /// <summary>Запрошены ли дополнительные страницы.</summary>
        public bool ContinueScan { get; private set; }

        /// <summary>
        /// Показывает вопрос и возвращает ответ. false — пользователь завершил
        /// документ или закрыл окно.
        /// </summary>
        public static bool Ask(Window owner)
        {
            var window = new ScanContinueWindow { Owner = owner };

            window.ShowDialog();

            return window.ContinueScan;
        }

        private void OnStopClick(object sender, RoutedEventArgs e) => Accept(false);

        private void OnContinueClick(object sender, RoutedEventArgs e) => Accept(true);

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Add:
                case Key.OemPlus:
                    Accept(true);
                    e.Handled = true;
                    return;

                case Key.Escape:
                    // Esc закрывает окно без ответа, и это читается как «завершить».
                    Accept(false);
                    e.Handled = true;
                    return;
            }

            base.OnPreviewKeyDown(e);
        }

        private void Accept(bool continueScan)
        {
            ContinueScan = continueScan;

            DialogResult = true;
            Close();
        }
    }
}