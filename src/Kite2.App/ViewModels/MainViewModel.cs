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
    // Долгие операции идут через Task.Run: захват и сохранение считают минуты, и в
    // главном потоке им места нет. Пока операция идёт, команды заблокированы, а
    // плашка показывает, что происходит.
    public sealed class MainViewModel : ObservableObject
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const int PageQuality = 25;

        private const int ScanDpi = 300;

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
            ScanMonoCommand = new RelayCommand(() => ScanAsync(CaptureColour.BlackAndWhite), () => !IsBusy);
            ScanColourCommand = new RelayCommand(() => ScanAsync(CaptureColour.Colour), () => !IsBusy);
            ScanAgainCommand = new RelayCommand(ScanAgain, () => !IsBusy);
            SettingsCommand = new RelayCommand(ChooseSource, () => !IsBusy);
            DeletePageCommand = new RelayCommand(DeletePage, () => !IsBusy && SelectedPage != null);
            MovePageUpCommand = new RelayCommand(() => MovePage(-1), () => !IsBusy && SelectedPage != null);
            MovePageDownCommand = new RelayCommand(() => MovePage(1), () => !IsBusy && SelectedPage != null);
            RotatePageCommand = new RelayCommand(RotatePage, () => !IsBusy && SelectedPage != null);
            CancelNoticeCommand = new RelayCommand(CancelOperation);

            _commands = new[]
            {
                NewDocumentCommand, SaveCommand, ScanMonoCommand, ScanColourCommand, ScanAgainCommand, SettingsCommand, DeletePageCommand,
                MovePageUpCommand, MovePageDownCommand, RotatePageCommand
            };

            // Плашка с результатом («сохранено», «добавлено страниц») убирается сама:
            // сообщение не должно висеть, пока пользователь смотрит на страницу.
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

        // Результат в акцентном цвете и убирается сам, ход работы остаётся, пока идёт
        // операция.
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

        // Доступность всех команд меняется по одним и тем же причинам — идёт операция
        // или нет, выбрана страница или нет, — поэтому они и обновляются одним списком.
        private readonly RelayCommand[] _commands;

        public RelayCommand NewDocumentCommand { get; }

        public RelayCommand SaveCommand { get; }

        public RelayCommand ScanMonoCommand { get; }

        public RelayCommand ScanColourCommand { get; }

        public RelayCommand ScanAgainCommand { get; }

        public RelayCommand SettingsCommand { get; }

        public RelayCommand DeletePageCommand { get; }

        public RelayCommand MovePageUpCommand { get; }

        public RelayCommand MovePageDownCommand { get; }

        public RelayCommand RotatePageCommand { get; }

        public RelayCommand CancelNoticeCommand { get; }

        private ScanMode CurrentScanMode { get; set; } = ScanMode.Greyscale;

        // Подготовка идёт после показа окна, а не в конструкторе: иначе окно успело бы
        // повисеть белым, пока чистится каталог и запускается ImageMagick.
        public void Start()
        {
            try
            {
                AppPaths.EmptyFolder(AppPaths.Scan);
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

            // Проверка обновления тоже после показа окна: вопрос не должен
            // выскочить раньше, чем оно раскрылось.
            CheckForUpdateAsync();
        }

        // --- Обновление ------------------------------------------------------

        // Повторная проверка, если первая не получила ответа: сбросы и обрывы в
        // наших сетях случаются на минуты, а вопрос всё равно нужен при входе —
        // хотя бы позже, без перезапуска программы.
        private const int UpdateRepeatDelaySeconds = 5 * 60;

        // Программа спрашивает об обновлении один раз при входе и только вне
        // тестового режима: автоматизация не должна зависеть от сети, да и вопрос
        // в ней кликали бы только зря.
        private async void CheckForUpdateAsync()
        {
            if (FakeScanner.Enabled)
                return;

            var (update, answered) = await UpdateService.Latest();

            if (update == null)
            {
                // «Обновлений нет» — окончательный ответ: каталог отвечал, а вот
                // недоступная сеть повтору подлежит, вопрос всё равно нужен при входе.
                if (!answered)
                    ScheduleRepeatCheck();

                return;
            }

            // Вопрос во время уже идущей операции уводит фокус: не встревает в неё.
            if (IsBusy)
                return;

            Log.Info("Доступно обновление: {0}", update.Version);

            if (!UserDialogs.Confirm("Обновление", Messages.UpdateQuestion))
                return;

            // Плашка показывает не только факт загрузки, но и её ход: проценты идут
            // из фонового потока и пишутся в то же свойство.
            var percent = new Progress<int>(value => NoticeText = Messages.UpdatingProgress(value));

            SetBusy(true);
            ShowNotice(Messages.Updating, result: false, cancel: false);

            try
            {
                await UpdateService.Stage(update, percent);

                // Дальше работает обновлятор: дождётся выхода, подменит файлы и
                // запустит программу снова, уже новой версии.
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
        private void ScheduleRepeatCheck()
        {
            // Ожидание прямо на диспетчере: вопрос поднимается в главном потоке,
            // как обычный вопрос при входе.
            async void Delayed()
            {
                await Task.Delay(TimeSpan.FromSeconds(UpdateRepeatDelaySeconds));

                // Программа могла уже закрыться: вопроса по покойнику быть не должно.
                if (Application.Current != null && Application.Current.Windows.Count > 0)
                    CheckForUpdateAsync();
            }

            Delayed();
        }

        // --- Сканирование ----------------------------------------------------

        // Порядок страниц задаётся временем создания файлов, поэтому композиция идёт
        // сразу после захвата, пока порядок ещё виден.
        private async void ScanAsync(CaptureColour colour)
        {
            if (!FakeScanner.Enabled && AppSettings.ScanSourceIndex == AppSettings.NotSelected)
            {
                UserDialogs.Error(Messages.ChooseSource);
                return;
            }

            RememberScanColour(colour);

            var cancellation = new CancellationTokenSource();
            _scanCancellation = cancellation;

            try
            {
                // Серии повторяются, пока пользователь не скажет «завершить». Вопрос
                // задаётся между сериями, а не после каждой: один сеанс работы со
                // сканером — это обычно несколько серий подряд.
                do
                {
                    SetBusy(true);
                    ShowNotice(Messages.Scanning, result: false, cancel: true);

                    await ScanOnce(colour, cancellation.Token);
                }
                while (AskToContinue());
            }
            catch (OperationCanceledException)
            {
                Log.Info("Сканирование прервано пользователем");
                HideNotice();
            }
            catch (SourceNotAvailableException)
            {
                UserDialogs.Error(Messages.SourceUnavailable);
            }
            catch (Exception ex)
            {
                UserDialogs.Error(ex);
            }
            finally
            {
                DisposeScanCancellation();
                SetBusy(false);

                // Плашка «Выполняется сканирование» убирается всегда: после ошибки
                // она иначе осталась бы висеть, а её «Отмена» уже ничего не отменяет,
                // потому что источник отмены освобождён. Плашку с результатом не трогаем:
                // она убирается сама.
                if (NoticeVisible && !NoticeIsResult)
                    HideNotice();
            }
        }

        private CaptureColour CurrentColour { get; set; } = CaptureColour.BlackAndWhite;

        // Без отметки не видно, какой режим повторит «+»: кнопки «Ч/Б» и «Цветное»
        // на панели выглядят одинаково. Отметка нужна и до первого сканирования — «+»
        // и так повторяет чёрно-белое.
        public bool LastScanIsMono { get; private set; } = true;

        public bool LastScanIsColour { get; private set; }

        private void RememberScanColour(CaptureColour colour)
        {
            CurrentColour = colour;

            LastScanIsMono = colour == CaptureColour.BlackAndWhite;
            LastScanIsColour = colour == CaptureColour.Colour;

            OnPropertyChanged(nameof(LastScanIsMono));
            OnPropertyChanged(nameof(LastScanIsColour));
        }

        // «+» повторяет предыдущий режим, а не всегда чёрно-белый: иначе после
        // цветной серии он тихо переключил бы программу в другой режим, и страницы
        // вышли бы не те, что человек ожидал.
        private void ScanAgain()
        {
            ScanAsync(CurrentColour);
        }

        private async Task ScanOnce(CaptureColour colour, CancellationToken cancellationToken)
        {
            var request = new ScanRequest
            {
                SourceIndex = FakeScanner.Enabled ? 0 : AppSettings.ScanSourceIndex,
                Dpi = ScanDpi,
                Colour = colour
            };

            CurrentScanMode = request.Mode;

            await Task.Run(() =>
            {
                // Проверка источника идёт здесь же: она занимает время, а к моменту
                // разговора с драйвером устройство вполне могло исчезнуть.
                if (!ScannerService.IsSourceAvailable(request.SourceIndex))
                    throw new SourceNotAvailableException();

                ScannerService.Capture(request, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                return PageImage.ComposeDirectory(request, PageQuality);
            }, cancellationToken);

            // Пока серия идёт, каталог наполняется; страницы забираются сразу, иначе
            // следующая серия очистит каталог вместе с только что отсканированным.
            var pages = await Task.Run(() => BuildPages(request.Directory), cancellationToken);

            AddPages(pages);
        }

        // В тестовом режиме вопрос не задаётся: там всё равно никто не сидит перед
        // окном, а вопрос остановил бы и прогон, и автоматическую проверку.
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
                pages.Add(new PageViewModel(file, number++));

            return pages;
        }

        private void AddPages(IEnumerable<PageViewModel> pages)
        {
            int added = 0;

            foreach (PageViewModel page in pages)
            {
                _pages.Add(page);
                added++;
            }

            if (_pages.Count > 0)
                SelectedPage = _pages[_pages.Count - 1];

            RenumberPages();
            RefreshCommands();

            // Показано число страниц в этой серии, а не в документе: после вопроса о
            // продолжении «добавлено 3» относилось бы к предыдущему захвату.
            if (added > 0)
                ShowNotice(Messages.PagesAdded(added), result: true, cancel: false);
        }

        // После вставки и удаления между номерами остаются пропуски, и подпись
        // «страница 2» попадает на страницу 1, 3, 2 — поэтому номера проставляются
        // подряд каждый раз.
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
            ScanMode mode = CurrentScanMode;

            SetBusy(true);
            ShowNotice(Messages.Saving, result: false, cancel: false);

            var watch = Stopwatch.StartNew();

            try
            {
                await Task.Run(() => DocumentExporter.Export(files, path, mode));

                // В журнал пишется то, что действительно появилось на диске. У картинок
                // это не запрошенное имя, а отдельные файлы с номерами страниц, поэтому
                // размер запрошенного пути мог бы и не существовать.
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

            // После удаления выбирается соседняя страница, иначе область просмотра
            // гаснет, как будто удалены все страницы.
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

        // Поворот меняет файл, а не только картинку на экране: страница уходит в
        // документ такой, какой лежит на диске, иначе после сохранения она была бы
        // повёрнута не так, как показано. Область просмотра следит только за выбором
        // страницы, поэтому после перечитывания ей это сообщение и нужно.
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

            // Отсчёт заново на каждое сообщение: два результата подряд не должны
            // ждать друг друга.
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

        // Отмена у WPF — это закрытие окна, но закрывать нечего: закрывается сессия
        // TWAIN, и ждать её поток перестаёт.
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
