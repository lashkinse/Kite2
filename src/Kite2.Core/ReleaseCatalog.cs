using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace Kite2.Core
{
    // Каталог обновлений — выпуски репозитория; черновики и предверсии
    // пропускаются: сравнение версий приписок не переносит.
    public static class ReleaseCatalog
    {
        public const string ReleasesUrl = "https://api.github.com/repos/lashkinse/Kite2/releases";

        // Имя архива меняется вместе с версией, поэтому шаблон со звёздочкой.
        private const string AssetPattern = "Kite2-*.zip";

        private static readonly Regex VersionPattern = new Regex(@"(\d+\.\d+(?:\.\d+)?(?:\.\d+)?)");

        private static readonly Regex AssetName = new Regex(BuildAssetPattern(AssetPattern));

        // Последний пригодный выпуск или null: ничего подходящего в каталоге нет.
        public static Entry Latest(string body)
        {
            IEnumerable<ReleaseJson> releases =
                body == null ? new ReleaseJson[0] : new JavaScriptSerializer().Deserialize<ReleaseJson[]>(body);

            Entry best = null;

            foreach (ReleaseJson release in releases ?? new ReleaseJson[0])
            {
                // Выпуски идут новыми вперёд, поэтому первый встреченный вариант
                // версии и есть её актуальная запись.
                Entry entry = ReadEntry(release);

                if (entry != null && (best == null || best.Version < entry.Version))
                    best = entry;
            }

            return best;
        }

        private static Entry ReadEntry(ReleaseJson release)
        {
            // Черновики человек не видит, а предверсии не пройдут сравнение версий.
            if (release == null || release.draft || release.prerelease)
                return null;

            string title = string.IsNullOrEmpty(release.name) ? release.tag_name : release.name;
            Version version = ReadVersion(title);

            if (version == null || release.assets == null)
                return null;

            AssetJson asset = release.assets.FirstOrDefault(named => named != null && AssetName.IsMatch(named.name ?? string.Empty));

            if (asset == null)
                return null;

            return new Entry(version, asset.browser_download_url);
        }

        // Публично: запасной путь проверки ищет версию в строке адреса выпуска.
        public static Version ReadVersion(string title)
        {
            Match match = VersionPattern.Match(title ?? string.Empty);

            return match.Success ? Version.Parse(match.Groups[1].Value) : null;
        }

        private static string BuildAssetPattern(string pattern)
        {
            string escaped = Regex.Escape(pattern)
                .Replace(@"\*", ".*")
                .Replace(@"\?", ".");

            return "^" + escaped + "$";
        }

        public sealed class Entry
        {
            public Entry(Version version, string url)
            {
                Version = version;
                Url = url;
            }

            public Version Version { get; }

            public string Url { get; }
        }

        private sealed class ReleaseJson
        {
            public string name { get; set; }

            public string tag_name { get; set; }

            public bool draft { get; set; }

            public bool prerelease { get; set; }

            public AssetJson[] assets { get; set; }
        }

        private sealed class AssetJson
        {
            public string name { get; set; }

            public string browser_download_url { get; set; }
        }
    }
}
