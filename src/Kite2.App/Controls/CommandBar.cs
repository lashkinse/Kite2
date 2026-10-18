using System.Windows;
using System.Windows.Media;

namespace Kite2.App.Controls
{
    /// <summary>
    /// Дополнительные свойства кнопки верхней панели: сочетание клавиш под подписью,
    /// его цвет и отметка «сработает следующей».
    /// </summary>
    /// <remarks>
    /// Отдельные свойства, а не часть содержимого кнопки: содержимое уже занято
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

        /// <summary>
        /// Цвет подсказки с сочетанием клавиш. У тёмной и у акцентной кнопки он
        /// разный: серая подсказка на синем читалась плохо. В шаблоне берётся через
        /// привязку к самой кнопке, поэтому общий шаблон подходит обеим.
        /// </summary>
        public static readonly DependencyProperty HintBrushProperty =
            DependencyProperty.RegisterAttached(
                "HintBrush",
                typeof(Brush),
                typeof(CommandBar),
                new PropertyMetadata(null));

        /// <summary>
        /// Помечает кнопку, по которой пойдёт следующее сканирование: вокруг неё
        /// рисуется та же пунктирная рамка, что вокруг выделения на странице.
        /// </summary>
        /// <remarks>
        /// Клавиша «+» повторяет предыдущий режим, и без отметки не видно, какой
        /// именно: кнопки «Ч/Б» и «Цветное» на панели выглядят одинаково.
        /// </remarks>
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