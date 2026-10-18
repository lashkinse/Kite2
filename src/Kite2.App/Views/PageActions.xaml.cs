using System.Windows.Controls;

namespace Kite2.App.Views
{
    /// <summary>
    /// Полоса команд над выбранной страницей.
    /// </summary>
    /// <remarks>
    /// Отдельный элемент, а не часть колонки страниц: колонка прокручивается, а
    /// полоса должна стоять на месте при любой длине документа.
    /// </remarks>
    public partial class PageActions : UserControl
    {
        public PageActions()
        {
            InitializeComponent();
        }
    }
}
