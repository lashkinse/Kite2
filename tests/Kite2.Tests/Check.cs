using System;
using System.IO;

namespace Kite2.Tests
{
    // Намеренно без Xunit и прочих библиотек: их пакеты тянут за собой десятки
    // мегабайт, а проверки здесь простые и запускаются одной программой.
    public static class Check
    {
        public static void True(bool condition, string what)
        {
            if (!condition)
                throw new CheckFailedException("ожидалось истина: " + what);
        }

        public static void False(bool condition, string what)
        {
            if (condition)
                throw new CheckFailedException("ожидалось ложь: " + what);
        }

        public static void Equal<T>(T expected, T actual, string what)
        {
            if (!Equals(expected, actual))
                throw new CheckFailedException($"{what}: ожидалось <{expected}>, получено <{actual}>");
        }

        public static void NotEmpty(string value, string what)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new CheckFailedException($"{what}: значение пустое");
        }

        public static void FileExists(string path, string what)
        {
            if (!File.Exists(path))
                throw new CheckFailedException($"{what}: нет файла {path}");
        }

        public static void Throws<TException>(Action action, string what) where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
                return;
            }
            catch (Exception error)
            {
                throw new CheckFailedException(
                    $"{what}: ожидалось {typeof(TException).Name}, получено {error.GetType().Name}: {error.Message}");
            }

            throw new CheckFailedException($"{what}: ожидалось {typeof(TException).Name}, исключения не было");
        }
    }

    public sealed class CheckFailedException : Exception
    {
        public CheckFailedException(string message) : base(message)
        {
        }
    }
}
