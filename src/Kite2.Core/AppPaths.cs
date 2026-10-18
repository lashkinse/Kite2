using System;
using System.IO;
using System.Reflection;

namespace Kite2.Core
{
    // Всё, что программа пишет, живёт в %AppData%, а не рядом с исполняемым файлом:
    // каталог установки может быть доступен только для чтения.
    public static class AppPaths
    {
        private static string _scan;
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

        private static string _data;

        public static string Scan => Resolve(ref _scan, "scan");

        public static string MagickNetTemp => Resolve(ref _magickNetTemp, "magicknet");

        public static string Logs => Resolve(ref _logs, "logs");

        public static string SettingsFile => Path.Combine(Data, "settings.ini");

        private static string Resolve(ref string cached, string folderName)
        {
            if (string.IsNullOrEmpty(cached))
                cached = Path.Combine(Data, folderName);

            EnsureFolder(cached);

            return cached;
        }

        private static void EnsureFolder(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        // Каталог не удаляется: на него ссылается ImageMagick, и он должен
        // существовать всё время работы программы.
        public static void EmptyFolder(string path)
        {
            foreach (string file in Directory.GetFiles(path))
                File.Delete(file);

            foreach (string folder in Directory.GetDirectories(path))
                EmptyFolder(folder);
        }
    }
}