using System.Windows;

namespace Kite2.App.Controls
{
    /// <summary>
    /// Дополнительное свойство кнопки верхней панели: сочетание клавиш под подписью.
    /// </summary>
    /// <remarks>
    /// Отдельное свойство, а не часть содержимого кнопки: содержимое уже занято
    /// значком, и подпись с сочетанием берутся из своих мест. Заодно вид кнопки
    /// описан один раз в стиле, а не в каждой кнопке.
    /// </remarks>
    public static class CommandBar
    {
        /// <summary>Текст сочетания клавиш, например «Ctrl+N».</summary>
        public static readonly DependencyProperty HintProperty =
            DependencyProperty.RegisterAttached(
                "Hint",
                typeof(string),
                typeof(CommandBar),
                new PropertyMetadata(string.Empty));

        public static string GetHint(DependencyObject element) => (string)element.GetValue(HintProperty);

        public static void SetHint(DependencyObject element, string value) => element.SetValue(HintProperty, value);
    }
}