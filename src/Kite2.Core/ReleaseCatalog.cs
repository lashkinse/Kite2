using System;
using System.Text.RegularExpressions;

namespace Kite2.Core
{
    // Версия выпуска читается из адреса приземления: releases/latest
    // переадресует на страницу выпуска, содержащую tag/v<версия>. Обновление
    // проверяется только так — API каталога вытеснен из-за лимитов запросов.
    public static class ReleaseCatalog
    {
        private static readonly Regex VersionPattern = new Regex(@"(\d+\.\d+(?:\.\d+)?(?:\.\d+)?)");

        public static Version ReadVersion(string title)
        {
            Match match = VersionPattern.Match(title ?? string.Empty);

            return match.Success ? Version.Parse(match.Groups[1].Value) : null;
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
    }
}
