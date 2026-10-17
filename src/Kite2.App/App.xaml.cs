using System;
using System.Windows;
using Kite2.App.Services;
using Kite2.App.ViewModels;
using Kite2.App.Views;
using Kite2.Core;

namespace Kite2.App
{
    /// <summary>
    /// Точка входа. Запуск программы и создание главного окна.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Главное окно программы.
        /// </summary>
        /// <remarks>
        /// Пользовательские настройки читаются до создания окна, чтобы выбранный
        /// источник сканирования был известен заранее.
        /// </remarks>
        public new MainWindow MainWindow { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppSettings.Load();

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                UserDialogs.Error(args.ExceptionObject as Exception);

            var viewModel = new MainViewModel();

            MainWindow = new MainWindow { DataContext = viewModel };
            MainWindow.Show();

            viewModel.Start();
        }
    }
}