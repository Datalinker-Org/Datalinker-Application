using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DataLinker.Web.Helpers
{
    public static class ToastrHelper
    {
        /// <summary>
        /// Generates the javascript required to show a toastr notification if one
        /// has been created.
        /// </summary>
        /// <param name="htmlHelper">HTML helper object to extend.</param>
        /// <returns></returns>
        public static MvcHtmlString ToastrNotifications(this HtmlHelper htmlHelper)
        {
            var notifyJS = new StringBuilder();

            if (htmlHelper.ViewContext.TempData.ContainsKey("ToastrNotification"))
            {
                var notification = htmlHelper.ViewContext.TempData["ToastrNotification"] as ToastrNotification;
                htmlHelper.ViewContext.TempData.Remove("ToastrNotification");
                if (notification.IsValid)
                {
                    notifyJS.AppendLine("<script type=\"text/javascript\">");
                    notifyJS.AppendLine("$(document).ready(function () { ");
                    notifyJS.AppendFormat("    toastr.{0}({1});", notification.ToastrFunction, notification.ToastrParameters);
                    notifyJS.AppendLine("});");
                    notifyJS.AppendLine("</script>");
                }
            }

            return new MvcHtmlString(notifyJS.ToString());
        }
    }

    /// <summary>
    /// Represents a single toastr notification to be displayed.
    /// </summary>
    public class ToastrNotification
    {
        /// <summary>
        /// Severity level for the notification.
        /// </summary>
        public enum NotificationLevel
        {
            INFO,
            WARNING,
            ERROR,
            SUCCESS
        }

        /// <summary>
        /// Gets the notification severity level.
        /// </summary>
        public NotificationLevel Level { get; set; }

        /// <summary>
        /// Gets the notification title, if any.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets the notification message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Flag to indicate if the notification has at least a message set.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Message);
            }
        }

        /// <summary>
        /// Gets the Toastr library function to call.
        /// </summary>
        public string ToastrFunction
        {
            get
            {
                switch (Level)
                {
                    case NotificationLevel.ERROR:
                        return "error";
                    case NotificationLevel.INFO:
                        return "info";
                    case NotificationLevel.SUCCESS:
                        return "success";
                    case NotificationLevel.WARNING:
                        return "warning";
                    default:
                        throw new ArgumentException($"Uncatered-for notification level: {Level}");
                }
            }
        }

        /// <summary>
        /// Gets the parameters for triggering the toastr library.
        /// </summary>
        public string ToastrParameters
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Title))
                {
                    return $"'{Message.Replace("'", "\\'").Trim()}'";
                }

                return $"'{Message.Replace("'", "\\'").Trim()}', '{Title.Replace("'", "\\'").Trim()}'";
            }
        }
    }

    public class Toastr
    {
        private Controller _controller;

        public Toastr(Controller controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// Display a notification on the UI.
        /// </summary>
        /// <param name="level">Notification severity level.</param>
        /// <param name="message">Notification body text.</param>
        /// <param name="title">Notification title text.</param>
        public void Notify(ToastrNotification.NotificationLevel level, string message, string title)
        {
            var notification = new ToastrNotification()
            {
                Level = level,
                Message = message,
                Title = title
            };
            _controller.TempData["ToastrNotification"] = notification;
        }

        /// <summary>
        /// Display a notification on the UI with an error severity level.
        /// </summary>
        /// <param name="message">Notification body text.</param>
        /// <param name="title">Notification title text.</param>
        public void Error(string message, string title = null)
        {
            Notify(ToastrNotification.NotificationLevel.ERROR, message, title);
        }

        /// <summary>
        /// Display a notification on the UI with a warning severity level.
        /// </summary>
        /// <param name="message">Notification body text.</param>
        /// <param name="title">Notification title text.</param>
        public void Warning(string message, string title = null)
        {
            Notify(ToastrNotification.NotificationLevel.WARNING, message, title);
        }

        /// <summary>
        /// Display a notification on the UI with an informational severity level.
        /// </summary>
        /// <param name="message">Notification body text.</param>
        /// <param name="title">Notification title text.</param>
        public void Info(string message, string title = null)
        {
            Notify(ToastrNotification.NotificationLevel.INFO, message, title);
        }

        /// <summary>
        /// Display a notification on the UI with a success severity level.
        /// </summary>
        /// <param name="message">Notification body text.</param>
        /// <param name="title">Notification title text.</param>
        public void Success(string message, string title = null)
        {
            Notify(ToastrNotification.NotificationLevel.SUCCESS, message, title);
        }
    }
}