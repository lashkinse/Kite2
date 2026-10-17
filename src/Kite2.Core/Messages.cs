namespace Kite2.Core
{
    /// <summary>
    /// Тексты, с которыми общается ядро. Интерфейс задаёт свой набор отдельно:
    /// ядро сообщает о ситуациях, которых в нём самой нет.
    /// </summary>
    public static class Messages
    {
        public const string Scanning = "Выполняется сканирование...\nПожалуйста, подождите";

        public const string Saving = "Выполняется сохранение файла...\nПожалуйста, подождите";

        public const string Saved = "Сохранено:";

        /// <summary>Сообщение о добавленных страницах.</summary>
        public static string PagesAdded(int count) => "Добавлено страниц: " + count;

        public const string ChooseSource = "Источник сканирования не выбран. Откройте «Параметры» и укажите источник.";

        public const string NoSources = "Источники сканирования не найдены. Проверьте, подключён ли сканер и установлен ли его драйвер.";

        public const string SourceUnavailable = "Ранее выбранный источник больше недоступен. Выберите источник заново.";

        public const string Cancel = "Отмена";
    }
}