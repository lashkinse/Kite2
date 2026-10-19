using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Kite2.App.ViewModels;

namespace Kite2.App.Views
{
    // Связь между моделью и представлениями ставится здесь: ни одна из сторон
    // о другой не знает. Escape перехватывается у окна.
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

            // Полоса команд страниц показывается, пока есть что командовать
            // и пока лист не едет.
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
        // Ctrl+Up/Down обходят ListBox: список съедает стрелки и обрабатывает их
        // сам даже с Ctrl — глобальные KeyBinding при фокусе в колонке опаздывают.
        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (Viewer != null && Viewer.ClearSelection())
                    e.Handled = true;
                return;
            }

            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            MainViewModel model = DataContext as MainViewModel;
            if (model == null)
                return;

            if (e.Key == Key.Up)
                Execute(model.MovePageUpCommand);
            else if (e.Key == Key.Down)
                Execute(model.MovePageDownCommand);
            else
                return;

            e.Handled = true;
        }

        private static void Execute(RelayCommand command)
        {
            if (command.CanExecute(null))
                command.Execute(null);
        }
    }
}