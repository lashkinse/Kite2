using System.Windows;
using System.Windows.Media;

namespace Kite2.App.Controls
{
    // Присоединённые свойства кнопок: содержимое кнопки занято значком,
    // а вид кнопки описан один раз в стиле.
    public static class CommandBar
    {
        public static readonly DependencyProperty HintProperty =
            DependencyProperty.RegisterAttached(
                "Hint",
                typeof(string),
                typeof(CommandBar),
                new PropertyMetadata(string.Empty));

        // Цвет подсказки свой: серый на синей акцентной кнопке читается плохо.
        public static readonly DependencyProperty HintBrushProperty =
            DependencyProperty.RegisterAttached(
                "HintBrush",
                typeof(Brush),
                typeof(CommandBar),
                new PropertyMetadata(null));

        // Помечает кнопку, которую повторит «+»: обе кнопки сканера выглядят
        // одинаково, и без отметки не видно текущего режима.
        public static readonly DependencyProperty IsNextProperty =
            DependencyProperty.RegisterAttached(
                "IsNext",
                typeof(bool),
                typeof(CommandBar),
                new PropertyMetadata(false));

        public static string GetHint(DependencyObject element) => (string)element.GetValue(HintProperty);

        public static void SetHint(DependencyObject element, string value) => element.SetValue(HintProperty, value);

        public static Brush GetHintBrush(DependencyObject element) => (Brush)element.GetValue(HintBrushProperty);

        public static void SetHintBrush(DependencyObject element, Brush value) => element.SetValue(HintBrushProperty, value);

        public static bool GetIsNext(DependencyObject element) => (bool)element.GetValue(IsNextProperty);

        public static void SetIsNext(DependencyObject element, bool value) => element.SetValue(IsNextProperty, value);
    }
}