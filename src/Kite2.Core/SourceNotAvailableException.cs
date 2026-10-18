using System;

namespace Kite2.Core
{
    // Отдельный тип оттого, что пользователю об этом нужно сказать прямо: общая
    // фраза «произошла ошибка» ни к чему его не побуждает, а здесь достаточно
    // предложить выбрать источник заново.
    public sealed class SourceNotAvailableException : Exception
    {
        public SourceNotAvailableException()
            : base(Messages.SourceUnavailable)
        {
        }
    }
}