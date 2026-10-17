using System.Windows;
using System.Windows.Input;

namespace Kite2.App.Views
{
    /// <summary>
    /// Главное окно программы.
    /// </summary>
    /// <remarks>
    /// Горячие клавиши задаются в разметке как привязки ввода: в WPF они не требуют
    /// перехвата клавиш в коде и работают, когда фокус на любом элементе окна,
    /// включая колонку страниц и область просмотра.
    /// Escape перехватывается в коде: он не команда, а снять выделение, и привязкой
    /// ввода это не выражается.
    /// </remarks>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            PreviewKeyDown += OnPreviewKeyDown;
        }

        /// <summary>
        /// Escape снимает выделение на листе. Перехват стоит на окне, а не на
        /// области просмотра: фокус после щелчка по колонке страниц остаётся в
        /// колонке, и выделение на листе тогда снять было нечем.
        /// </summary>
        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
                return;

            if (Viewer != null && Viewer.ClearSelection())
                e.Handled = true;
        }
    }
}
