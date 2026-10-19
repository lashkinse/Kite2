using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Kite2.Update
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
                WaitForExit(args[0]);
                CopyDirectory(args[1], Path.GetDirectoryName(args[0]));

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

        private static void WaitForExit(string programPath)
        {
            for (int swing = 0; swing < ExitWait.TotalMilliseconds / 500 && IsRunning(programPath); swing++)
                Thread.Sleep(500);
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
                        // Не у всякого процесса модуль прочтёшь: системные закрыты,
                        // а до программы с её помощником очередь всегда доходит.
                    }
                }
            }
            catch (Exception)
            {
                // Список процессов иногда недоступен — копирование с повторами
                // само подождёт, если файл ещё занят.
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

                // Повторы тут не украшение, а необходимость: файл может быть занят
                // антивирусом долю секунды, и без повторов замена срывалась бы.
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
    }
}
