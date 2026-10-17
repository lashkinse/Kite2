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

            // Журнал настраивается первым: с этого момента любая ошибка попадает в файл.
            Logging.Initialize(e.Args);
            Logging.CatchBackgroundFailures();

            // Тестовый режим включается ключом запуска: страницы рисует программа. В заголовке
            // окна он помечен, чтобы тестовый лист не приняли за настоящий документ.
            bool testMode = FakeScanner.EnableFromArguments(e.Args);

            Logging.WriteStartup(
                $"Kite2, версия {typeof(App).Assembly.GetName().Version}",
                $"Ключи запуска: {Logging.StartupArguments}",
                $"Каталог данных: {AppPaths.Data}",
                $"Разрядность процесса: {(Environment.Is64BitProcess ? "64" : "32")} бита",
                $"Тестовый режим: {(testMode ? "включён" : "выключен")}");

            AppSettings.Load();

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                UserDialogs.Error(args.ExceptionObject as Exception);

            var viewModel = new MainViewModel();

            MainWindow = new MainWindow { DataContext = viewModel };

            // Модель и область просмотра соединяются здесь: ни одна из сторон о другой
            // не знает, и круг между ними не возникнет.
            MainWindow.Connect(viewModel);

            if (testMode)
                MainWindow.Title += " (тестовый режим)";

            MainWindow.Show();

            viewModel.Start();
        }
    }
}