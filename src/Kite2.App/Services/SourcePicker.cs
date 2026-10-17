using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Kite2.App.Services
{
    /// <summary>
    /// Окно выбора источника сканирования.
    /// </summary>
    /// <remarks>
    /// Список устройств меняется при переподключении сканера, поэтому окно спрашивает
    /// источники заново и показывает то, что есть сейчас. Свой выбор пользователь
    /// узнаёт по галочке: сохранённый в настройках индекс после переподключения мог
    /// бы указывать на другое устройство.
    /// </remarks>
    public static class SourcePicker
    {
        /// <summary>
        /// Показывает окно и ждёт выбора.
        /// </summary>
        /// <param name="sources">Названия доступных источников.</param>
        /// <param name="current">Индекс источника из настроек.</param>
        /// <returns>Индекс выбранного источника или -1, если пользователь закрыл окно.</returns>
        public static int Ask(IReadOnlyList<string> sources, int current)
        {
            var list = new ListBox
            {
                ItemsSource = sources,
                SelectedIndex = current >= 0 && current < sources.Count ? current : 0,
                Margin = new Thickness(12),
                MinWidth = 320,
                MinHeight = 160,
                MaxHeight = 320
            };

            var accept = new Button
            {
                Content = "Выбрать",
                Width = 96,
                Height = 28,
                Margin = new Thickness(0, 0, 8, 0),
                IsDefault = true
            };

            var cancel = new Button
            {
                Content = "Отмена",
                Width = 96,
                Height = 28,
                IsCancel = true
            };

            var buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(12, 0, 12, 12)
            };

            buttons.Children.Add(accept);
            buttons.Children.Add(cancel);

            var panel = new StackPanel();
            panel.Children.Add(list);
            panel.Children.Add(buttons);

            var window = new Window
            {
                Title = UserDialogs.SelectSourceTitle,
                Content = panel,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false
            };

            accept.Click += (sender, e) => window.DialogResult = true;

            return window.ShowDialog() == true ? list.SelectedIndex : -1;
        }
    }
}