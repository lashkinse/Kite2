using System;
using ImageMagick;
using NLog;

namespace Kite2.Core
{
    /// <summary>
    /// Запуск ImageMagick. Вызывается один раз, до первой работы с изображениями.
    /// </summary>
    public static class ImageMagickSetup
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>Предел памяти ImageMagick — 256 МБ.</summary>
        private const ulong MemoryLimit = 268435456UL;

        private static bool _started;

        /// <summary>
        /// Готовит ImageMagick к работе: временный каталог и предел памяти.
        /// </summary>
        /// <remarks>
        /// Повторный вызов ничего не делает. Инициализация не идемпотентна и при втором
        /// вызове сбрасывает состояние библиотеки на середине работы программы.
        /// </remarks>
        public static void EnsureStarted()
        {
            if (_started)
                return;

            try
            {
                MagickNET.SetTempDirectory(AppPaths.MagickNetTemp);
                ResourceLimits.Memory = MemoryLimit;
                _started = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Не удалось инициализировать ImageMagick");
                throw;
            }
        }
    }
}