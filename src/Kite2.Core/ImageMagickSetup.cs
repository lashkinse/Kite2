using System;
using ImageMagick;
using NLog;

namespace Kite2.Core
{
    public static class ImageMagickSetup
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const ulong MemoryLimit = 268435456UL;

        private static bool _started;

        // Инициализация не идемпотентна: повторный вызов сбросил бы состояние
        // библиотеки на середине работы программы, поэтому она выполняется один раз.
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