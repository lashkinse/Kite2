using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kite2.App.Services;
using Kite2.Core;
using NLog;

namespace Kite2.App.ViewModels
{
    /// <summary>
    /// Главная модель представления: команды верхней панели, список страниц и
    /// уведомление о ходе работы.
    /// </summary>
    /// <remarks>
    /// Долгие операции идут через <see cref="Task.Run"/>: захват и сохранение считают
    /// минуты, и в главном потоке им места нет. Пока операция идёт, команды
    /// заблокированы, а плашка показывает, что происходит.
    /// </remarks>
    public sealed class MainViewModel : ObservableObject
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>Качество JPEG страницы.</summary>
        private const int PageQuality = 25;

        /// <summary>Разрешение захвата.</summary>
        private const int ScanDpi = 300;

        private readonly ObservableCollection<PageViewModel> _pages = new ObservableCollection<PageViewModel>();
        private PageViewModel _selectedPage;
        private bool _busy;
        private bool _isSaveMenuOpen;
        private bool _noticeVisible;
        private bool _noticeIsResult;
        private bool _cancelWanted;
        private string _noticeText = string.Empty;
        private CancellationTokenSource _scanCancellation;

        public MainViewModel()
        {
            NewDocumentCommand = new RelayCommand(NewDocument, () => !IsBusy);
            SaveCommand = new RelayCommand(BeginSave, () => !IsBusy && _pages.Count > 0);
            SaveAsPdfCommand = new RelayCommand(() => Save(SaveFilePicker.FilterPdf), () => !IsBusy && _pages.Count > 0);
            SaveAsZipCommand = new RelayCommand(() => Save(SaveFilePicker.FilterZip), () => !IsBusy && _pages.Count > 0);
            SaveAsJpgCommand = new RelayCommand(() => Save(SaveFilePicker.FilterJpg), () => !IsBusy && _pages.Count > 0);
            SaveAsTiffCommand = new RelayCommand(() => Save(SaveFilePicker.FilterTiff), () => !IsBusy && _pages.Count > 0);
            ScanMonoCommand = new RelayCommand(() => ScanAsync(CaptureColour.BlackAndWhite), () => !IsBusy);
            ScanColourCommand = new RelayCommand(() => ScanAsync(CaptureColour.Colour), () => !IsBusy);
            SettingsCommand = new RelayCommand(ChooseSource, () => !IsBusy);
            DeletePageCommand = new RelayCommand(DeletePage, () => !IsBusy && SelectedPage != null);
            MovePageUpCommand = new RelayCommand(() => MovePage(-1), () => !IsBusy && SelectedPage != null);
            MovePageDownCommand = new RelayCommand(() => MovePage(1), () => !IsBusy && SelectedPage != null);
            RotatePageCommand = new RelayCommand(RotatePage, () => !IsBusy && SelectedPage != null);
            CancelNoticeCommand = new RelayCommand(CancelOperation);

            _commands = new[]
            {
                NewDocumentCommand, SaveCommand, SaveAsPdfCommand, SaveAsZipCommand, SaveAsJpgCommand,
                SaveAsTiffCommand, ScanMonoCommand, ScanColourCommand, SettingsCommand, DeletePageCommand,
                MovePageUpCommand, MovePageDownCommand, RotatePageCommand
            };
        }

        /// <summary>Отсканированные страницы в порядке документа.</summary>
        public ObservableCollection<PageViewModel> Pages => _pages;

        /// <summary>
        /// Выбранная страница. Область просмотра показывает то, что выбрано, а не то,
        /// что было выбрано раньше, поэтому смена выделения сразу обновляет команды.
        /// </summary>
        public PageViewModel SelectedPage
        {
            get => _selectedPage;
            set
            {
                if (SetProperty(ref _selectedPage, value))
                    RefreshCommands();
            }
        }

        /// <summary>
        /// Открыт ли список форматов сохранения. Список живёт рядом с кнопкой
        /// «Сохранить» и открывается под ней же: список от левого края панели
        /// уезжал в сторону и тянулся до кнопки искать.
        /// </summary>
        public bool IsSaveMenuOpen
        {
            get => _isSaveMenuOpen;
            set => SetProperty(ref _isSaveMenuOpen, value);
        }

        /// <summary>Идёт ли операция: команды на это время заблокированы.</summary>
        public bool IsBusy => _busy;

        /// <summary>Показана ли плашка с ходом работы или результатом.</summary>
        public bool NoticeVisible
        {
            get => _noticeVisible;
            private set => SetProperty(ref _noticeVisible, value);
        }

        /// <summary>
        /// Плашка показывает результат, а не ход работы. Результат в акцентном цвете и
        /// убирается сам, ход работы остаётся, пока идёт операция.
        /// </summary>
        public bool NoticeIsResult
        {
            get => _noticeIsResult;
            private set => SetProperty(ref _noticeIsResult, value);
        }

        /// <summary>Текст плашки.</summary>
        public string NoticeText
        {
            get => _noticeText;
            private set => SetProperty(ref _noticeText, value);
        }

        /// <summary>Нужна ли кнопка «Отмена» у плашки.</summary>
        public bool CancelWanted
        {
            get => _cancelWanted;
            private set => SetProperty(ref _cancelWanted, value);
        }

        /// <summary>
        /// Все команды верхней панели одним списком: их доступность меняется
        /// одновременно и по одним и тем же причинам — идёт операция или нет,
        /// выбрана страница или нет.
        /// </summary>
        private readonly RelayCommand[] _commands;

