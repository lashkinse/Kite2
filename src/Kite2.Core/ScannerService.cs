using System;
using System.Collections.Generic;
using System.Threading;

namespace Kite2.Core
{
    // Захват всегда идёт в фоновом потоке и всегда в отдельном процессе
    // помощника. Прерывание делается закрытием помощника из другого потока —
    // зависший драйвер сам не вернётся.
    public static class ScannerService
    {
        public const string RawExtension = ".bmp";

        public static IReadOnlyList<string> GetSources()
        {
            if (FakeScanner.Enabled)
                return FakeScanner.GetSources();

            return ScanHelperClient.GetSources();
        }

        public static string[] Capture(ScanRequest request, CancellationToken cancellationToken)
        {
            if (request.SourceIndex == AppSettings.NotSelected)
                throw new InvalidOperationException(Messages.ChooseSource);

            if (FakeScanner.Enabled)
            {
                FakeScanner.Capture(request, cancellationToken);
                return new string[0];
            }

            return ScanHelperClient.Capture(request, cancellationToken);
        }
    }
}
