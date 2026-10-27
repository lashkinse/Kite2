using Kite2.Core;

namespace Kite2.Tests
{
    public static class ReleaseCatalogTests
    {
        [Test("Версия вычленяется из адреса выпуска")]
        public static void ReadsVersionFromLandingAddress()
        {
            Check.Equal(
                "1.2.3",
                ReleaseCatalog.ReadVersion("https://github.com/lashkinse/Kite2/releases/tag/v1.2.3").ToString(),
                "версия выпуска");
        }

        [Test("Приписки предверсии не ломают разбор")]
        public static void ReadsVersionWithSuffix()
        {
            Check.Equal("1.2", ReleaseCatalog.ReadVersion("/releases/tag/v1.2-rc1").ToString(), "версия выпуска");
        }

        [Test("Если адрес не содержит версии — ответ пустой")]
        public static void MissingVersionGivesNothing()
        {
            Check.True(ReleaseCatalog.ReadVersion("https://github.com/lashkinse/Kite2/releases/latest") == null,
                "адрес без версии");
            Check.True(ReleaseCatalog.ReadVersion(null) == null, "пустой адрес");
        }
    }
}