        /// <summary>Создаёт новый документ.</summary>
        public RelayCommand NewDocumentCommand { get; }

        /// <summary>Открывает список форматов сохранения.</summary>
        public RelayCommand SaveCommand { get; }

        public RelayCommand SaveAsPdfCommand { get; }

        public RelayCommand SaveAsZipCommand { get; }

        public RelayCommand SaveAsJpgCommand { get; }

        public RelayCommand SaveAsTiffCommand { get; }

        public RelayCommand ScanMonoCommand { get; }

        public RelayCommand ScanColourCommand { get; }

        public RelayCommand SettingsCommand { get; }

        public RelayCommand DeletePageCommand { get; }

        public RelayCommand MovePageUpCommand { get; }

        public RelayCommand MovePageDownCommand { get; }

        public RelayCommand RotatePageCommand { get; }

        /// <summary>Отменяет текущую операцию, если она её допускает.</summary>
        public RelayCommand CancelNoticeCommand { get; }

        /// <summary>Режим последнего сканирования: он определяет кодировку страниц.</summary>
        private ScanMode CurrentScanMode { get; set; } = ScanMode.Greyscale;

        /// <summary>
        /// Подготовка после показа окна: чистка каталога прошлой сессии и запуск
        /// ImageMagick. Делается здесь, а не в конструкторе, чтобы окно успело
        /// показаться и не висело белым.
        /// </summary>
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
        }

        // --- Сканирование ----------------------------------------------------

        /// <summary>
        /// Забирает страницы со сканера и добавляет их в документ.
        /// </summary>
        /// <remarks>
        /// Каталог результатов очищается в самом запросе: драйвер пишет файлы с
        /// именами по порядку, и сканы прошлого раза иначе попали бы в новый
        /// документ. Порядок страниц задаётся временем создания файлов, поэтому
        /// композиция страниц идёт сразу после захвата, пока порядок ещё виден.
        /// </remarks>
        private async void ScanAsync(CaptureColour colour)
        {
            if (AppSettings.ScanSourceIndex == AppSettings.NotSelected)
            {
                UserDialogs.Error(Messages.ChooseSource);
                return;
            }

            var cancellation = new CancellationTokenSource();
            _scanCancellation = cancellation;

            SetBusy(true);
            ShowNotice(Messages.Scanning, result: false, cancel: true);

            try
            {
                var request = new ScanRequest
                {
                    SourceIndex = AppSettings.ScanSourceIndex,
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

                    ScannerService.Capture(request, cancellation.Token);

                    cancellation.Token.ThrowIfCancellationRequested();

                    return PageImage.ComposeDirectory(request, PageQuality);
                }, cancellation.Token);

                var pages = await Task.Run(() => BuildPages(request.Directory), cancellation.Token);

                AddPages(pages);
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

                if (NoticeVisible && !CancelWanted)
                    HideNotice();
            }
        }

        /// <summary>Собирает модели страниц по файлам, отсортированным по имени.</summary>
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
            foreach (PageViewModel page in pages)
                _pages.Add(page);

            if (_pages.Count > 0)
                SelectedPage = _pages[_pages.Count - 1];

            RefreshCommands();

            if (_pages.Count > 0)
                ShowNotice(Messages.PagesAdded(_pages.Count), result: true, cancel: false);
        }

        // --- Сохранение -------------------------------------------------------

        /// <summary>Открывает список форматов сохранения.</summary>
        private void BeginSave() => IsSaveMenuOpen = !IsSaveMenuOpen;

        private async void Save(string filter)
        {
            IsSaveMenuOpen = false;

            string path = SaveFilePicker.Ask(filter);

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

                // Время и размер в журнал: по одному «экспорт запущен» не понять,
                // появился файл или нет, а ожидание большого документа иначе
                // выглядит как зависание.
                Log.Info("Экспорт завершён: {0}, байт={1}, мс={2}", path, new FileInfo(path).Length, watch.ElapsedMilliseconds);

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
        }

        /// <summary>
        /// Поворачивает выбранную страницу на 180° и переписывает файл на диске.
        /// </summary>
        /// <remarks>
        /// Поворот меняет файл, а не только картинку на экране: страница уходит в
        /// документ такой, какой лежит на диске, и иначе после сохранения она была бы
        /// повёрнута не так, как показано.
        /// </remarks>
        private void RotatePage()
        {
            PageViewModel page = SelectedPage;

            if (page == null)
                return;

            try
            {
                PageImage.Rotate(page.FilePath, 180.0);
                page.Reload();
            }
            catch (Exception ex)
            {
                UserDialogs.Error(ex);
            }
        }

        private void ChooseSource()
        {
            var sources = ScannerService.GetSources();

            if (sources.Count == 0)
            {
                UserDialogs.Error(Messages.NoSources);
                return;
            }

            int chosen = SourcePicker.Ask(sources, AppSettings.ScanSourceIndex);

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
        }

        private void HideNotice()
        {
            NoticeVisible = false;
            CancelWanted = false;
        }

        /// <summary>
        /// Отменяет операцию, если она это допускает. Сохранение отменить нельзя:
        /// файл дописывается на месте, и прерывание оставило бы его обрывок.
        /// </summary>
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

        /// <summary>
        /// Обновляет доступность всех команд разом. Список команд короткий и
        /// общий, а отслеживать зависимости каждой отдельно — лишняя работа.
        /// </summary>
        private void RefreshCommands()
        {
            foreach (RelayCommand command in _commands)
                command.NotifyCanExecuteChanged();
        }
    }
}