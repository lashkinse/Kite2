using System;
using System.IO;

namespace Kite2.Tests
{
    /// <summary>
    /// Временный каталог на время проверки.
    /// </summary>
    /// <remarks>
    /// Папка удаляется сама, даже если проверка упала: иначе после неудачного
    /// запуска в %TEMP% остаются десятки папок с недосканированными страницами.
    /// </remarks>
    public sealed class TemporaryFolder : IDisposable
    {
        public TemporaryFolder()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "kite2-test-" + Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(Path);
        }

        /// <summary>Путь к каталогу.</summary>
        public string Path { get; }

        /// <summary>Путь к файлу внутри каталога.</summary>
        public string File(string name) => System.IO.Path.Combine(Path, name);

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Path))
                    Directory.Delete(Path, true);
            }
            catch (IOException)
            {
                // Файлы заняты: временную папку уберёт очистка системы.
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}