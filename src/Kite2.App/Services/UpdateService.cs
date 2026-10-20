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
    // человека при входе и распаковывает новую версию в хранилище, а отдельный
    // обновлятор подменяет файлы и запускает программу заново.
    public static class UpdateService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        // Вход в программу ждать нельзя: сети может не оказаться вовсе. Проверка
        // десять секунд, а дальше обновление откладывается до следующего запуска.
        private static readonly TimeSpan NetworkWait = TimeSpan.FromSeconds(10);

        // Запасной вход не ходит через API GitHub: на него лимитов почти нет.
        private const string ReleasesLatestUrl =
            "https://github.com/lashkinse/Kite2/releases/latest";

        private static readonly HttpClient Http = CreateHttp();

        // Дата-хранилище одно на пользователя, не на каталог установки:
        // архивы в Program Files хранить нельзя.
        private static string Storage =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Kite2",
                "Update");

        // Ответ проверки: само обновление и признак того, что каталог вообще
        // ответил. «Обновлений нет» повторов не просит, сеть без ответа — просит.
        public static async Task<(ReleaseCatalog.Entry Update, bool Answered)> Latest()
        {
            (ReleaseCatalog.Entry Update, bool Answered) byCatalog = await LatestByCatalog();

            // Каталог API GitHub ограничен числом запросов в час на компьютер;
            // исчерпанный лимит раньше оставлял проверку молча пустой до конца
            // сеанса. Прямая страница выпуска ходит через обычный сайт, на неё
            // лимиты API не давят, — на таком запасном входе проверка и живёт.
            if (!byCatalog.Answered)
                return await LatestByPage();

            return byCatalog;
        }

        private static async Task<(ReleaseCatalog.Entry Update, bool Answered)> LatestByCatalog()
        {
            try
            {
                // Граница общая для всех быстрых повторов: вход в программу
                // в худшем случае задерживается несильно.
                using (var wait = new CancellationTokenSource(NetworkWait))
                {
                    for (int attempt = 1; ; attempt++)
                    {
                        try
                        {
                            using (HttpResponseMessage response =
                                await Http.GetAsync(ReleaseCatalog.ReleasesUrl, wait.Token))
                            {
                                response.EnsureSuccessStatusCode();

                                ReleaseCatalog.Entry update = ReleaseCatalog.Latest(await response.Content.ReadAsStringAsync());

                                // Обновлением считается только то, что младше не запущено:
                                // выпуск совпал с установленной версией — вопроса не будет.
                                Version current = Assembly.GetEntryAssembly().GetName().Version;

                                if (update != null && current < update.Version)
                                    return (update, true);

                                return (null, true);
                            }
                        }
                        catch (HttpRequestException) when (attempt < 5)
                        {
                            // Быстрый повтор: пауза всегда умещается в общий срок.
                            await Task.Delay(TimeSpan.FromMilliseconds(500), wait.Token);
                        }
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                Log.Debug(ex, "Проверка обновлений прервана по времени");

                return (null, false);
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Проверка обновлений не удалась");

                return (null, false);
            }
        }

        // Отмена здесь не нужна: человек сам попросил обновиться и видит плашку,
        // пока идёт скачивание.
        public static async Task Stage(ReleaseCatalog.Entry update, IProgress<int> progress)
        {
            string temporary = Path.Combine(Storage, "package.zip");
            string versionDirectory = Path.Combine(Storage, update.Version.ToString());

            Directory.CreateDirectory(Storage);

            // Папки в хранилище — остатки прошлых обновлений.
            foreach (string stale in Directory.GetDirectories(Storage))
                Directory.Delete(stale, true);

            if (File.Exists(temporary))
                File.Delete(temporary);

            await Download(update, temporary, progress);

            ZipFile.ExtractToDirectory(temporary, versionDirectory);

            // Архив после распаковки не нужен.
            File.Delete(temporary);
        }

        // Подменить свои файлы запущенная программа не может — это делает
        // обновлятор, запущенный из хранилища (его собственный файл не мешает
        // копированию).
        public static void Launch(Version version)
        {
            string programPath = Assembly.GetEntryAssembly().Location;
            string updaterSource = Path.Combine(Path.GetDirectoryName(programPath), "Kite2.Updater.exe");
            string target = Path.Combine(Storage, "Kite2.Updater.exe");

            File.Copy(updaterSource, target, true);

            // Перезапуск просит обновлятор. Ключей запуска не передаётся:
            // тестовый режим до вопроса об обновлении не доходит.
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

                        // Процентов нет, если адрес отдал файл без длины.
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

        // Запасной вход: обычный сайт вместо API. Адрес обязательного выпуска
        // (releases/latest) уходит редиректом на страницу самого выпуска, а
        // версия видна прямо в адресе приземления. Архив складывается из версии:
        // имя актива всегда ходит в ногу с ней.
        private static async Task<(ReleaseCatalog.Entry Update, bool Answered)> LatestByPage()
        {
            try
            {
                using (HttpResponseMessage response = await Http.GetAsync(
                    ReleasesLatestUrl, HttpCompletionOption.ResponseHeadersRead))
                {
                    Version version = ReleaseCatalog.ReadVersion(
                        response.RequestMessage.RequestUri?.ToString());

                    if (version == null)
                    {
                        Log.Warn("Страница выпуска не отдала версии: {0}",
                            response.RequestMessage.RequestUri);
                        return (null, false);
                    }

                    Log.Info("Прямая страница выпуска: {0} (каталог API был недоступен)", version);

                    var update = new ReleaseCatalog.Entry(version, DownloadUrl(version));

                    return (update, true);
                }
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Полный сбой проверки обновлений");
                return (null, false);
            }
        }

        private static string DownloadUrl(Version version) =>
            "https://github.com/lashkinse/Kite2/releases/download/v" + version + "/Kite2-" + version + ".zip";
    }
}
