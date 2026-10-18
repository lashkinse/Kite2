using System.Windows;
using System.Windows.Media;

namespace Kite2.App.Controls
{
        // Отдельные свойства, а не часть содержимого кнопки: содержимое уже занято
    // значком. Заодно вид кнопки описан один раз в стиле, а не в каждой кнопке.
    public static class CommandBar
    {
        public static readonly DependencyProperty HintProperty =
            DependencyProperty.RegisterAttached(
                "Hint",
                typeof(string),
                typeof(CommandBar),
                new PropertyMetadata(string.Empty));

        // Цвет подсказки разный у тёмной и у акцентной кнопки: серая на синем
        // читалась плохо. В шаблоне он берётся привязкой к самой кнопке, поэтому
        // общий шаблон подходит обеим.
        public static readonly DependencyProperty HintBrushProperty =
            DependencyProperty.RegisterAttached(
                "HintBrush",
                typeof(Brush),
                typeof(CommandBar),
                new PropertyMetadata(null));

        // Помечает кнопку, по которой пойдёт следующее сканирование: вокруг неё
        // рисуется та же пунктирная рамка, что вокруг выделения на странице. Без
        // отметки не видно, какой режим повторит «+»: обе кнопки выглядят одинаково.
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