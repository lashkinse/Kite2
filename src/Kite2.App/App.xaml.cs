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
    public partial class App : Application
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public new MainWindow MainWindow { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Журнал настраивается первым: с этого момента любая ошибка попадает в файл.
            Logging.Initialize(e.Args);
            Logging.CatchBackgroundFailures();

            // Отрисовка только софтом: у части видеокарт WPF «слепнет» окном.
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            // Тестовый режим включается ключом запуска: страницы рисует программа.
            bool testMode = FakeScanner.EnableFromArguments(e.Args);

            Logging.WriteStartup(
                $"Kite2, версия {typeof(App).Assembly.GetName().Version}",
                $"Ключи запуска: {Logging.StartupArguments}",
                $"Каталог данных: {AppPaths.Data}",
                $"Разрядность процесса: {(Environment.Is64BitProcess ? "64" : "32")} бита",
                $"Тестовый режим: {(testMode ? "включён" : "выключен")}");

            AppSettings.Load();

            // Сбой процесса: лишь бы показать пользователю; в журнал это запишет
            // подписка из Logging.CatchBackgroundFailures().
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                UserDialogs.Error(args.ExceptionObject as Exception);

            var viewModel = new MainViewModel();

            MainWindow = new MainWindow { DataContext = viewModel };

            MainWindow.Icon = ProgramIcon();

            // Версия в заголовке: какой сборкой человек работает, видно всегда.
            MainWindow.Title = "Kite2 " + DisplayVersion();

            // Модель и область просмотра соединяются здесь: ни одна из сторон о другой
            // не знает, и круг между ними не возникнет.
            MainWindow.Connect(viewModel);

            if (testMode)
                MainWindow.Title += " (тестовый режим)";

            MainWindow.Show();

            viewModel.Start();
        }

        // Значок берётся из самого файла программы: WPF не достаёт значок
        // из файла, и на кнопке панели задач вставало серое окно.
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

        // У окна WPF своего ExecutablePath нет.
        private static string ExecutablePath()
        {
            using (var process = System.Diagnostics.Process.GetCurrentProcess())
                return process.MainModule.FileName;
        }

        // Ревизия не показывается: «1.8.1.0» читается как номер сборки CI,
        // а для человека существует только первые три части.
        private static string DisplayVersion()
        {
            Version version = typeof(App).Assembly.GetName().Version;

            return version.Revision == 0 && version.Build == 0
                ? version.Major + "." + version.Minor
                : version.Major + "." + version.Minor + "." + version.Build;
        }
    }
}
