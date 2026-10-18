using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Kite2.App.Services;
using Kite2.App.ViewModels;
using Kite2.App.Views;
using Kite2.Core;
using NLog;

namespace Kite2.App
{
    /// <summary>
    /// Точка входа. Запуск программы и создание главного окна.
    /// </summary>
    public partial class App : Application
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

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

            MainWindow.Icon = ProgramIcon();

            // Модель и область просмотра соединяются здесь: ни одна из сторон о другой
            // не знает, и круг между ними не возникнет.
            MainWindow.Connect(viewModel);

            if (testMode)
                MainWindow.Title += " (тестовый режим)";

            MainWindow.Show();

            viewModel.Start();
        }

        /// <summary>
        /// Значок программы для окна. Берётся из самого файла программы: WPF значок
        /// из файла не достаёт, и на кнопке панели задач вставало серое окно.
        /// </summary>
        private static ImageSource ProgramIcon()
        {
            try
            {
                using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(ExecutablePath()))
                {
                    if (icon == null)
                        return null;

                    ImageSource image = Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromWidthAndHeight(32, 32));

                    image.Freeze();

                    return image;
                }
            }
            catch (Exception ex)
            {
                // Без значка программа работает, а из-за него ронять запуск нельзя.
                Log.Warn("Значок программы не загружен: {0}", ex.Message);

                return null;
            }
        }

        /// <summary>
        /// Путь к файлу программы: у окна WPF своего <c>ExecutablePath</c> нет.
        /// </summary>
        private static string ExecutablePath()
        {
            using (var process = System.Diagnostics.Process.GetCurrentProcess())
                return process.MainModule.FileName;
        }
    }
}