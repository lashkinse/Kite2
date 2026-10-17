using System;
using System.Globalization;
using System.IO;
using System.Text;
using NLog;

namespace Kite2.Core
{
    /// <summary>
    /// Пользовательские настройки в файле settings.ini рядом с рабочими каталогами.
    /// </summary>
    /// <remarks>
    /// Отдельный файл, а не user.config: каталог установки может быть недоступен для
    /// записи, а каталог AppData доступен всегда. Формат — обычный INI, чтобы
    /// пользователь мог поправить значение руками.
    /// </remarks>
    public static class AppSettings
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const string Section = "Scan";
        private const string KeySourceIndex = "ScanSourceIndex";

        /// <summary>Источник не выбран.</summary>
        public const int NotSelected = -1;

        /// <summary>
        /// Индекс TWAIN-источника в списке устройств; <see cref="NotSelected"/>, пока
        /// пользователь не выбрал источник в «Параметрах».
        /// </summary>
        public static int ScanSourceIndex { get; set; } = NotSelected;

        /// <summary>
        /// Читает настройки, а если файла нет — создаёт его со значениями по умолчанию,
        /// чтобы пользователь всегда мог найти его и поправить вручную.
        /// </summary>
        public static void Load()
        {
            if (!File.Exists(AppPaths.SettingsFile))
            {
                TrySave();
                return;
            }

            try
            {
                foreach (string line in File.ReadAllLines(AppPaths.SettingsFile))
                    Apply(line);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Log.Warn(ex, "Не удалось прочитать {0} — используются значения по умолчанию", AppPaths.SettingsFile);
            }
        }

        /// <summary>
        /// Сохраняет настройки. Ошибки записи не подавляются: молчаливая потеря
        /// выбранного источника хуже, чем сообщение о ней.
        /// </summary>
        public static void Save() => TrySave();

        private static void TrySave()
        {
            var content = new StringBuilder()
                .AppendLine("; Настройки Kite2. Файл создаётся программой автоматически.")
                .Append('[').Append(Section).AppendLine("]")
                .Append(KeySourceIndex).Append('=')
                .AppendLine(ScanSourceIndex.ToString(CultureInfo.InvariantCulture))
                .ToString();

            try
            {
                File.WriteAllText(AppPaths.SettingsFile, content, Encoding.UTF8);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Log.Warn(ex, "Не удалось создать {0}", AppPaths.SettingsFile);
            }
        }

        /// <summary>Разбирает строку вида «ключ=значение». Комментарии и секции пропускаются.</summary>
        public static void Apply(string line)
        {
            string text = line.Trim();

            if (text.Length == 0 || text[0] == ';' || text[0] == '#' || text[0] == '[')
                return;

            int separator = text.IndexOf('=');

            if (separator <= 0)
                return;

            string key = text.Substring(0, separator).Trim();
            string value = text.Substring(separator + 1).Trim();

            if (!string.Equals(key, KeySourceIndex, StringComparison.OrdinalIgnoreCase))
                return;

            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int index))
                ScanSourceIndex = index;
        }
    }
}