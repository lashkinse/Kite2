using System;
using System.Windows;
using Kite2.App.Views;
using Kite2.Core;
using NLog;

namespace Kite2.App.Services
{
    // Окна вызываются из моделей представления, а не из представлений: иначе в моделях
    // появились бы ссылки на окна, и их нельзя было бы проверять без запуска программы.
    // Здесь же пишется в журнал то, что видит пользователь.
    public static class UserDialogs
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public const string ErrorTitle = "Ошибка";

        public const string NewDocumentTitle = "Создать документ";

        public const string SelectSourceTitle = "Источник сканирования";

        public const string ErrorGeneric = "Произошла ошибка. Подробности записаны в журнал.";

        public const string ErrorUnexpected = "Непредвиденный сбой. Подробности записаны в журнал.";

        public const string NewDocumentQuestion =
            "Изменения в текущем документе будут утеряны, если их не сохранить. Продолжить?";

        public static bool Confirm(string title, string message, string yes = "Да", string no = "Нет")
        {
            Log.Info("Вопрос пользователю: {0}", message);

            return MessageWindow.Ask(OwnerWindow(), title, message, yes, no);
        }

        public static void Error(string message)
        {
            Log.Error(message);

            MessageWindow.Show(OwnerWindow(), ErrorTitle, message, MessageKind.Error);
        }

        public static void Error(Exception exception)
        {
            if (exception == null)
            {
                Error(ErrorGeneric);
                return;
            }

            Log.Error(exception, "Необработанная ошибка");

            MessageWindow.Show(
                OwnerWindow(),
                ErrorTitle,
                string.IsNullOrWhiteSpace(exception.Message) ? ErrorUnexpected : exception.Message,
                MessageKind.Error);
        }

        private static Window OwnerWindow()
        {
            return Application.Current != null ? Application.Current.MainWindow : null;
        }
    }
}