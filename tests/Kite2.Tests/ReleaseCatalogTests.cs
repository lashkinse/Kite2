using Kite2.Core;

namespace Kite2.Tests
{
    public static class ReleaseCatalogTests
    {
        // Выпуски идут новыми вперёд, как их отдаёт GitHub: наверху все
        // неподходящие — отбор должен пробраться к последнему готовому.
        private const string Catalog =
            "[" +
            "  { \"name\": \"Kite2 2.0\", \"draft\": true, \"prerelease\": false," +
            "    \"assets\": [ { \"name\": \"Kite2-2.0.zip\", \"browser_download_url\": \"https://example.com/2.0\" } ] }," +
            "  { \"name\": \"Заготовка\", \"tag_name\": \"v1.2-rc\", \"draft\": false, \"prerelease\": true," +
            "    \"assets\": [ { \"name\": \"Kite2-1.2.zip\", \"browser_download_url\": \"https://example.com/1.2\" } ] }," +
            "  { \"name\": \"Kite2 1.1\", \"tag_name\": \"v1.1\", \"draft\": false, \"prerelease\": false," +
            "    \"assets\": [ { \"name\": \"README.md\", \"browser_download_url\": \"https://example.com/readme\" }," +
            "                  { \"name\": \"Kite2-1.1.zip\", \"browser_download_url\": \"https://example.com/Kite2-1.1.zip\" } ] }," +
            "  { \"name\": \"Kite2 1.0\", \"draft\": false, \"prerelease\": false, \"assets\": [] }" +
            "]";

        [Test("Из каталога берётся последний готовый выпуск с архивом")]
        public static void PicksLatestReadyRelease()
        {
            ReleaseCatalog.Entry latest = ReleaseCatalog.Latest(Catalog);

            Check.Equal("1.1", latest.Version.ToString(), "версия выпуска");
            Check.Equal("https://example.com/Kite2-1.1.zip", latest.Url, "адрес архива");
        }

        [Test("Название выпуска может быть пустым — версия живёт в метке")]
        public static void VersionFallsBackToTag()
        {
            string catalog =
                "[{ \"name\": null, \"tag_name\": \"v1.0\", \"draft\": false, \"prerelease\": false," +
                "   \"assets\": [ { \"name\": \"Kite2-1.0.zip\", \"browser_download_url\": \"https://example.com/old\" } ] }]";

            ReleaseCatalog.Entry latest = ReleaseCatalog.Latest(catalog);

            Check.Equal("1.0", latest.Version.ToString(), "версия выпуска");
            Check.Equal("https://example.com/old", latest.Url, "адрес архива");
        }

        [Test("Если в каталоге ничего пригодного — ответ пустой")]
        public static void EmptyCatalogGivesNothing()
        {
            // Только черновики и предверсии — от доверия к версии пользы нет.
            string catalog =
                "[{ \"name\": \"Kite2 3.0\", \"draft\": true, \"prerelease\": false, \"assets\": [] }]";

            Check.True(ReleaseCatalog.Latest(catalog) == null, "каталог без готовых выпусков");
            Check.True(ReleaseCatalog.Latest(null) == null, "пустой ответ тоже не выпуск");
        }
    }
}
