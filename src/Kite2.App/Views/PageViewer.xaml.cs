using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Kite2.App.ViewModels;
using NLog;


namespace Kite2.App.Views
{
    /// <summary>
    /// Область просмотра выбранной страницы: лист по центру, масштаб колесом мыши
    /// и выделение участка страницы щелчком.
    /// </summary>
    public partial class PageViewer : UserControl
    {
        /// <summary>Наименьший масштаб: ниже лист превращается в пятно.</summary>
        private const double MinZoom = 0.05;

        /// <summary>Наибольший масштаб: зачем показывать пиксель во весь экран.</summary>
        private const double MaxZoom = 4.0;

        /// <summary>Шаг масштаба за щелчок колеса.</summary>
        private const double ZoomStep = 0.15;

        /// <summary>
        /// Отступ листа от края окна. Тот же, что был в прежней версии программы: он
        /// держал лист на одном расстоянии от краёв и при разном масштабе.
        /// </summary>
        private const double PageMargin = 24;

        /// <summary>
        /// Сторона выделения, создаваемого щелчком без перетаскивания. Такой размер
        /// взят из прежней версии программы: он одинаково подходит для отметки
        /// печати, подписи и номера страницы.
        /// </summary>
        private const double DefaultRegionSize = 160;

        /// <summary>Размер точки в углах выделения.</summary>
        private const double HandleSize = 6;

        /// <summary>На что делить, чтобы получить увеличение в полтора раза.</summary>
        private const double WheelFactor = 1.5;

        /// <summary>Текущий масштаб листа. Задаёт и размер, и положение.</summary>
        private double CurrentZoom { get; set; } = 1.0;

        /// <summary>Идёт ли протягивание выделения мышью.</summary>
        private bool _selecting;

        private Point _selectStart;

        public PageViewer()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;

            // Обработчики висят на корне, а не на листе: лист меняет размер и
            // становится не тем элементом, на который повесили бы их изначально,
            // а колесо должно работать и над серым столом.
            //
            // События берутся предпросмотровые: обычные до корня не доходят —
            // полоса прокрутки и сама картинка обрабатывают щелчок и колесо раньше,
            // и выделение не появлялось, а колесо листало область вместо увеличения.
            Root.PreviewMouseWheel += OnMouseWheel;
            Root.SizeChanged += OnRootSizeChanged;
            Root.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            Root.PreviewMouseMove += OnMouseMove;
            Root.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.OldValue is MainViewModel previous)
                previous.PropertyChanged -= OnViewModelPropertyChanged;

            if (args.NewValue is MainViewModel current)
                current.PropertyChanged += OnViewModelPropertyChanged;

