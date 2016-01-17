using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Waterwark.Utilities
{
    static class CustomMessageBox
    {

        /// <summary>
        /// A message box for user Notifications/Information
        /// </summary>
        /// <param name="notificationMessage"></param>
        public static void Notification(string notificationMessage)
        {
            MessageBox.Show(notificationMessage, "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Error message box.
        /// If an exception is supplied as well, show the exception in a separate box.
        /// 
        /// In the future, maybe make the exception log to log file instead of notifying the user of a stack
        /// trace, because they have no use/need to see a stack trace
        /// </summary>
        /// <param name="errorMessage"></param>
        public static void Error(string errorMessage, Exception ex = null)
        {
            //string exceptionMessage = ex.Message ?? "";

            MessageBox.Show(errorMessage, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);

            //If an optional exception object is supplied - show it in a new MessageBox
            if (ex != null)
            {
                MessageBox.Show(ex.ToString(), "Exception - Stack Trace", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        /// <summary>
        /// Shows a messagebox window to get a Yes or No MessageBoxResult answer from a user.
        /// </summary>
        /// <returns>
        /// MessageBoxResult of Yes or No
        /// </returns>
        public static MessageBoxResult GetUserYesNoChoice(string promptMessage, string captionText = "Confirmation")
        {
            return MessageBox.Show(promptMessage, captionText, MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

    }
}
