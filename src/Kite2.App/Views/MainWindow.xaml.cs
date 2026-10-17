using System;
using System.Windows;
using System.Windows.Input;

namespace Kite2.App.Views
{
    /// <summary>
    /// Главное окно программы.
    /// </summary>
    /// <remarks>
    /// Горячие клавиши задаются в разметке как привязки ввода: в WPF они не требуют
    /// перехвата клавиш в коде и работают, когда фокус на любом элементе окна,
    /// включая колонку страниц и область просмотра.
    /// </remarks>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}