            ShowPage();
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.SelectedPage))
                ShowPage();
        }

        /// <summary>
        /// Показывает выбранную страницу. Подгонка размера отложена до следующего
        /// прохода разметки: в момент смены страницы элемент ещё не получил размера,
        /// и лист уезжал в угол крошечным пятном.
        /// </summary>
        private void ShowPage()
        {
            var model = DataContext as MainViewModel;
            var page = model?.SelectedPage;

            bool hasPage = page?.Page != null;

            Placeholder.Visibility = hasPage ? Visibility.Collapsed : Visibility.Visible;
            Sheet.Visibility = hasPage ? Visibility.Visible : Visibility.Collapsed;

            if (!hasPage)
            {
                ClearSelection();
                return;
            }

            Picture.Source = page.Page;

            // Выделение относится к прежней странице: на новой оно указывало бы
            // на пустое место.
            ClearSelection();

            Dispatcher.BeginInvoke(new Action(FitPage), DispatcherPriority.Loaded);
        }

        /// <summary>Вписывает лист в окно целиком и ставит его по центру.</summary>
        private void FitPage()
        {
            var model = DataContext as MainViewModel;
            var page = model?.SelectedPage;

            // Пока у элемента нет настоящего размера, подгонять нечего: масштаб вышел бы
            // из расчёта «ширина окна равна нулю» и лист схлопнулся бы в точку.
            if (page?.Page == null || ActualWidth < 100 || ActualHeight < 100)
                return;

            double availableWidth = Math.Max(1, ActualWidth - PageMargin * 2);
            double availableHeight = Math.Max(1, ActualHeight - PageMargin * 2);

            double scale = Math.Min(
                availableWidth / page.Page.Width,
                availableHeight / page.Page.Height);

            CurrentZoom = scale;

            LayoutSheet(scale);
            CentreSheet();
        }

        /// <summary>
        /// Задаёт размеры листа под масштаб. Размер задаётся вручную, потому что
        /// картинка растягивается: у листа должны быть ровно такие размеры, какие
        /// показывает полоса прокрутки, иначе она считает область по старой картинке.
        /// </summary>
        private void LayoutSheet(double scale)
        {
            var model = DataContext as MainViewModel;
            var page = model?.SelectedPage;

            if (page?.Page == null)
                return;

            Sheet.Width = page.Page.Width * scale;
            Sheet.Height = page.Page.Height * scale;
        }

        /// <summary>
        /// Ставит лист по центру области просмотра. Лист, не помещающийся в окно,
        /// выравнивается по левому и верхнему краю с полями: по центру он уезжал бы
        /// за края, и добраться до содержимого было бы нечем.
        /// </summary>
        private void CentreSheet()
        {
            double viewportWidth = Scroller.ViewportWidth > 0 ? Scroller.ViewportWidth : ActualWidth;
            double viewportHeight = Scroller.ViewportHeight > 0 ? Scroller.ViewportHeight : ActualHeight;

            Canvas.SetLeft(Sheet, Math.Max(PageMargin, (viewportWidth - Sheet.Width) / 2));
            Canvas.SetTop(Sheet, Math.Max(PageMargin, (viewportHeight - Sheet.Height) / 2));
        }

        private void OnRootSizeChanged(object sender, SizeChangedEventArgs args)
        {
            if (CurrentZoom <= MinZoom + 0.001)
                return;

            FitPage();
        }

        /// <summary>
        /// Колесо меняет масштаб, удерживая на месте точку листа под курсором. Привязка
        /// именно к курсору: привязка к середине листа уводила увеличенный лист из-под
        /// мыши, и приходилось искать нужное место заново.
        /// </summary>
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta == 0 || Sheet.Visibility != Visibility.Visible)
                return;

            var model = DataContext as MainViewModel;
            var page = model?.SelectedPage;

            if (page?.Page == null)
                return;

            Point cursor = e.GetPosition(Sheet);

            double next = Math.Max(MinZoom, Math.Min(MaxZoom, CurrentZoom * Math.Pow(WheelFactor, e.Delta / 120.0)));

            if (Math.Abs(next - CurrentZoom) < 0.0001)
                return;

            CurrentZoom = next;

            LayoutSheet(next);

            // Доля листа под курсором до изменения масштаба: она же должна остаться
            // под курсором после.
            double ratioX = cursor.X / Sheet.ActualWidth;
            double ratioY = cursor.Y / Sheet.ActualHeight;

            Sheet.Width = page.Page.Width * next;
            Sheet.Height = page.Page.Height * next;

            double left = Canvas.GetLeft(Sheet) + cursor.X - ratioX * Sheet.Width;
            double top = Canvas.GetTop(Sheet) + cursor.Y - ratioY * Sheet.Height;

            Canvas.SetLeft(Sheet, left);
            Canvas.SetTop(Sheet, top);

            // Точки угла выделения заданы рамкой, а не листом, поэтому после
            // изменения размера рамку надо пересчитать.
            LayoutSelection();

            e.Handled = true;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Sheet.Visibility != Visibility.Visible)
                return;

            // Двойной щелчок возвращает лист в исходный размер и по центру. Событие
            // двойного щелчка у Grid отсутствует, поэтому второй щелчок узнаётся
            // по счётчику в этом же событии.
            if (e.ClickCount == 2)
            {
                FitPage();
                e.Handled = true;
                return;
            }

            _selecting = true;
            _selectStart = e.GetPosition(Sheet);

            Root.CaptureMouse();

            e.Handled = true;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {

            if (!_selecting)
                return;

            ShowSelection(_selectStart, e.GetPosition(Sheet));
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_selecting)
            {
                _selecting = false;
                Root.ReleaseMouseCapture();

                // Щелчок без перетаскивания отмечает участок заданного размера под
                // курсором: иначе выделение получалось бы нулевого размера и его
                // не было бы видно.
                if (Selection.Visibility != Visibility.Visible)
                {
                    double half = Math.Min(DefaultRegionSize, Math.Min(Sheet.ActualWidth, Sheet.ActualHeight)) / 2;

                    ShowSelection(
                        new Point(_selectStart.X - half, _selectStart.Y - half),
                        new Point(_selectStart.X + half, _selectStart.Y + half));
                }

                LayoutSelection();
            }
            else
            {
            }

        }



        /// <summary>Показывает выделение между двумя точками листа.</summary>
        private void ShowSelection(Point from, Point to)
        {
            double left = Math.Min(from.X, to.X);
            double top = Math.Min(from.Y, to.Y);

            double width = Math.Abs(to.X - from.X);
            double height = Math.Abs(to.Y - from.Y);

            // Выделение не выходит за лист: рамка за краем уходила бы на серый стол и
            // выглядела бы отмеченной областью, которой на странице нет.
            left = Math.Max(0, Math.Min(left, Sheet.ActualWidth));
            top = Math.Max(0, Math.Min(top, Sheet.ActualHeight));

            width = Math.Min(width, Sheet.ActualWidth - left);
            height = Math.Min(height, Sheet.ActualHeight - top);

            if (width < 1 || height < 1)
            {
                Selection.Visibility = Visibility.Collapsed;
                return;
            }

            Canvas.SetLeft(Selection, left);
            Canvas.SetTop(Selection, top);

            Selection.Width = width;
            Selection.Height = height;

            Selection.Visibility = Visibility.Visible;

            LayoutSelection();
        }

        /// <summary>Ставит точки в углы рамки выделения.</summary>
        private void LayoutSelection()
        {
            if (Selection.Visibility != Visibility.Visible)
                return;

            double left = Canvas.GetLeft(Selection);
            double top = Canvas.GetTop(Selection);
            double right = left + Selection.Width;
            double bottom = top + Selection.Height;

            Place(HandleTopLeft, left, top);
            Place(HandleTopRight, right, top);
            Place(HandleBottomLeft, left, bottom);
            Place(HandleBottomRight, right, bottom);
        }

        /// <summary>
        /// Ставит точку в углу рамки. Смещение на половину точки нужен, чтобы она
        /// попадала на сам угол, а не на его внутренний край.
        /// </summary>
        private void Place(FrameworkElement handle, double left, double top)
        {
            Canvas.SetLeft(handle, left - HandleSize / 2);
            Canvas.SetTop(handle, top - HandleSize / 2);

            handle.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Снимает выделение. Возвращает, было ли оно: окну это нужно, чтобы
        /// отдать клавишу дальше, если снимать было нечего.
        /// </summary>
        public bool ClearSelection()
        {
            bool had = Selection.Visibility == Visibility.Visible;

            Selection.Visibility = Visibility.Collapsed;
            Selection.Width = 0;
            Selection.Height = 0;

            HandleTopLeft.Visibility = Visibility.Collapsed;
            HandleTopRight.Visibility = Visibility.Collapsed;
            HandleBottomLeft.Visibility = Visibility.Collapsed;
            HandleBottomRight.Visibility = Visibility.Collapsed;

            return had;
        }
    }
}
