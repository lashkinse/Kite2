using System.Windows;
using System.Windows.Input;

namespace Kite2.App.Views
{
    // «+» и Enter продолжают сканирование, Esc завершает. В прежней версии окно
    // ждало ещё и нажатия «ОК», а Esc просто закрывал его, ни о чём не сказав.
    public partial class ScanContinueWindow : Window
    {
        public ScanContinueWindow()
        {
            InitializeComponent();
        }

        public bool ContinueScan { get; private set; }

        // Закрытие крестиком считается отказом.
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