using System;
using Kite2.Core;

namespace Kite2.Tests
{
    public static class UpdateTextTests
    {
        [Test("Вопрос об обновлении называет будущую версию")]
        public static void QuestionNamesVersion()
        {
            // Версии в выпусках могут быть и двухчастные: люди видят «1.2», а не «1.2.0.0».
            Check.Equal("Доступна версия 1.2. Обновить сейчас?", Messages.UpdateQuestion(new Version(1, 2)), "текст вопроса");
            Check.Equal("Доступна версия 1.2. Обновить сейчас?", Messages.UpdateQuestion(new Version(1, 2, 0, 0)), "тот же вопрос у полной версии");
        }
    }
}
