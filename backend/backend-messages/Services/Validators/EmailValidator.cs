using BackendMessages.Models.Email;
using BackendMessages.Services.Abstractions;
using System.Text.RegularExpressions;

namespace BackendMessages.Services
{
    public class EmailValidator : IEmailValidator
    {
        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email);
        }

        public bool IsValidUnreadMessagesNotification(UnreadMessagesNotification notification)
        {
            return notification != null &&
                   !string.IsNullOrWhiteSpace(notification.RecipientEmail) &&
                   IsValidEmail(notification.RecipientEmail) &&
                   notification.UnreadCount > 0;
        }

        public bool IsValidContactRequestNotification(ContactRequestNotification notification)
        {
            return notification?.Recipient != null &&
                   notification.Sender != null &&
                   !string.IsNullOrWhiteSpace(notification.Recipient.Email) &&
                   IsValidEmail(notification.Recipient.Email) &&
                   !string.IsNullOrWhiteSpace(notification.Sender.Username);
        }
    }
} 