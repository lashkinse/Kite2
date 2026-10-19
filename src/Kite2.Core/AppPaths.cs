using System;
using System.IO;
using System.Reflection;

namespace Kite2.Core
{
    // Всё, что программа пишет, живёт в %AppData%, а не рядом с исполняемым файлом:
    // каталог установки может быть доступен только для чтения.
    public static class AppPaths
    {
        private static string _data;
        private static string _scan;
        private static string _pages;
        private static string _magickNetTemp;
        private static string _logs;

        public static string AppDirectory => AppDomain.CurrentDomain.BaseDirectory;

        public static string Data
        {
            get
            {
                if (string.IsNullOrEmpty(_data))
                    _data = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        Assembly.GetEntryAssembly()?.GetName().Name ?? "Kite2");

                EnsureFolder(_data);

                return _data;
            }
        }

        public static string Scan => Resolve(ref _scan, "scan");

        // Файлы страниц хранятся отдельно от каталога сканирования: тот очищается между сериями.
        public static string Pages => Resolve(ref _pages, "pages");

        public static string MagickNetTemp => Resolve(ref _magickNetTemp, "magick.net");

        public static string Logs => Resolve(ref _logs, "logs");

        public static string SettingsFile => Path.Combine(Data, "Настройки.ini");

        public static string LogFile => Path.Combine(Logs, "Kite2-" + DateTime.Now.ToString("yyyyMMdd") + ".log");

        // Содержимое стирается, но не сам каталог.
        public static void ClearFolder(string path)
        {
            foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
                File.Delete(file);
        }

        private static string Resolve(ref string saved, string name)
        {
            if (string.IsNullOrEmpty(saved))
                saved = Path.Combine(Data, name);

            EnsureFolder(saved);

            return saved;
        }

        private static void EnsureFolder(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
