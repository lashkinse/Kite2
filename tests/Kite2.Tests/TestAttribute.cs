using System;

namespace Kite2.Tests
{
    // Свой тестовый вид без внешней библиотеки: проверки запускаются простой
    // программой, их видно целиком в двух файлах, и в сборку приложения они не
    // попадают.
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TestAttribute : Attribute
    {
        public TestAttribute(string description)
        {
            Description = description;
        }

        public string Description { get; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TestCaseAttribute : Attribute
    {
    }
}
