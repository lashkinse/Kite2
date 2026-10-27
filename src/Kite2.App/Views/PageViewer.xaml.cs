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
    // Левая кнопка выделяет фрагмент, средняя двигает лист, колесо — масштаб.
    public partial class PageViewer : UserControl
    {
        private const double MinZoom = 0.05;
        private const double MaxZoom = 4.0;
        private const double WheelFactor = 1.5;
        private const double PageMargin = 24;
        private const double HandleSize = 6;
        private const double HintGap = 6;
        private const double MinimumSelection = 8;

        private double _zoom = 1.0;
        private bool _dragging;
        private Point _dragOrigin;
        private double _dragLeft;
        private double _dragTop;
        private bool _middleDrag;

        private readonly DispatcherTimer _scrollEndTimer;

        public event EventHandler<Rect> FragmentRequested;

        // Пока лист тянут (илинули прокрутку), панель команд поверх страницы прячется.
        public event EventHandler<bool> SheetDragChanged;

        public PageViewer()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;

            // Предпросмотровые обработчики на корне: события ниже перехватывают
            // полоса прокрутки и картинка.
            Root.PreviewMouseWheel += OnMouseWheel;
            Root.SizeChanged += OnRootSizeChanged;
            Root.PreviewMouseDown += OnMouseDown;
            Root.PreviewMouseMove += OnMouseMove;
            Root.PreviewMouseUp += OnMouseUp;

            Scroller.ScrollChanged += OnScrollerScrollChanged;

            _scrollEndTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            _scrollEndTimer.Tick += (s, a) =>
            {
                _scrollEndTimer.Stop();
                RaiseSheetDragChanged(false);
            };
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
            ClearSelection();

            // Размер элемента ещё не посчитан — подгонка следующим проходом разметки.
            Dispatcher.BeginInvoke(new Action(FitPage), DispatcherPriority.Loaded);
        }

        private void FitPage()
        {
            var page = SelectedPageOf(DataContext);

            if (page?.Page == null || ActualWidth < 100 || ActualHeight < 100)
                return;

            // Масштаб: лист вписывается в окно с полями.
            double scale = Math.Min(
                Math.Max(1, ActualWidth - PageMargin * 2) / page.Page.Width,
                Math.Max(1, ActualHeight - PageMargin * 2) / page.Page.Height);

            _zoom = scale;

            LayoutSheet(scale);
            CentreSheet();
        }

        private void LayoutSheet(double scale)
        {
            var page = SelectedPageOf(DataContext);

            if (page?.Page == null)
                return;

            Sheet.Width = page.Page.Width * scale;
            Sheet.Height = page.Page.Height * scale;
        }

        // Лист помещается по центру; не помещающийся — у левого верхнего края с полями.
        private void CentreSheet()
        {
            double viewportWidth = Scroller.ViewportWidth > 0 ? Scroller.ViewportWidth : ActualWidth;
            double viewportHeight = Scroller.ViewportHeight > 0 ? Scroller.ViewportHeight : ActualHeight;

            Canvas.SetLeft(Sheet, Math.Max(PageMargin, (viewportWidth - Sheet.Width) / 2));
            Canvas.SetTop(Sheet, Math.Max(PageMargin, (viewportHeight - Sheet.Height) / 2));

            UpdateCursor();
        }

        private void UpdateCursor()
        {
            Root.Cursor = Sheet.Visibility == Visibility.Visible ? Cursors.Cross : Cursors.Arrow;
        }

        private void OnRootSizeChanged(object sender, SizeChangedEventArgs args)
        {
            // Полосы прокрутки ещё старого размера — подгонка следующим проходом.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                FitPage();
                FollowSheet();
            }), DispatcherPriority.Loaded);
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta == 0 || Sheet.Visibility != Visibility.Visible)
                return;

            var page = SelectedPageOf(DataContext);

            if (page?.Page == null)
                return;

            Point cursor = e.GetPosition(Sheet);

            double next = Math.Max(MinZoom, Math.Min(MaxZoom, _zoom * Math.Pow(WheelFactor, e.Delta / 120.0)));

            if (Math.Abs(next - _zoom) < 0.0001)
                return;

            ClearSelection();

            // Точка листа под курсором остаётся под курсором после смены масштаба.
            double ratioX = cursor.X / Sheet.ActualWidth;
            double ratioY = cursor.Y / Sheet.ActualHeight;

            _zoom = next;
            LayoutSheet(next);

            Canvas.SetLeft(Sheet, Canvas.GetLeft(Sheet) + cursor.X - ratioX * Sheet.Width);
            Canvas.SetTop(Sheet, Canvas.GetTop(Sheet) + cursor.Y - ratioY * Sheet.Height);

            UpdateCursor();
            e.Handled = true;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Sheet.Visibility != Visibility.Visible)
                return;

            if (e.ChangedButton == MouseButton.Left && IsOnOverlay(e.OriginalSource))
                return;

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

        private Point PointInSheet(Point point)
        {
            Point origin = Sheet.TranslatePoint(new Point(0, 0), Root);

            return new Point(point.X - origin.X, point.Y - origin.Y);
        }

        private bool IsDragButtonDown() =>
            _middleDrag
                ? Mouse.MiddleButton == MouseButtonState.Pressed
                : Mouse.LeftButton == MouseButtonState.Pressed;

        private void ShowSelection(Point from, Point to)
        {
            double width = Math.Abs(to.X - from.X);
            double height = Math.Abs(to.Y - from.Y);

            if (width < MinimumSelection || height < MinimumSelection)
            {
                ClearSelection();
                return;
            }

            double left = Math.Max(0, Math.Min(Math.Min(from.X, to.X), Sheet.ActualWidth));
            double top = Math.Max(0, Math.Min(Math.Min(from.Y, to.Y), Sheet.ActualHeight));

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

            PlaceHint(left, top);
        }

        private void Place(FrameworkElement handle, double left, double top)
        {
            Canvas.SetLeft(handle, left - HandleSize / 2);
            Canvas.SetTop(handle, top - HandleSize / 2);

            handle.Visibility = Visibility.Visible;
        }

        private void PlaceHint(double left, double top)
        {
            Point corner = Sheet.TranslatePoint(new Point(left, top), Overlay);

            // Высота свёрнутого элемента нулевая — сначала показать, потом мерить.
            SelectionHint.Visibility = Visibility.Visible;

            Canvas.SetLeft(SelectionHint, corner.X - HintGap);
            Canvas.SetTop(SelectionHint, Math.Max(PageMargin, corner.Y - HintGap - SelectionHint.ActualHeight));
        }

        private void FollowSheet()
        {
            if (Selection.Visibility == Visibility.Visible)
                LayoutSelection();
        }

        private void RaiseSheetDragChanged(bool dragging) =>
            SheetDragChanged?.Invoke(this, dragging);

        private void OnCropClick(object sender, RoutedEventArgs e)
        {
            Rect area = SelectionInImagePixels();

            if (area.Width >= 1 && area.Height >= 1)
                FragmentRequested?.Invoke(this, area);

            e.Handled = true;
        }

        private Rect SelectionInImagePixels()
        {
            var page = SelectedPageOf(DataContext);

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

        // true — было скрыто выделение (окно решает, продолжать ли обработку Esc).
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

            SelectionHint.Visibility = Visibility.Collapsed;

            return had;
        }

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

        private static PageViewModel SelectedPageOf(object viewModel) =>
            (viewModel as MainViewModel)?.SelectedPage;
    }
}
