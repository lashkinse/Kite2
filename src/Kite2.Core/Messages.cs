using System;

namespace Kite2.Core
{
    public static class Messages
    {
        public const string Scanning = "Выполняется сканирование...\nПожалуйста, подождите";

        public const string Saving = "Выполняется сохранение файла...\nПожалуйста, подождите";

        public const string Saved = "Сохранено:";

        public const string FragmentSaved = "Фрагмент сохранён:";

        public const string NothingSelected = "Сначала выделите участок страницы";

        public static string PagesAdded(int count) => "Добавлено страниц: " + count;

        public const string ChooseSource = "Источник сканирования не выбран. Откройте «Параметры» и укажите источник.";

        public const string NoSources = "Источники сканирования не найдены. Проверьте, подключён ли сканер и установлен ли его драйвер.";

        public const string SourceUnavailable = "Ранее выбранный источник больше недоступен. Выберите источник заново.";

        public const string ScanHelperMissing = "Не найдена программа захвата Kite2.Scan.exe рядом с этой программой.";

        public const string Cancel = "Отмена";

        // Обновление раздаётся выпусками репозитория, и вопрос задаётся при входе:
        // человеку достаточно самого факта обновления, номер он увидит после установки.
        public const string UpdateQuestion = "Доступно обновление. Обновить сейчас?";

        public const string Updating = "Выполняется загрузка обновления...";

        public static string UpdatingProgress(int percent) =>
            Updating + " " + percent + "%";

        public const string UpdateFailed =
            "Обновление установить не удалось. Новую версию можно скачать вручную: https://github.com/lashkinse/Kite2/releases";
    }
}