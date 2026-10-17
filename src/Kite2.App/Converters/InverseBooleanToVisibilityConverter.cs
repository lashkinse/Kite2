using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Kite2.App.Converters
{
    /// <summary>
    /// Показывает элемент, когда значение истина. Необратный — наоборот.
    /// </summary>
    /// <remarks>
    /// В WPF есть только прямой преобразователь, а в разметке он нужен и обратный:
    /// так прячутся элементы, которые показывать незачем.
    /// </remarks>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool flag && flag ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility visibility && visibility != Visibility.Visible;
        }
    }
}