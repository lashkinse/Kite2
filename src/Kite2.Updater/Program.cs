using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Kite2.Updater
{
    // Запускается программой, не человеком:
    //   Kite2.Updater.exe <файл программы> <каталог новой версии> <перезапуск: true|false>
    // Консоли нет: чёрного окна при каждом обновлении быть не должно.
    internal static class Program
    {
        // Полминуты на выход достаточно: программа к этому моменту уже закрывается
        // сама, а зависший процесс — уже беда не обновления.
        private static readonly TimeSpan ExitWait = TimeSpan.FromSeconds(30);

        private static int Main(string[] args)
        {
            if (args.Length < 3)
                return 2;

            try
            {
                string targetRoot = Path.GetDirectoryName(args[0]);
                // Помощник может остаться в повисшем драйвере и после выхода
                // программы: файлы Kite2.Scan.exe он не отдаст, пока жив.
                WaitForProgramExit(args[0]);
                KillLingeringHelpers(targetRoot);
                CopyDirectory(args[1], targetRoot);
                CleanupStale(args[1], targetRoot);

                if (args[2] == "true")
                    StartAgain(args[0]);
            }
            catch (Exception)
            {
                // Обновлятор на экран ничего не выводит: если файлы подменились не
                // все, человек запустит программу старой версии и попробует снова.
                return 3;
            }

            return 0;
        }

        private static void WaitForProgramExit(string programPath)
        {
            for (int attempt = 0; attempt < 60 && IsRunning(programPath); attempt++)
                Thread.Sleep(500);
        }

        // Зависший в драйвере Kite2.Scan.exe не выйдет сам: после выхода программы
        // его гасим принудительно — установке данные непосредственно не принадлежат.
        private static void KillLingeringHelpers(string targetRoot)
        {
            foreach (Process process in Process.GetProcessesByName("Kite2.Scan"))
            {
                try
                {
                    string module = process.MainModule == null ? null : process.MainModule.FileName;

                    if (module != null && module.StartsWith(targetRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        process.Kill();
                        process.WaitForExit(5000);
                    }
                }
                catch (Exception)
                {
                    // Не осилил — и ладно: копирование повторами переживёт занятые файлы.
                }
            }
        }

        private static bool IsRunning(string programPath)
        {
            try
            {
                foreach (Process process in Process.GetProcesses())
                {
                    try
                    {
                        string other = process.MainModule == null ? null : process.MainModule.FileName;

                        if (other != null && string.Equals(other, programPath, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                    catch (Exception)
                    {
                        // Не у всякого процесса модуль прочтёшь — это не ошибка.
                    }
                }
            }
            catch (Exception)
            {
                // Список процессов иногда недоступен: копирование с повторами
                // само дождётся освобождения файлов.
            }

            return false;
        }

        // Каталог новой версии — полный слепок программы: копируется всё содержимое
        // целиком, каталогами и файлами, поверх установленного.
        private static void CopyDirectory(string sourceDirectory, string targetRoot)
        {
            foreach (string file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                string relative = file.Substring(sourceDirectory.Length).TrimStart(Path.DirectorySeparatorChar);
                string target = Path.Combine(targetRoot, relative);

                Directory.CreateDirectory(Path.GetDirectoryName(target));

                // Повторы нужны: файл может быть занят антивирусом долю секунды.
                for (int attempt = 1; ; attempt++)
                {
                    try
                    {
                        File.Copy(file, target, true);

                        break;
                    }
                    catch (IOException)
                    {
                        if (attempt >= 20)
                            throw;

                        Thread.Sleep(500);
                    }
                }
            }
        }

        private static void StartAgain(string programPath)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = programPath,
                UseShellExecute = true
            });
        }

        // Новая версия перечисляется целиком: всё, что осталось от прошедших
        // выпусков и не перечислено в пакете, демонтируется уже после копирования.
        // Иначе пустые папки (`_win32` из прошлой поставки) и снятые с поддержки
        // сборки копятся в каталоге годами.
        private static void CleanupStale(string sourceDirectory, string targetRoot)
        {
            var kept = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string file in Directory.GetFiles(sourceDirectory, "*", SearchOption.AllDirectories))
                kept.Add(TrimPrefix(file, sourceDirectory));

            foreach (string file in Directory.GetFiles(targetRoot, "*", SearchOption.AllDirectories))
            {
                string relative = TrimPrefix(file, targetRoot);

                if (kept.Contains(relative))
                    continue;

                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                    // Отказавшийся удаляться файл достанется следующему обновлению:
                    // нельзя дать одной застрявшей dll остановить всю установку.
                }
            }

            foreach (string directory in Directory.GetDirectories(targetRoot, "*", SearchOption.AllDirectories))
            {
                if (Directory.GetFileSystemEntries(directory).Length == 0)
                {
                    try
                    {
                        Directory.Delete(directory, false);
                    }
                    catch (Exception)
                    {
                        // Пустая папка не мешает ничему — просто исчезнет в другой раз.
                    }
                }
            }
        }

        private static string TrimPrefix(string path, string prefix)
        {
            string relative = path.Substring(prefix.Length).TrimStart(Path.DirectorySeparatorChar);
            return relative;
        }
    }
}
