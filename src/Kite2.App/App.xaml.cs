using System;
using System.Diagnostics;
using System.Threading.Tasks;
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

        // Спеш не читать «вполглаза»: держит экран, пока человек не успел пробежать текст.
        private static readonly TimeSpan SplashMinimum = TimeSpan.FromSeconds(6);

        // Спит остаток минимального показа сплеша, если он уже истёк — ничего.
        private static async Task WaitSplashMinimum(Stopwatch shown)
        {
            TimeSpan remainder = SplashMinimum - shown.Elapsed;

            if (remainder > TimeSpan.Zero)
                await Task.Delay(remainder);
        }

        protected override async void OnStartup(StartupEventArgs e)
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
                $"Kite, версия {typeof(App).Assembly.GetName().Version}",
                $"Ключи запуска: {Logging.StartupArguments}",
                $"Каталог данных: {AppPaths.Data}",
                $"Разрядность процесса: {(Environment.Is64BitProcess ? "64" : "32")} бита",
                $"Тестовый режим: {(testMode ? "включён" : "выключен")}");

            AppSettings.Load();

            // Сбой процесса: лишь бы показать пользователю; в журнал это запишет
            // подписка из Logging.CatchBackgroundFailures().
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
                UserDialogs.Error(args.ExceptionObject as Exception);

            // Сплеш, как в первой версии: тяжёлая инициализация в фоне, шаги
            // видны на экране, окно программы появляется только готовым.
            var splash = new SplashWindow(DisplayVersion());
            splash.Show();

            var shown = Stopwatch.StartNew();

            IProgress<string> status = new Progress<string>(splash.SetStatus);

            status.Report("Очистка временных файлов...");
            await Task.Run(() =>
            {
                // Остатки прошлых запусков не нужны: документ живёт в памяти.
                try
                {
                    AppPaths.ClearFolder(AppPaths.Scan);
                    AppPaths.ClearFolder(AppPaths.Pages);
                }
                catch (Exception ex)
                {
                    Log.Warn(ex, "Не удалось очистить каталог сканирования");
                }
            });

            status.Report("Инициализация MagickNET...");
            try
            {
                await Task.Run(ImageMagickSetup.EnsureStarted);
            }
            catch (Exception ex)
            {
                UserDialogs.Error(ex);
            }

            status.Report("Инициализация пользовательского интерфейса...");

            // Спеш не должен моргнуть: основное окно показывается,
            // когда минимальное время показа сплеша отработало.
            await WaitSplashMinimum(shown);

            var viewModel = new MainViewModel();

            MainWindow = new MainWindow { DataContext = viewModel };

            MainWindow.Icon = ProgramIcon();

            // Версия в заголовке: какой сборкой человек работает, видно всегда.
            MainWindow.Title = "Kite " + DisplayVersion();

            // Модель и область просмотра соединяются здесь: ни одна из сторон о другой
            // не знает, и круг между ними не возникнет.
            MainWindow.Connect(viewModel);

            if (testMode)
                MainWindow.Title += " (тестовый режим)";

            MainWindow.Show();

            // Сплеш был показан первым, и WPF успел считать его главным
            // окном: без явного переназначения закрытие сплеша закрывает
            // и приложение.
            Application.Current.MainWindow = MainWindow;

            splash.Close();

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
