using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Kite2.Core;
using NLog;

namespace Kite2.App.Services
{
    // Обновления раздаются архивами в выпусках репозитория: программа спрашивает
    // человека при входе, скачивает и распаковывает новую версию в хранилище, а
    // отдельный обновлятор подменяет файлы рядом с запущенной программой и запускает
    // её заново. Всё общение с сетью — из комплекта .NET Framework: сторонних
    // пакетов у обновления нет.
    public static class UpdateService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // Вход в программу ждать нельзя: сети может не оказаться вовсе. Проверка
        // десять секунд, а дальше обновление откладывается до следующего запуска.
        private static readonly TimeSpan NetworkWait = TimeSpan.FromSeconds(10);

        private static readonly HttpClient Http = CreateHttp();

        // Дата-хранилище одно на заядлую копию программы, не на каталог: архивы
        // больших вариантов хранить в Program Files нельзя.
        private static string Storage =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Kite2",
                "Update");

        public static async Task<ReleaseCatalog.Entry> Latest()
        {
            try
            {
                using (var wait = new CancellationTokenSource(NetworkWait))
                using (HttpResponseMessage response =
                    await Http.GetAsync(ReleaseCatalog.ReleasesUrl, HttpCompletionOption.ResponseContentRead, wait.Token))
                {
                    response.EnsureSuccessStatusCode();

                    ReleaseCatalog.Entry update = ReleaseCatalog.Latest(await response.Content.ReadAsStringAsync());

                    // Обновлением считается только то, что младше не запущено:
                    // выпуск совпал с установленной версией — вопроса не будет.
                    Version current = Assembly.GetEntryAssembly().GetName().Version;

                    return update != null && current < update.Version ? update : null;
                }
            }
            catch (OperationCanceledException ex)
            {
                Log.Debug(ex, "Проверка обновлений прервана по времени");

                return null;
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Проверка обновлений не удалась");

                return null;
            }
        }

        // Отмена здесь не нужна: человек сам попросил обновиться и видит плашку,
        // пока идёт скачивание.
        public static async Task Stage(ReleaseCatalog.Entry update, IProgress<int> progress)
        {
            string temporary = Path.Combine(Storage, "package.zip");
            string versionDirectory = Path.Combine(Storage, update.Version.ToString());

            Directory.CreateDirectory(Storage);

            // Папки в хранилище — это недораспакованные копии прошлых обновлений,
            // они ничего не значат и остаются только мусорить.
            foreach (string stale in Directory.GetDirectories(Storage))
                Directory.Delete(stale, true);

            if (File.Exists(temporary))
                File.Delete(temporary);

            await Download(update, temporary, progress);

            ZipFile.ExtractToDirectory(temporary, versionDirectory);

            // Архив габаритный держать смысла нет: копия уже распакована рядом.
            File.Delete(temporary);
        }

        // Подмена файлов возможна только снаружи — сама запущенная программа свои
        // файлы не перезапишет. Обновлятор поэтому запускается из хранилища, чтобы
        // его собственный файл не мешал копированию, и дожидается выхода программы.
        public static void Launch(Version version)
        {
            string programPath = Assembly.GetEntryAssembly().Location;
            string updaterSource = Path.Combine(Path.GetDirectoryName(programPath), "Kite2.Updater.exe");
            string target = Path.Combine(Storage, "Kite2.Updater.exe");

            File.Copy(updaterSource, target, true);

            // Перезапуск просит обновлятор, а не программа. Аргументов при запуске
            // не передаётся: тестовый режим до вопроса об обновлении не доходит.
            var startInfo = new ProcessStartInfo
            {
                FileName = target,
                Arguments = Quote(programPath) + " " + Quote(Path.Combine(Storage, version.ToString())) + " true",
                UseShellExecute = true,
                CreateNoWindow = true
            };

            Process.Start(startInfo);
        }

        private static async Task Download(ReleaseCatalog.Entry update, string temporary, IProgress<int> progress)
        {
            using (HttpResponseMessage response = await Http.GetAsync(update.Url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                long total = response.Content.Headers.ContentLength ?? -1;

                using (Stream source = await response.Content.ReadAsStreamAsync())
                using (Stream target = File.Create(temporary))
                {
                    var buffer = new byte[64 * 1024];

                    long done = 0;

                    int taken;
                    while ((taken = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await target.WriteAsync(buffer, 0, taken);

                        done += taken;

                        // Процентов нет, если адрес отдал файл без длины: тогда
                        // плашка остаётся просто с текстом «Выполняется загрузка».
                        if (total > 0 && progress != null)
                            progress.Report((int)(done * 100 / total));
                    }
                }
            }
        }

        private static HttpClient CreateHttp()
        {
            // Windows 7 может иметь отключённые новые шифры в системных настройках:
            // явно разрешённый TLS 1.2 не зависит от таких crooked настройек.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            return new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip
            });
        }

        private static string Quote(string value) => "\"" + value + "\"";
    }
}
