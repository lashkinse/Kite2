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
        private bool _noticeVisible;
        private bool _noticeIsResult;
        private bool _cancelWanted;
        private bool _selectionMode;
        private string _noticeText = string.Empty;
        private CancellationTokenSource _scanCancellation;

        public MainViewModel()
        {
            NewDocumentCommand = new RelayCommand(NewDocument, () => !IsBusy);
            SaveCommand = new RelayCommand(Save, () => !IsBusy && _pages.Count > 0);
            ScanMonoCommand = new RelayCommand(() => ScanAsync(CaptureColour.BlackAndWhite), () => !IsBusy);
            ScanColourCommand = new RelayCommand(() => ScanAsync(CaptureColour.Colour), () => !IsBusy);
            SettingsCommand = new RelayCommand(ChooseSource, () => !IsBusy);
            DeletePageCommand = new RelayCommand(DeletePage, () => !IsBusy && SelectedPage != null);
            MovePageUpCommand = new RelayCommand(() => MovePage(-1), () => !IsBusy && SelectedPage != null);
            MovePageDownCommand = new RelayCommand(() => MovePage(1), () => !IsBusy && SelectedPage != null);
            RotatePageCommand = new RelayCommand(RotatePage, () => !IsBusy && SelectedPage != null);
            CancelNoticeCommand = new RelayCommand(CancelOperation);
            SelectionModeCommand = new RelayCommand(ToggleSelectionMode, () => !IsBusy && SelectedPage != null);

            _commands = new[]
            {
                NewDocumentCommand, SaveCommand, ScanMonoCommand, ScanColourCommand, SettingsCommand, DeletePageCommand,
                MovePageUpCommand, MovePageDownCommand, RotatePageCommand, SelectionModeCommand
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

        /// <summary>
        /// Сохраняет документ. Формат выбирается в окне сохранения, отдельного
        /// списка под кнопкой нет.
        /// </summary>
        public RelayCommand SaveCommand { get; }

        public RelayCommand ScanMonoCommand { get; }

        public RelayCommand ScanColourCommand { get; }

        public RelayCommand SettingsCommand { get; }

        public RelayCommand DeletePageCommand { get; }

        public RelayCommand MovePageUpCommand { get; }

        public RelayCommand MovePageDownCommand { get; }

        public RelayCommand RotatePageCommand { get; }

        /// <summary>
        /// Включён ли режим выделения. В нём протягивание мышью отмечает участок
        /// страницы, обычно лист двигают по экрану.
        /// </summary>
        public bool IsSelectionMode
        {
            get => _selectionMode;
            private set => SetProperty(ref _selectionMode, value);
        }

        /// <summary>Включает и выключает режим выделения фрагмента.</summary>
        public RelayCommand SelectionModeCommand { get; }

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
            if (!FakeScanner.Enabled && AppSettings.ScanSourceIndex == AppSettings.NotSelected)
            {
                UserDialogs.Error(Messages.ChooseSource);
                return;
            }

            CurrentColour = colour;

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

        /// <summary>Режим цвета последней серии: он определяет кодировку страниц.</summary>
        private CaptureColour CurrentColour { get; set; } = CaptureColour.BlackAndWhite;

        /// <summary>
        /// Одна серия: захват и превращение результата в страницы документа.
        /// </summary>
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

        /// <summary>
        /// Спрашивает, сканировать ли ещё. В тестовом режиме вопрос не задаётся:
        /// там всё равно никто не сидит перед окном, а окно остановит и прогон, и
        /// автоматическую проверку.
        /// </summary>
        private bool AskToContinue()
        {
            if (FakeScanner.Enabled && !FakeScanner.AlwaysAsk)
            {
                Log.Debug("Вопрос о продолжении пропущен: тестовый режим");
                return false;
            }

            return ScanContinueWindow.Ask(Application.Current.MainWindow);
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

        /// <summary>
        /// Проставляет номера по нынешнему порядку страниц. Номера присваиваются не
        /// при создании, а здесь: иначе после удаления первой страницы колонка
        /// начинается с двойки, а после перестановки идёт 1, 3, 2.
        /// </summary>
        private void RenumberPages()
        {
            for (int i = 0; i < _pages.Count; i++)
                _pages[i].Number = i + 1;
        }

        // --- Сохранение -------------------------------------------------------

        /// <summary>Спрашивает имя файла и сохраняет документ в выбранном формате.</summary>
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

        /// <summary>
        /// Описание того, что лежит на диске после сохранения: имя файла и его размер.
        /// Для картинок в запрошенном имени может не быть ничего — вместо него
        /// появляются файлы с номерами страниц.
        /// </summary>
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

        /// <summary>
        /// Поворачивает выбранную страницу на 180° и переписывает файл на диске.
        /// </summary>
        /// <remarks>
        /// Поворот меняет файл, а не только картинку на экране: страница уходит в
        /// документ такой, какой лежит на диске, и иначе после сохранения она была бы
        /// повёрнута не так, как показано.
        /// </remarks>
        /// <summary>
        /// Включает и выключает режим выделения. В нём протягивание мышью
        /// отмечает участок страницы, а у рамки появляются ножницы.
        /// </summary>
        private void ToggleSelectionMode()
        {
            IsSelectionMode = !IsSelectionMode;

            Log.Info("Режим выделения: {0}", IsSelectionMode ? "включён" : "выключен");
        }

        /// <summary>
        /// Сохраняет выделенный участок страницы отдельным файлом PNG. Прямоугольник
        /// присылает область просмотра и задаёт его в пикселях картинки, а не на
        /// экране.
        /// </summary>
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
        /// <summary>
        /// Пересчитывает доступность команд. Список нужен для явного обновления,
        /// а общий запрос WPF — страховка: команда, забытая в списке, иначе осталась
        /// бы выключенной навсегда, и об её поломке узнаёшь только по кнопке.
        /// </summary>
        private void RefreshCommands()
        {
            foreach (RelayCommand command in _commands)
                command.NotifyCanExecuteChanged();

            CommandManager.InvalidateRequerySuggested();
        }
    }
}
