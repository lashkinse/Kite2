using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Kite2.App.ViewModels;

namespace Kite2.App.Views
{
    // Левая кнопка мыши выделяет участок, средняя двигает лист. Одно на другое в
    // одном жесте не уживается: протяжка либо двигает лист, либо выделяет.
    public partial class PageViewer : UserControl
    {
        private const double MinZoom = 0.05;

        private const double MaxZoom = 4.0;

        // На что делить, чтобы получить увеличение в полтора раза.
        private const double WheelFactor = 1.5;

        // Отступ листа от края окна — как в прежней версии программы.
        private const double PageMargin = 24;

        private const double HandleSize = 6;

        private const double SaveGap = 8;

        private const double HintGap = 6;

        // Минимальный размер выделения: меньше обрезать нечего.
        private const double MinimumSelection = 8;

        private double _zoom = 1.0;

        private bool _dragging;

        private Point _dragOrigin;

        private double _dragLeft;

        private double _dragTop;

        private bool _middleDrag;

        // Запрошено сохранение фрагмента. В событии идёт прямоугольник в пикселях
        // картинки страницы: пересчёт из координат листа делает тот, кто видит и
        // лист, и картинку, — область просмотра.
        public event EventHandler<Rect> FragmentRequested;

        // Лист поехал под мышью или прокруткой. Панель команд над страницей на это
        // время прячется: под ней оказался бы тот самый лист.
        public event EventHandler<bool> SheetDragChanged;

        // Таймер, которым ловится конец прокрутки. Само событие приходит на каждое
        // движение полосы, а «остановился» оно не сообщает.
        private readonly DispatcherTimer _scrollEndTimer;

        public PageViewer()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;

            // Обработчики висят на корне (лист меняет размер, а колесо должно работать
            // и над серым столом) и берутся предпросмотровые: обычные до корня
            // не доходят, их перехватывают полоса прокрутки и картинка.
            Root.PreviewMouseWheel += OnMouseWheel;
            Root.SizeChanged += OnRootSizeChanged;
            Root.PreviewMouseDown += OnMouseDown;
            Root.PreviewMouseMove += OnMouseMove;
            Root.PreviewMouseUp += OnMouseUp;

            Scroller.ScrollChanged += OnScrollerScrollChanged;

            _scrollEndTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            _scrollEndTimer.Tick += OnScrollEnded;
        }

        private void OnScrollEnded(object sender, EventArgs args)
        {
            _scrollEndTimer.Stop();

            RaiseSheetDragChanged(false);
        }

