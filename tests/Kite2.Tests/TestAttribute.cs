using System;

namespace Kite2.Tests
{
    /// <summary>
    /// Помечает метод, который проверяет одну вещь.
    /// </summary>
    /// <remarks>
    /// Свой тестовый вид без внешней библиотеки: проверки запускаются простой
    /// программой, их видно целиком в двух файлах, и в сборку приложения они не
    /// попадают.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TestAttribute : Attribute
    {
        public TestAttribute(string description)
        {
            Description = description;
        }

        public string Description { get; }
    }

    /// <summary>Метод проверки ничего не возвращает: всё сказано в описании.</summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TestCaseAttribute : Attribute
    {
    }
}