using System;
using System.IO;
using System.Reflection;

namespace Kite2.Core
{
    /// <summary>
    /// Рабочие каталоги приложения в профиле пользователя. Каталоги создаются при
    /// первом обращении.
    /// </summary>
    /// <remarks>
    /// Всё, что программа пишет, живёт в %AppData%, а не рядом с исполняемым файлом:
    /// каталог установки может быть доступен только для чтения.
    /// </remarks>
    public static class AppPaths
    {
        private static string _scan;
        private static string _magickNetTemp;
        private static string _logs;

        /// <summary>Каталог с исполняемым файлом и его ресурсами.</summary>
        public static string AppDirectory => AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Корневой каталог приложения в профиле пользователя, %AppData%\Kite2.
        /// </summary>
        /// <remarks>
        /// Имя берётся у сборки, с которой программа запущена, а не у ядра: ядро —
        /// библиотека, и её имя в адресе каталога пользователю ничего не скажет.
        /// </remarks>
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

        /// <summary>Каталог, куда складываются результаты сканирования.</summary>
        public static string Scan => Resolve(ref _scan, "scan");

        /// <summary>Каталог для временных файлов ImageMagick.</summary>
        public static string MagickNetTemp => Resolve(ref _magickNetTemp, "magicknet");

        /// <summary>Каталог с журналом работы программы.</summary>
        public static string Logs => Resolve(ref _logs, "logs");

        /// <summary>Файл пользовательских настроек.</summary>
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

        /// <summary>
        /// Удаляет содержимое каталога, не удаляя сам каталог: на него ссылается
        /// ImageMagick, и он должен существовать все время работы программы.
        /// </summary>
        public static void EmptyFolder(string path)
        {
            foreach (string file in Directory.GetFiles(path))
                File.Delete(file);

            foreach (string folder in Directory.GetDirectories(path))
                EmptyFolder(folder);
        }
    }
}