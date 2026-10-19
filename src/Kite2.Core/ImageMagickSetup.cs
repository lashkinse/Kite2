using System;
using ImageMagick;
using NLog;

namespace Kite2.Core
{
    // Инициализация ImageMagick выполняется однажды: повторный вызов сбрасывает
    // состояние библиотеки на середине работы.
    public static class ImageMagickSetup
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const ulong MemoryLimit = 268435456UL;

        private static bool _started;

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