        private void OnScrollerScrollChanged(object sender, ScrollChangedEventArgs args)
        {
            RaiseSheetDragChanged(true);

            _scrollEndTimer.Stop();
            _scrollEndTimer.Start();

            FollowSheet();
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

        // Показывает выбранную страницу. Подгонка размера отложена до следующего
        // прохода разметки: в момент смены страницы элемент ещё не получил размера.
        private void ShowPage()
        {
            var model = DataContext as MainViewModel;
            var page = model?.SelectedPage;

            bool hasPage = page?.Page != null;

            Placeholder.Visibility = hasPage ? Visibility.Collapsed : Visibility.Visible;
            Sheet.Visibility = hasPage ? Visibility.Visible : Visibility.Collapsed;

            UpdateCursor();

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

            _zoom = scale;


            LayoutSheet(scale);
            CentreSheet();
        }

        // Задаёт размеры листа под масштаб вручную: растянутая картинка должна
        // давать полосе прокрутки ровно те размеры, что видны.
        private void LayoutSheet(double scale)
        {
            var model = DataContext as MainViewModel;
            var page = model?.SelectedPage;

            if (page?.Page == null)
                return;

            Sheet.Width = page.Page.Width * scale;
            Sheet.Height = page.Page.Height * scale;
        }

        // Ставит лист по центру, а не помещающийся лист — к левому верхнему краю
        // с полями: по центру он уезжал бы за края без возможности добраться.
        private void CentreSheet()
        {
            double viewportWidth = Scroller.ViewportWidth > 0 ? Scroller.ViewportWidth : ActualWidth;
            double viewportHeight = Scroller.ViewportHeight > 0 ? Scroller.ViewportHeight : ActualHeight;

            Canvas.SetLeft(Sheet, Math.Max(PageMargin, (viewportWidth - Sheet.Width) / 2));
            Canvas.SetTop(Sheet, Math.Max(PageMargin, (viewportHeight - Sheet.Height) / 2));

            UpdateCursor();
        }

        // Вид курсора говорит, что будет делать протягивание: левой кнопкой —
        // выделять участок, средней — двигать лист.
        private void UpdateCursor()
        {
            if (Sheet.Visibility != Visibility.Visible)
            {
                Root.Cursor = Cursors.Arrow;
                return;
            }

            Root.Cursor = Cursors.Cross;
        }

        // После изменения размера окна лист возвращается в исходный масштаб.
        private void OnRootSizeChanged(object sender, SizeChangedEventArgs args)
        {
            // Подгонка откладывается до следующего прохода разметки: полосы
            // прокрутки ещё старого размера.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                FitPage();
                FollowSheet();
            }), DispatcherPriority.Loaded);
        }

        // Колесо меняет масштаб, удерживая на месте точку листа под курсором, и
        // убирает выделение: после увеличения рамка указывала бы совсем не на тот
        // участок, который отмечали.
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta == 0 || Sheet.Visibility != Visibility.Visible)
                return;

            var model = DataContext as MainViewModel;
            var page = model?.SelectedPage;

            if (page?.Page == null)
                return;

            Point cursor = e.GetPosition(Sheet);

            double next = Math.Max(MinZoom, Math.Min(MaxZoom, _zoom * Math.Pow(WheelFactor, e.Delta / 120.0)));

            if (Math.Abs(next - _zoom) < 0.0001)
                return;

            ClearSelection();

            // Доля листа под курсором до изменения масштаба: она же должна остаться
            // под курсором после.
            double ratioX = cursor.X / Sheet.ActualWidth;
            double ratioY = cursor.Y / Sheet.ActualHeight;

            _zoom = next;
            LayoutSheet(next);

            double left = Canvas.GetLeft(Sheet) + cursor.X - ratioX * Sheet.Width;
            double top = Canvas.GetTop(Sheet) + cursor.Y - ratioY * Sheet.Height;

            Canvas.SetLeft(Sheet, left);
            Canvas.SetTop(Sheet, top);

            UpdateCursor();
            e.Handled = true;
        }

        // Нажатие начинает выделение (слева) или перетаскивание листа (средней).
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Sheet.Visibility != Visibility.Visible)
                return;

            // Щелчок по кнопке поверх листа доходит до самой кнопки.
            if (e.ChangedButton == MouseButton.Left && IsOnOverlay(e.OriginalSource))
                return;

            // Двойной щелчок возвращает лист в исходный размер. События двойного
            // щелчка у Grid нет, второй щелчок узнаётся по счётчику.
            if (e.ClickCount == 2)
            {
                FitPage();
                e.Handled = true;
                return;
            }

            _dragging = true;
            _middleDrag = e.ChangedButton == MouseButton.Middle;

            if (_middleDrag)
                RaiseSheetDragChanged(true);

            // Точка отсчёта берётся от области просмотра: отсчёт от едущего листа
            // менялся бы каждый раз — лист трясся бы вместо того, чтобы ехать.
            _dragOrigin = e.GetPosition(Root);
            _dragLeft = Canvas.GetLeft(Sheet);
            _dragTop = Canvas.GetTop(Sheet);

            Root.CaptureMouse();

            e.Handled = true;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragging || !IsDragButtonDown())
                return;

            ContinueDrag(e.GetPosition(Root));
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_dragging)
                return;

            _dragging = false;

            if (_middleDrag)
            {
                _middleDrag = false;
                RaiseSheetDragChanged(false);
            }

            if (Root.IsMouseCaptureWithin)
                Root.ReleaseMouseCapture();

            if (e.ChangedButton == MouseButton.Left)
                ShowSelection(PointInSheet(_dragOrigin), PointInSheet(e.GetPosition(Root)));

            UpdateCursor();
        }

        // Продолжает начатое мышью: средней кнопкой двигает лист, левой тянет
        // рамку выделения.
        private void ContinueDrag(Point current)
        {
            if (!_middleDrag)
            {
                ShowSelection(PointInSheet(_dragOrigin), PointInSheet(current));
                return;
            }

            Canvas.SetLeft(Sheet, _dragLeft + current.X - _dragOrigin.X);
            Canvas.SetTop(Sheet, _dragTop + current.Y - _dragOrigin.Y);

            FollowSheet();
        }

        // Точка мыши в координатах листа; пересчёт на каждое обращение, пока лист едет.
        private Point PointInSheet(Point point)
        {
            Point origin = Sheet.TranslatePoint(new Point(0, 0), Root);

            return new Point(point.X - origin.X, point.Y - origin.Y);
        }

        private bool IsDragButtonDown()
        {
            if (_middleDrag)
                return Mouse.MiddleButton == MouseButtonState.Pressed;

            return Mouse.LeftButton == MouseButtonState.Pressed;
        }
        // Одиночный щелчок выделения не оставляет: отмечать надо протяжкой.
        private void ShowSelection(Point from, Point to)
        {
            double width = Math.Abs(to.X - from.X);
            double height = Math.Abs(to.Y - from.Y);

            if (width < MinimumSelection || height < MinimumSelection)
            {
                ClearSelection();
                return;
            }

            double left = Math.Min(from.X, to.X);
            double top = Math.Min(from.Y, to.Y);

            // Выделение не выходит за лист.
            left = Math.Max(0, Math.Min(left, Sheet.ActualWidth));
            top = Math.Max(0, Math.Min(top, Sheet.ActualHeight));

            width = Math.Min(width, Sheet.ActualWidth - left);
            height = Math.Min(height, Sheet.ActualHeight - top);

            if (width < MinimumSelection || height < MinimumSelection)
            {
                ClearSelection();
                return;
            }

            Canvas.SetLeft(Selection, left);
            Canvas.SetTop(Selection, top);

            Selection.Width = width;
            Selection.Height = height;

            Selection.Visibility = Visibility.Visible;

            LayoutSelection();
        }

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

            // Кнопка сохранения стоит у правого нижнего угла рамки, но не на самом
            // углу: иначе она закрывает точку, за которую рамку тянут.
            PlaceSaveButton(right, bottom);

            PlaceHint(left, top);
        }

        // Ставит точку в углу рамки. Смещение на половину точки нужно, чтобы она
        // попадала на сам угол, а не на его внутренний край.
        private void Place(FrameworkElement handle, double left, double top)
        {
            Canvas.SetLeft(handle, left - HandleSize / 2);
            Canvas.SetTop(handle, top - HandleSize / 2);

            handle.Visibility = Visibility.Visible;
        }

        // Ставит кнопку сохранения у нижнего правого угла рамки. Координаты рамки
        // заданы в листе, а полотно лежит поверх окна просмотра, поэтому точка
        // переводится из одного пространства в другое. Никаких зажимов: у края
        // окна кнопка уезжает за него вместе со своим углом, но не прилипает к краю.
        private void PlaceSaveButton(double right, double bottom)
        {
            Point corner = Sheet.TranslatePoint(new Point(right, bottom), Overlay);

            Canvas.SetLeft(FragmentSave, corner.X + SaveGap);
            Canvas.SetTop(FragmentSave, corner.Y + SaveGap);

            FragmentSave.Visibility = Visibility.Visible;
        }

        // Ставит подсказку «Esc — отменить» над левым верхним углом рамки. Никаких
        // зажимов: у верхнего и левого края окна подсказка уезжает за них вместе со
        // своим углом, но не прилипает к краю.
        private void PlaceHint(double left, double top)
        {
            Point corner = Sheet.TranslatePoint(new Point(left, top), Overlay);

            // Пока элемент свёрнут, его высота нулевая, поэтому подсказку сначала
            // показываем, а потом уже меряем её высоту.
            SelectionHint.Visibility = Visibility.Visible;

            Canvas.SetLeft(SelectionHint, corner.X - HintGap);
            Canvas.SetTop(SelectionHint, corner.Y - SelectionHint.ActualHeight - HintGap);
        }

        // Ставит кнопку сохранения и подсказку к рамке выделения. Они лежат в слое
        // поверх листа, а рамка — внутри листа, поэтому при переносе листа их
        // положение пересчитывается.
        private void FollowSheet()
        {
            if (Selection.Visibility != Visibility.Visible)
                return;

            LayoutSelection();
        }

        private void RaiseSheetDragChanged(bool dragging)
        {
            SheetDragChanged?.Invoke(this, dragging);
        }

        private void OnCropClick(object sender, RoutedEventArgs e)
        {
            Rect area = SelectionInImagePixels();

            if (area.Width >= 1 && area.Height >= 1)
                FragmentRequested?.Invoke(this, area);

            e.Handled = true;
        }

        // Прямоугольник выделения в пикселях картинки: координаты рамки делятся
        // на масштаб, в котором показан лист.
        private Rect SelectionInImagePixels()
        {
            var model = DataContext as MainViewModel;
            var page = model?.SelectedPage;

            if (page?.Page == null || Sheet.ActualWidth < 1 || Sheet.ActualHeight < 1)
                return Rect.Empty;

            double scale = Sheet.ActualWidth / page.Page.Width;

            if (scale <= 0)
                return Rect.Empty;

            return new Rect(
                Canvas.GetLeft(Selection) / scale,
                Canvas.GetTop(Selection) / scale,
                Selection.Width / scale,
                Selection.Height / scale);
        }


        // Снимает выделение и убирает ножницы. Возвращает, было ли выделение:
        // окну это нужно, чтобы отдать клавишу дальше, если снимать было нечего.
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

            FragmentSave.Visibility = Visibility.Collapsed;
            SelectionHint.Visibility = Visibility.Collapsed;

            return had;
        }

        // Нажатие пришло по кнопке поверх листа, а не по самому листу. Кнопка
        // лежит в полотне, которое перекрывает просмотр, и её нажатия не должны
        // начинать ни выделение, ни перетаскивание.
        private bool IsOnOverlay(object source)
        {
            DependencyObject current = source as DependencyObject;

            while (current != null)
            {
                if (ReferenceEquals(current, Overlay))
                    return true;

                current = VisualTreeHelper.GetParent(current);
            }

            return false;
        }
    }
}
