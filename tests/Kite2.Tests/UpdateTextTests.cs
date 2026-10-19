using Kite2.Core;

namespace Kite2.Tests
{
    public static class UpdateTextTests
    {
        [Test("Вопрос об обновлении называет только сам факт обновления")]
        public static void QuestionStatesUpdateIsAvailable()
        {
            Check.Equal("Доступно обновление. Обновить сейчас?", Messages.UpdateQuestion, "текст вопроса");
        }
    }
}
