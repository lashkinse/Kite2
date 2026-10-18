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
    /// <summary>
    /// Область просмотра выбранной страницы: лист по центру, масштаб колесом мыши,
    /// перетаскивание листа и выделение участка страницы.
    /// </summary>
    /// <remarks>
    /// Левая кнопка мыши выделяет участок, средняя двигает лист. Одно на другое в
    /// одном жесте не уживается: протяжка либо двигает лист, либо выделяет.
    /// </remarks>
    public partial class PageViewer : UserControl
    {
        /// <summary>Наименьший масштаб: ниже лист превращается в пятно.</summary>
        private const double MinZoom = 0.05;

        /// <summary>Наибольший масштаб: зачем показывать пиксель во весь экран.</summary>
        private const double MaxZoom = 4.0;

        /// <summary>На что делить, чтобы получить увеличение в полтора раза.</summary>
        private const double WheelFactor = 1.5;

        /// <summary>
        /// Отступ листа от края окна. Тот же, что был в прежней версии программы: он
        /// держал лист на одном расстоянии от краёв и при разном масштабе.
        /// </summary>
        private const double PageMargin = 24;

        /// <summary>Размер точки в углах выделения.</summary>
        private const double HandleSize = 6;

        /// <summary>На сколько пикселей кнопка сохранения отходит от угла выделения.</summary>
        private const double ScissorsGap = 8;

        /// <summary>На сколько пикселей подсказка отходит от угла выделения.</summary>
        private const double HintGap = 6;

        /// <summary>Минимальный размер выделения: меньше обрезать нечего.</summary>
        private const double MinimumSelection = 8;

        private double _zoom = 1.0;

        /// <summary>Идёт ли протягивание мышью: листа или выделения.</summary>
        private bool _dragging;

        private Point _dragOrigin;

        private double _dragLeft;

        private double _dragTop;

        /// <summary>Перетаскивание начато средней кнопкой: им двигают лист.</summary>
        private bool _middleDrag;

        /// <summary>
        /// Запрошено сохранение фрагмента. В событии идёт прямоугольник в пикселях
        /// картинки страницы: пересчёт из координат листа делает тот, кто видит и
        /// лист, и картинку, — область просмотра.
        /// </summary>
        public event EventHandler<Rect> FragmentRequested;

        /// <summary>
        /// Лист поехал под мышью или прокруткой. Панель команд над страницей на это
        /// время прячется: под ней оказался бы тот самый лист.
        /// </summary>
        public event EventHandler<bool> SheetDragChanged;

        /// <summary>
        /// Таймер, которым ловится конец прокрутки. Само событие приходит на каждое
        /// движение полосы, а «остановился» оно не сообщает.
        /// </summary>
        private readonly DispatcherTimer _scrollEndTimer;

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

            _zoom = scale;


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

            UpdateCursor();
        }

        /// <summary>
        /// Вид курсора говорит, что будет делать протягивание: левой кнопкой —
        /// выделять участок, средней — двигать лист.
        /// </summary>
        private void UpdateCursor()
        {
            if (Sheet.Visibility != Visibility.Visible)
            {
                Root.Cursor = Cursors.Arrow;
                return;
            }

            Root.Cursor = Cursors.Cross;
        }

        /// <summary>
        /// После изменения размера окна лист возвращается в исходный масштаб и
        /// встаёт по центру: увеличенный лист иначе остался бы увеличенным или уехал
        /// бы в угол.
        /// </summary>
        private void OnRootSizeChanged(object sender, SizeChangedEventArgs args)
        {
            // Подгонка откладывается до следующего прохода разметки: во время
            // SizeChanged полосы прокрутки ещё старого размера, и лист встал бы
            // по центру прежнего окна.
            Dispatcher.BeginInvoke(new Action(FitPage), DispatcherPriority.Loaded);
        }

        /// <summary>
        /// Колесо меняет масштаб, удерживая на месте точку листа под курсором, и
        /// убирает выделение: после увеличения рамка указывала бы совсем не на тот
        /// участок, который отмечали.
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

        /// <summary>
        /// Нажатие мыши начинает либо выделение участка, либо перетаскивание листа.
        /// Выделение отмечается всегда, а лист двигают средней кнопкой: ею его уводят
        /// из-под себя, не сбивая уже начатую рамку.
        /// </summary>
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Sheet.Visibility != Visibility.Visible)
                return;

            // Щелчок по кнопке сохранения должен дойти до кнопки: этот обработчик
            // хватает событие раньше неё и помечает обработанным, и без проверки
            // кнопка просто не срабатывала бы.
            if (e.ChangedButton == MouseButton.Left && IsOnOverlay(e.OriginalSource))
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

            _dragging = true;
            _middleDrag = e.ChangedButton == MouseButton.Middle;

            if (_middleDrag)
                RaiseSheetDragChanged(true);

            // Точка отсчёта берётся от области просмотра, а не от листа: лист под
            // курсором сам идёт направо, и отсчёт от него каждый раз меняется — лист
            // трясётся вместо того, чтобы ехать.
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

        /// <summary>
        /// Продолжает начатое мышью: средней кнопкой двигает лист, левой тянет
        /// рамку выделения.
        /// </summary>
        private void ContinueDrag(Point current)
        {
            if (!_middleDrag)
            {
                ShowSelection(PointInSheet(_dragOrigin), PointInSheet(current));
                return;
            }

            Canvas.SetLeft(Sheet, _dragLeft + current.X - _dragOrigin.X);
            Canvas.SetTop(Sheet, _dragTop + current.Y - _dragOrigin.Y);

            // Кнопка сохранения и подсказка лежат не в листе, а поверх него:
            // без этого шага они остались бы на месте, пока лист уезжал, и оторвались
            // бы от своей рамки.
            FollowSheet();
        }

        /// <summary>
        /// Точка мыши в координатах листа. Пока лист едет, его начало тоже едет, и
        /// пересчёт идёт каждый раз — иначе рамка тянулась бы с опозданием.
        /// </summary>
        private Point PointInSheet(Point point)
        {
            Point origin = Sheet.TranslatePoint(new Point(0, 0), Root);

            return new Point(point.X - origin.X, point.Y - origin.Y);
        }

        /// <summary>Зажата та кнопка, которой начали перетаскивание.</summary>
        private bool IsDragButtonDown()
        {
            if (_middleDrag)
                return Mouse.MiddleButton == MouseButtonState.Pressed;

            return Mouse.LeftButton == MouseButtonState.Pressed;
        }
        /// <summary>
        /// Показывает выделение между двумя точками листа. Одиночный щелчок выделения
        /// не оставляет: отмечать участок надо протяжкой, иначе щелчок мимо создаёт
        /// рамку в одну точку.
        /// </summary>
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

            // Выделение не выходит за лист: рамка за краем уходила бы на серый стол и
            // выглядела бы отмеченной областью, которой на странице нет.
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

        /// <summary>Ставит точки в углы рамки выделения и ножницы рядом с ними.</summary>
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

            // Ножницы стоят у правого нижнего угла рамки, но не на самом углу:
            // иначе они закрывают точку, за которую рамку тянут.
            PlaceScissors(right, bottom);

            PlaceHint(left, top);
        }

        /// <summary>
        /// Ставит точку в углу рамки. Смещение на половину точки нужно, чтобы она
        /// попадала на сам угол, а не на его внутренний край.
        /// </summary>
        private void Place(FrameworkElement handle, double left, double top)
        {
            Canvas.SetLeft(handle, left - HandleSize / 2);
            Canvas.SetTop(handle, top - HandleSize / 2);

            handle.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Ставит кнопку обрезки у рамки выделения. Координаты рамки заданы в листе,
        /// а полотно лежит поверх окна просмотра, поэтому точка переводится из одного
        /// пространства в другое.
        /// </summary>
        private void PlaceScissors(double right, double bottom)
        {
            Point corner = Sheet.TranslatePoint(new Point(right, bottom), Overlay);

            double left = corner.X + ScissorsGap;
            double top = corner.Y + ScissorsGap;

            // У края окна кнопка переворачивается на другую сторону рамки, иначе
            // она уезжала бы за пределы видимой части.
            if (left + FragmentSave.Width > ActualWidth - 4)
                left = corner.X - FragmentSave.Width - ScissorsGap;

            if (top + FragmentSave.Height > ActualHeight - 4)
                top = corner.Y - FragmentSave.Height - ScissorsGap;

            Canvas.SetLeft(FragmentSave, Math.Max(0, left));
            Canvas.SetTop(FragmentSave, Math.Max(0, top));

            FragmentSave.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Ставит подсказку «Esc — отменить» у левого верхнего угла рамки.
        /// </summary>
        private void PlaceHint(double left, double top)
        {
            Point corner = Sheet.TranslatePoint(new Point(left, top), Overlay);

            // Пока элемент свёрнут, его высота нулевая, поэтому подсказку сначала
            // показываем, а потом уже смотрим, влезает ли она над рамкой.
            SelectionHint.Visibility = Visibility.Visible;

            double y = corner.Y - SelectionHint.ActualHeight - HintGap;

            // У самого верха листа над рамкой места нет: подсказка переходит под
            // верхний край, иначе её срезало бы.
            if (y < 2)
                y = corner.Y + HintGap;

            Canvas.SetLeft(SelectionHint, Math.Max(0, corner.X - HintGap));
            Canvas.SetTop(SelectionHint, y);
        }

        /// <summary>
        /// Держит кнопку сохранения и подсказку у рамки выделения. Они лежат в слое
        /// поверх листа, а рамка — внутри листа, и без этого шага при переносе или
        /// прокрутке они остались бы на месте и оторвались бы от своего угла.
        /// </summary>
        private void FollowSheet()
        {
            if (Selection.Visibility != Visibility.Visible)
                return;

            LayoutSelection();
        }

        /// <summary>Сообщает, что лист поехал: true — едет, false — встал.</summary>
        private void RaiseSheetDragChanged(bool dragging)
        {
            SheetDragChanged?.Invoke(this, dragging);
        }
        /// <summary>Дискета нажата: фрагмент под рамкой выделения запрошен.</summary>
        private void OnCropClick(object sender, RoutedEventArgs e)
        {
            Rect area = SelectionInImagePixels();

            if (area.Width >= 1 && area.Height >= 1)
                FragmentRequested?.Invoke(this, area);

            e.Handled = true;
        }

        /// <summary>
        /// Прямоугольник выделения в пикселях картинки страницы. Лист на экране
        /// показан в масштабе, поэтому координаты рамки делятся на масштаб.
        /// </summary>
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


        /// <summary>
        /// Снимает выделение и убирает ножницы. Возвращает, было ли выделение:
        /// окну это нужно, чтобы отдать клавишу дальше, если снимать было нечего.
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

            FragmentSave.Visibility = Visibility.Collapsed;
            SelectionHint.Visibility = Visibility.Collapsed;

            return had;
        }

        /// <summary>
        /// Нажатие пришло по кнопке поверх листа, а не по самому листу. Кнопка
        /// лежит в полотне, которое перекрывает просмотр, и её нажатия не должны
        /// начинать ни выделение, ни перетаскивание.
        /// </summary>
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