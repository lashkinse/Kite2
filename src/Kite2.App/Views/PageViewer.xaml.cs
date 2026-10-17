using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Media;
using Kite2.App.ViewModels;

namespace Kite2.App.Views
{
    /// <summary>
    /// Область просмотра выбранной страницы с масштабом по колесу мыши.
    /// </summary>
    public partial class PageViewer : UserControl
    {
        /// <summary>Наименьший масштаб: ниже лист превращается в пятно.</summary>
        private const double MinZoom = 0.05;

        /// <summary>Наибольший масштаб: зачем показывать пиксель во весь экран.</summary>
        private const double MaxZoom = 3.0;

        /// <summary>Шаг масштаба за щелчок колеса.</summary>
        private const double ZoomStep = 0.15;

        /// <summary>
        /// Отступ листа от края окна. Тот же, что был в прежней версии программы: он
        /// держал лист на одном расстоянии от краёв и при разном масштабе.
        /// </summary>
        private const double PageMargin = 24;

        /// <summary>Текущий масштаб листа. Задаёт и размер, и положение.</summary>
        private double CurrentZoom { get; set; } = 1.0;

        public PageViewer()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;

            Root.MouseWheel += OnMouseWheel;
            Root.SizeChanged += (sender, args) => FitPage();
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
                return;

            Picture.Source = page.Page;

            Dispatcher.BeginInvoke(new Action(FitPage), DispatcherPriority.Loaded);
        }

        /// <summary>Вписывает лист в окно целиком, с полями по краям.</summary>
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

            Canvas.SetLeft(Sheet, PageMargin);
            Canvas.SetTop(Sheet, PageMargin);
        }

        /// <summary>
        /// Колесо меняет масштаб, удерживая на месте середину листа. Привязка к
        /// середине, а не к курсору: точка листа под мышью считалась бы от старой
        /// картинки, и при увеличении лист уезжал из-под курсора.
        /// </summary>
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta == 0)
                return;

            double next = CurrentZoom + (e.Delta > 0 ? ZoomStep : -ZoomStep);
            next = Math.Max(MinZoom, Math.Min(MaxZoom, next));

            if (Math.Abs(next - CurrentZoom) < 0.001)
                return;

            var model = DataContext as MainViewModel;
            var page = model?.SelectedPage;

            if (page?.Page == null)
                return;

            // Середина листа на экране до изменения масштаба: она же должна остаться
            // на месте после.
            double centreX = Canvas.GetLeft(Sheet) + Sheet.Width / 2;
            double centreY = Canvas.GetTop(Sheet) + Sheet.Height / 2;

            CurrentZoom = next;

            LayoutSheet(next);

            Canvas.SetLeft(Sheet, centreX - Sheet.Width / 2);
            Canvas.SetTop(Sheet, centreY - Sheet.Height / 2);

            e.Handled = true;
        }
    }
}