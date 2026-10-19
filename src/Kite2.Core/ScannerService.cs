using System;
using System.Collections.Generic;
using System.Threading;

namespace Kite2.Core
{
    // Захват всегда идёт в фоновом потоке, а прерывание делается закрытием помощника
    // из другого потока: TWAIN-драйвер умеет не возвращаться вообще, и ждать его
    // с таймаутом — значит прервать захват нельзя. Пользователь нажимает «Отмена»,
    // помощник закрывается, и ожидание заканчивается.
    public static class ScannerService
    {
        public const string RawExtension = ".bmp";

        // Отсутствие сканера — не ошибка, а отсутствие устройств: драйвер
        // недоступен, значит возвращается пустой список.
        public static IReadOnlyList<string> GetSources()
        {
            // В тестовом режиме драйвер не открывается вовсе: источник один и он
            // выдуманный, а настоящий сканер под рукой может и не оказаться.
            if (FakeScanner.Enabled)
                return FakeScanner.GetSources();

            return ScanHelperClient.GetSources();
        }

        // Порядок устройств после переподключения меняется, поэтому сохранённый в
        // настройках индекс может перестать соответствовать реальному устройству.
        public static bool IsSourceAvailable(int sourceIndex)
        {
            return sourceIndex >= 0 && sourceIndex < GetSources().Count;
        }

        public static string[] Capture(ScanRequest request, CancellationToken cancellationToken)
        {
            if (request.SourceIndex == AppSettings.NotSelected)
                throw new InvalidOperationException(Messages.ChooseSource);

            // В тестовом режиме страницы рисует программа: драйвер не запускается,
            // иначе проверка пути от захвата до просмотра требовала бы сканера.
            if (FakeScanner.Enabled)
            {
                FakeScanner.Capture(request, cancellationToken);
                return new string[0];
            }

            return ScanHelperClient.Capture(request, cancellationToken);
        }
    }
}
