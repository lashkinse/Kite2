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
    // Вопрос об обновлении при входе: последняя версия читается со страницы
    // releases/latest, скачивание и подмену файлов делает Kite2.Updater.
    public static class UpdateService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // Сети может не оказаться вовсе: не дождавшись, проверка откладывается
        // до следующего запуска.
        private static readonly TimeSpan NetworkWait = TimeSpan.FromSeconds(10);

        private const string ReleasesLatestUrl =
            "https://github.com/lashkinse/Kite2/releases/latest";

        private static readonly HttpClient Http = CreateHttp();

        // Хранилище одно на пользователя: архивы в Program Files хранить нельзя.
        private static string Storage =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Kite2",
                "Update");

        // (update == null, answered == true) — обновлений нет, вопрос не задаётся;
        // (null, false) — сеть не ответила, проверка повторится.
        public static async Task<(ReleaseCatalog.Entry Update, bool Answered)> Latest()
        {
            try
            {
                using (var wait = new CancellationTokenSource(NetworkWait))
                using (HttpResponseMessage response = await Http.GetAsync(
                    ReleasesLatestUrl, HttpCompletionOption.ResponseHeadersRead, wait.Token))
                {
                    Version version = ReleaseCatalog.ReadVersion(
                        response.RequestMessage.RequestUri?.ToString());

                    if (version == null)
                    {
                        Log.Warn("Страница выпуска не отдала версии: {0}",
                            response.RequestMessage.RequestUri);
                        return (null, false);
                    }

                    // Выпуск совпал с установленной версией — вопроса не будет.
                    Version current = Assembly.GetEntryAssembly().GetName().Version;

                    if (current >= version)
                        return (null, true);

                    Log.Info("Актуальный выпуск по странице: {0}", version);

                    return (new ReleaseCatalog.Entry(version, DownloadUrl(version)), true);
                }
            }
            catch (OperationCanceledException ex)
            {
                Log.Debug(ex, "Проверка обновлений прервана по времени");

                return (null, false);
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Полный сбой проверки обновлений");
                return (null, false);
            }
        }

        public static async Task Stage(ReleaseCatalog.Entry update, IProgress<int> progress)
        {
            string temporary = Path.Combine(Storage, "package.zip");
            string versionDirectory = Path.Combine(Storage, update.Version.ToString());

            Directory.CreateDirectory(Storage);

            // Остатки прошлых обновлений.
            foreach (string stale in Directory.GetDirectories(Storage))
                Directory.Delete(stale, true);

            if (File.Exists(temporary))
                File.Delete(temporary);

            await Download(update, temporary, progress);

            ZipFile.ExtractToDirectory(temporary, versionDirectory);

            File.Delete(temporary);
        }

        // Запущенная программа не может подменить свои файлы — запускает обновлятор,
        // который дождётся выхода и скопирует версию поверх установки.
        public static void Launch(Version version)
        {
            string programPath = Assembly.GetEntryAssembly().Location;
            string updaterSource = Path.Combine(Path.GetDirectoryName(programPath), "Kite2.Updater.exe");
            string target = Path.Combine(Storage, "Kite2.Updater.exe");

            File.Copy(updaterSource, target, true);

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

                        // Длины может не быть — процентов тогда не показать.
                        if (total > 0 && progress != null)
                            progress.Report((int)(done * 100 / total));
                    }
                }
            }
        }

        private static HttpClient CreateHttp()
        {
            // Windows 7 может иметь отключённый TLS 1.2 в системных настройках:
            // явное разрешение от них не зависит.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            var client = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip
            });

            // GitHub отвечает отказом любому запросу без имени программы в заголовке.
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Kite2");

            return client;
        }

        private static string Quote(string value) => "\"" + value + "\"";

        private static string DownloadUrl(Version version) =>
            "https://github.com/lashkinse/Kite2/releases/download/v" + version + "/Kite2-" + version + ".zip";
    }
}
