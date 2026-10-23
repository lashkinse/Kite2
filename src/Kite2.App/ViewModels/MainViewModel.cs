using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kite2.App.Services;
using Kite2.App.Views;
using Kite2.Core;
using NLog;
using System.Windows.Threading;

namespace Kite2.App.ViewModels
{
    // Долгие операции идут в фоновом потоке: команды блокируются,
    // плашка показывает, что происходит.
    public sealed class MainViewModel : ObservableObject
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const int PageQuality = 25;

        private const int ResultNoticeSeconds = 4;

        private readonly ObservableCollection<PageViewModel> _pages = new ObservableCollection<PageViewModel>();
        private PageViewModel _selectedPage;
        private bool _busy;
        private bool _noticeVisible;
        private bool _noticeIsResult;
        private bool _cancelWanted;
        private string _noticeText = string.Empty;
        private CancellationTokenSource _scanCancellation;

        private readonly DispatcherTimer _resultNoticeTimer;

        public MainViewModel()
        {
            NewDocumentCommand = new RelayCommand(NewDocument, () => !IsBusy);
            SaveCommand = new RelayCommand(Save, () => !IsBusy && _pages.Count > 0);
            // Цветность и разрешение больше не задаёт программа: у драйвера
            // своё окно, где человек сам выбирает цвет, Ч/Б и всё прочее.
            // Одна кнопка — открыть окно и принять то, что оно отсканирует.
            ScanCommand = new RelayCommand(ScanAsync, () => !IsBusy);
            // «+» просто повторяет сканирование: режим выбирает драйвер.
            ScanAgainCommand = new RelayCommand(ScanAsync, () => !IsBusy);
            SettingsCommand = new RelayCommand(ChooseSource, () => !IsBusy);
            DeletePageCommand = new RelayCommand(DeletePage, () => !IsBusy && SelectedPage != null);
            MovePageUpCommand = new RelayCommand(() => MovePage(-1), () => !IsBusy && SelectedPage != null);
            MovePageDownCommand = new RelayCommand(() => MovePage(1), () => !IsBusy && SelectedPage != null);
            RotatePageCommand = new RelayCommand(RotatePage, () => !IsBusy && SelectedPage != null);
            CancelNoticeCommand = new RelayCommand(CancelOperation);
            LogFolderCommand = new RelayCommand(OpenLogsFolder);

            _commands = new[]
            {
                NewDocumentCommand, SaveCommand, ScanCommand, ScanAgainCommand, SettingsCommand, DeletePageCommand,
                MovePageUpCommand, MovePageDownCommand, RotatePageCommand
            };

            _resultNoticeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(ResultNoticeSeconds) };
            _resultNoticeTimer.Tick += OnResultNoticeTimeout;
        }

        public ObservableCollection<PageViewModel> Pages => _pages;

        public PageViewModel SelectedPage
        {
            get => _selectedPage;
            set
            {
                if (SetProperty(ref _selectedPage, value))
                    RefreshCommands();
            }
        }

        public bool IsBusy => _busy;

        public bool NoticeVisible
        {
            get => _noticeVisible;
            private set => SetProperty(ref _noticeVisible, value);
        }

        // Результат в плашке уходит сам, ход работы остаётся до конца операции.
        public bool NoticeIsResult
        {
            get => _noticeIsResult;
            private set => SetProperty(ref _noticeIsResult, value);
        }

        public string NoticeText
        {
            get => _noticeText;
            private set => SetProperty(ref _noticeText, value);
        }

        public bool CancelWanted
        {
            get => _cancelWanted;
            private set => SetProperty(ref _cancelWanted, value);
        }

        // Обновляются одной ссылкой: доступность меняется по одним и тем же причинам.
        private readonly RelayCommand[] _commands;

        public RelayCommand NewDocumentCommand { get; }

        public RelayCommand SaveCommand { get; }

        public RelayCommand ScanCommand { get; }

        public RelayCommand ScanAgainCommand { get; }

        public RelayCommand SettingsCommand { get; }

        public RelayCommand DeletePageCommand { get; }

        public RelayCommand MovePageUpCommand { get; }

        public RelayCommand MovePageDownCommand { get; }

        public RelayCommand RotatePageCommand { get; }

        public RelayCommand CancelNoticeCommand { get; }

        public RelayCommand LogFolderCommand { get; }

        // Подготовка после показа окна: чистка каталога и запуск ImageMagick
        // не должны задерживать раскрытие.
        public void Start()
        {
            try
            {
                // Остатки страниц прошлых запусков не нужны: документ живёт в памяти.
                AppPaths.ClearFolder(AppPaths.Scan);
                AppPaths.ClearFolder(AppPaths.Pages);
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Не удалось очистить каталог сканирования");
            }

            try
            {
                ImageMagickSetup.EnsureStarted();
            }
            catch (Exception ex)
            {
                UserDialogs.Error(ex);
            }

            CheckForUpdateAsync();
        }

        // --- Обновление ------------------------------------------------------

        // Повторный вопрос, если сеть не ответила: дешевле, чем перезапуск программы.
        private const int UpdateRepeatDelaySeconds = 5 * 60;

        // Один проверочный вопрос за сеанс и только вне тестового режима.
        private async void CheckForUpdateAsync()
        {
            if (FakeScanner.Enabled)
                return;

            var (update, answered) = await UpdateService.Latest();

            if (update == null)
            {
                // «Обновлений нет» — окончательный ответ, недоступная сеть — нет.
                if (!answered)
                    ScheduleRepeatCheck();

                return;
            }

            if (IsBusy)
                return;

            Log.Info("Доступно обновление: {0}", update.Version);

            if (!UserDialogs.Confirm("Обновление", Messages.UpdateQuestion))
                return;

            // Плашка показывает не только факт, но и ход загрузки.
            var percent = new Progress<int>(value => NoticeText = Messages.UpdatingProgress(value));

            SetBusy(true);
            ShowNotice(Messages.Updating, result: false, cancel: false);

            try
            {
                await UpdateService.Stage(update, percent);

                await Task.Run(() => UpdateService.Launch(update.Version));

                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Обновление не применилось");

                HideNotice();
                UserDialogs.Error(Messages.UpdateFailed);
            }
            finally
            {
                SetBusy(false);
            }
        }

        // Повтор приостанавливается на минуты: чаще дёргать GitHub незачем.
        private async void ScheduleRepeatCheck()
        {
            await Task.Delay(TimeSpan.FromSeconds(UpdateRepeatDelaySeconds));

            // Программа могла уже закрыться: вопроса по покойнику быть не должно.
            if (Application.Current != null && Application.Current.Windows.Count > 0)
                CheckForUpdateAsync();
        }

        // --- Журнал -----------------------------------------------------------

        // Путь одинаков на всякой машине: по нему журнал передают с другого компьютера.
        private void OpenLogsFolder()
        {
            try
            {
                Directory.CreateDirectory(AppPaths.Logs);

                Process.Start(new ProcessStartInfo { FileName = AppPaths.Logs, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                UserDialogs.Error(ex);
            }
        }

        // --- Сканирование ----------------------------------------------------

        // Порядок страниц задаётся временем создания файлов, поэтому композиция
        // идёт сразу после захвата.
        private async void ScanAsync()
        {
            if (!FakeScanner.Enabled && AppSettings.ScanSourceIndex == AppSettings.NotSelected)
            {
                UserDialogs.Error(Messages.ChooseSource);
                return;
            }

            var cancellation = new CancellationTokenSource();
            _scanCancellation = cancellation;

            try
            {
                // Вопрос задаётся между сериями, но не после серии без страниц:
                // пустой захват — провал, а не просьба продолжить.
                int scanned;

                do
                {
                    SetBusy(true);
                    ShowNotice(Messages.Scanning, result: false, cancel: true);

                    scanned = await ScanOnce(cancellation.Token);
                }
                while (scanned > 0 && AskToContinue());
            }
            catch (OperationCanceledException)
            {
                Log.Info("Сканирование прервано пользователем");
                HideNotice();
            }
            catch (Exception ex)
            {
                UserDialogs.Error(ex);
            }
            finally
            {
                DisposeScanCancellation();
                SetBusy(false);

                // Плашку хода убираем всегда, даже после ошибки; результат убирается сам.
                if (NoticeVisible && !NoticeIsResult)
                    HideNotice();
            }
        }

        private async Task<int> ScanOnce(CancellationToken cancellationToken)
        {
            var request = new ScanRequest
            {
                SourceIndex = FakeScanner.Enabled ? 0 : AppSettings.ScanSourceIndex
            };

            await Task.Run(() =>
            {
                ScannerService.Capture(request, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                return PageImage.ComposeDirectory(request, PageQuality);
            }, cancellationToken);

            // Страницы забираются сразу: следующая серия очистит каталог
            // вместе с только что отсканированным.
            var pages = await Task.Run(() => BuildPages(request.Directory), cancellationToken);

            AddPages(pages);

            return pages.Count;
        }

        // В тестовом режиме вопрос остановил бы и прогон, и автоматическую проверку.
        private bool AskToContinue()
        {
            if (FakeScanner.Enabled && !FakeScanner.AlwaysAsk)
            {
                Log.Debug("Вопрос о продолжении пропущен: тестовый режим");
                return false;
            }

            return ScanContinueWindow.Ask(Application.Current.MainWindow);
        }

        private List<PageViewModel> BuildPages(string directory)
        {
            var pages = new List<PageViewModel>();

            int number = 1;

            foreach (string file in Directory.GetFiles(directory, "*.jpg").OrderBy(Path.GetFileName))
                pages.Add(new PageViewModel(PageImage.Store(file), number++));

            return pages;
        }

        private void AddPages(List<PageViewModel> pages)
        {
            foreach (PageViewModel page in pages)
                _pages.Add(page);

            if (_pages.Count > 0)
                SelectedPage = _pages[_pages.Count - 1];

            RenumberPages();
            RefreshCommands();

            // В плашке — размер серии, а не документа: вопрос о продолжении
            // относится к следующему захвату.
            if (pages.Count > 0)
                ShowNotice(Messages.PagesAdded(pages.Count), result: true, cancel: false);
        }

        // Номера проставляются подряд после всякого изменения состава: иначе
        // остаются пропуски и повторы.
        private void RenumberPages()
        {
            for (int i = 0; i < _pages.Count; i++)
                _pages[i].Number = i + 1;
        }

        // --- Сохранение -------------------------------------------------------

        private async void Save()
        {
            string path = SaveFilePicker.Ask(".pdf");

            if (string.IsNullOrEmpty(path))
                return;

            string[] files = _pages.Select(page => page.FilePath).ToArray();

            // Режим кодировки — всегда «как отсканировано»: цветность и прочее
            // задаёт драйвер в своём окне, и программа ничего не решает за него.
            ScanMode mode = ScanMode.Colour;

            SetBusy(true);
            ShowNotice(Messages.Saving, result: false, cancel: false);

            var watch = Stopwatch.StartNew();

            try
            {
                await Task.Run(() => DocumentExporter.Export(files, path, mode));

                // В журнал пишется то, что появилось на диске: у картинок это
                // отдельные файлы с номерами страниц, а не запрошенное имя.
                Log.Info("Экспорт завершён: {0}, мс={1}, файлов={2}", path, watch.ElapsedMilliseconds, Describe(path));

                ShowNotice(Messages.Saved + " " + Path.GetFileName(path), result: true, cancel: false);
            }
            catch (Exception ex)
            {
                UserDialogs.Error(ex);
                HideNotice();
            }
            finally
            {
                SetBusy(false);
            }
        }

        // Для картинок в запрошенном имени может не быть ничего — вместо него
        // появляются файлы с номерами страниц.
        private static string Describe(string requestedPath)
        {
            if (File.Exists(requestedPath))
            {
                FileInfo single = new FileInfo(requestedPath);
                return single.Name + " (" + single.Length + " байт)";
            }

            string folder = Path.GetDirectoryName(requestedPath);
            string baseName = Path.GetFileNameWithoutExtension(requestedPath);

            FileInfo[] written = new DirectoryInfo(folder)
                .GetFiles(baseName + "*.???")
                .Where(file => file.Extension.Equals(Path.GetExtension(requestedPath), StringComparison.OrdinalIgnoreCase))
                .OrderBy(file => file.Name)
                .ToArray();

            return string.Join(", ", written.Select(file => file.Name + " (" + file.Length + " байт)"));
        }

        // --- Работа со страницами --------------------------------------------

        private void NewDocument()
        {
            if (_pages.Count > 0 && !UserDialogs.Confirm(UserDialogs.NewDocumentTitle, UserDialogs.NewDocumentQuestion))
                return;

            ClearPages();
        }

        private void ClearPages()
        {
            foreach (PageViewModel page in _pages)
                page.Delete();

            _pages.Clear();
            SelectedPage = null;

            RefreshCommands();
        }

        private void DeletePage()
        {
            PageViewModel page = SelectedPage;

            if (page == null)
                return;

            int index = _pages.IndexOf(page);

            _pages.Remove(page);
            page.Delete();

            // После удаления выбирается соседняя страница: область просмотра
            // не должна гаснуть.
            SelectedPage = _pages.Count == 0
                ? null
                : _pages[Math.Min(index, _pages.Count - 1)];

            RenumberPages();
            RefreshCommands();
        }

        private void MovePage(int step)
        {
            int index = _pages.IndexOf(SelectedPage);

            if (index < 0)
                return;

            int target = index + step;

            if (target < 0 || target > _pages.Count - 1)
                return;

            _pages.Move(index, target);

            RenumberPages();
        }

        // Страница не меняется: фрагмент уходит рядом, документ остаётся прежним.
        public async void SaveSelectedFragment(Rect area)
        {
            PageViewModel page = SelectedPage;

            if (page == null)
                return;

            if (area.Width < 1 || area.Height < 1)
            {
                UserDialogs.Error(Messages.NothingSelected);
                return;
            }

            string target = SaveFilePicker.AskFragment();

            if (string.IsNullOrEmpty(target))
                return;

            string source = page.FilePath;

            SetBusy(true);

            try
            {
                var watch = Stopwatch.StartNew();

                await Task.Run(() => PageImage.SaveFragment(
                    source, (int)area.X, (int)area.Y, (int)area.Width, (int)area.Height, target));

                Log.Info(
                    "Фрагмент сохранён: {0}, {1} x {2} пикселей, мс={3}, байт={4}",
                    target, (int)area.Width, (int)area.Height,
                    watch.ElapsedMilliseconds, new FileInfo(target).Length);

                ShowNotice(Messages.FragmentSaved + " " + Path.GetFileName(target), result: true, cancel: false);
            }
            catch (Exception ex)
            {
                UserDialogs.Error(ex);
            }
            finally
            {
                SetBusy(false);
            }
        }

        // Поворот меняет файл, а не только картинку на экране. Область просмотра
        // следит за выбором страницы, поэтому после перечитывания ей это
        // сообщение и нужно.
        private void RotatePage()
        {
            PageViewModel page = SelectedPage;

            if (page == null)
                return;

            try
            {
                PageImage.Rotate(page.FilePath, 180.0);
                page.Reload();

                OnPropertyChanged(nameof(SelectedPage));
            }
            catch (Exception ex)
            {
                UserDialogs.Error(ex);
            }
        }

        private void ChooseSource()
        {
            IReadOnlyList<string> sources;

            try
            {
                sources = ScannerService.GetSources();
            }
            catch (Exception ex)
            {
                UserDialogs.Error(ex);
                return;
            }

            if (sources.Count == 0)
            {
                UserDialogs.Error(Messages.NoSources);
                return;
            }

            int chosen = SourcePicker.Ask(Application.Current.MainWindow, sources, AppSettings.ScanSourceIndex);

            if (chosen < 0)
                return;

            AppSettings.ScanSourceIndex = chosen;
            AppSettings.Save();
        }

        // --- Уведомление -----------------------------------------------------

        private void ShowNotice(string text, bool result, bool cancel)
        {
            NoticeText = text;
            NoticeIsResult = result;
            CancelWanted = cancel;
            NoticeVisible = true;

            // Отсчёт заново на каждое сообщение.
            _resultNoticeTimer.Stop();

            if (result)
                _resultNoticeTimer.Start();
        }

        private void HideNotice()
        {
            _resultNoticeTimer.Stop();

            NoticeVisible = false;
            CancelWanted = false;
        }

        private void OnResultNoticeTimeout(object sender, EventArgs args)
        {
            _resultNoticeTimer.Stop();

            HideNotice();
        }

        // Отмена закрывает сессию TWAIN через помощника: ждать её поток перестаёт.
        private void CancelOperation()
        {
            if (_scanCancellation == null)
                return;

            try
            {
                _scanCancellation.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // Сканирование завершилось раньше, чем нажали «Отмена».
            }
        }

        private void SetBusy(bool busy)
        {
            _busy = busy;
            RefreshCommands();
        }

        private void DisposeScanCancellation()
        {
            CancellationTokenSource cancellation = _scanCancellation;
            _scanCancellation = null;
            cancellation?.Dispose();
        }

        private void RefreshCommands()
        {
            foreach (RelayCommand command in _commands)
                command.NotifyCanExecuteChanged();

            CommandManager.InvalidateRequerySuggested();
        }
    }
}
