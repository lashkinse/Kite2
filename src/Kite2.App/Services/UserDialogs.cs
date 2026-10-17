using System;
using System.Windows;
using Kite2.App.Views;
using Kite2.Core;
using NLog;

namespace Kite2.App.Services
{
    /// <summary>
    /// Окна сообщений и вопросов пользователю.
    /// </summary>
    /// <remarks>
    /// Отдельный класс оттого, что вызовы окон идут из моделей представления, а не из
    /// представлений: иначе в моделях появлялись бы ссылки на окна, и их нельзя было
    /// бы проверять без запуска программы. Здесь же пишется в журнал то, что видит
    /// пользователь, чтобы по журналу можно было понять, что происходило.
    /// </remarks>
    public static class UserDialogs
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>Титры окон: те же, что были в прежней версии программы.</summary>
        public const string ErrorTitle = "Ошибка";

        public const string NewDocumentTitle = "Создать документ";

        public const string SelectSourceTitle = "Источник сканирования";

        public const string ErrorGeneric = "Произошла ошибка. Подробности записаны в журнал.";

        public const string ErrorUnexpected = "Непредвиденный сбой. Подробности записаны в журнал.";

        public const string NewDocumentQuestion =
            "Изменения в текущем документе будут утеряны, если их не сохранить. Продолжить?";

        /// <summary>Вопрос с двумя ответами. true — согласие пользователя.</summary>
        public static bool Confirm(string title, string message, string yes = "Да", string no = "Нет")
        {
            Log.Info("Вопрос пользователю: {0}", message);

            return MessageWindow.Ask(OwnerWindow(), title, message, yes, no);
        }

        /// <summary>Сообщение об ошибке с понятным пользователю текстом.</summary>
        public static void Error(string message)
        {
            Log.Error(message);

            MessageWindow.Show(OwnerWindow(), ErrorTitle, message, MessageKind.Error);
        }

        /// <summary>
        /// Сообщение об ошибке из исключения. Пользователю показывается текст ошибки,
        /// если он есть, иначе — общая фраза: пустое окно с одним словом «Ошибка»
        /// пользователю ничего не объясняет.
        /// </summary>
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

        /// <summary>
        /// Главное окно программы: сообщение встаёт по центру его, а не экрана.
        /// </summary>
        private static Window OwnerWindow()
        {
            return Application.Current != null ? Application.Current.MainWindow : null;
        }
    }
}