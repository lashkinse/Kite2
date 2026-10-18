using System.Windows;
using System.Windows.Input;
using Kite2.App.ViewModels;

namespace Kite2.App.Views
{
    // Модель и представления не знают друг о друге: связь между ними ставится здесь,
    // иначе между ними возник бы круг. Escape перехватывается у окна, а не у
    // области просмотра: при выделении он снимает его, иначе — просто уходит.
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            PreviewKeyDown += OnPreviewKeyDown;
        }

        public void Connect(MainViewModel model)
        {
            Viewer.FragmentRequested += (sender, area) => model.SaveSelectedFragment(area);

            // Полоса команд страниц показывается, пока есть что командовать и пока
            // лист не едет: ни одной из сторон о другой знать не нужно.
            Viewer.SheetDragChanged += (sender, dragging) => UpdateActions(model, dragging);
            model.Pages.CollectionChanged += (sender, args) => UpdateActions(model);

            UpdateActions(model);
        }

        private void UpdateActions(MainViewModel model, bool sheetDragging = false)
        {
            Actions.Visibility = sheetDragging || model.Pages.Count == 0
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        // Escape снимает выделение, но не окно: иначе закрыть программу, не разобрав
        // результат случайного протягивания мышью, было бы невозможно.
        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape)
                return;

            if (Viewer != null && Viewer.ClearSelection())
                e.Handled = true;
        }
    }
}