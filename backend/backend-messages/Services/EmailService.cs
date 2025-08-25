using BackendMessages.Models;
using BackendMessages.Models.Email;
using BackendMessages.Services.Abstractions;

namespace BackendMessages.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IEmailValidator _emailValidator;
        private readonly IEmailTemplateEngine _templateEngine;
        private readonly ISmtpClientService _smtpClientService;
        private readonly IConfiguration _configuration;

        public EmailService(
            ILogger<EmailService> logger,
            IEmailValidator emailValidator,
            IEmailTemplateEngine templateEngine,
            ISmtpClientService smtpClientService,
            IConfiguration configuration)
        {
            _logger = logger;
            _emailValidator = emailValidator;
            _templateEngine = templateEngine;
            _smtpClientService = smtpClientService;
            _configuration = configuration;
        }

        public async Task<bool> SendUnreadMessagesNotificationAsync(UnreadMessagesNotification notification)
        {
            // Validate input
            if (!ValidateNotification(notification, _emailValidator.IsValidUnreadMessagesNotification))
            {
                return false;
            }

            try
            {
                var subject = $"You have {notification.UnreadCount} unread message{(notification.UnreadCount > 1 ? "s" : "")}";
                var (htmlBody, textBody) = _templateEngine.GenerateUnreadMessagesTemplate(notification);

                var emailMessage = CreateEmailMessage(
                    notification.RecipientEmail,
                    notification.RecipientName,
                    subject,
                    htmlBody,
                    textBody);

                return await _smtpClientService.SendEmailAsync(emailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send unread messages notification to {RecipientEmail}", 
                    notification.RecipientEmail);
                return false;
            }
        }

        public async Task<bool> SendContactRequestNotificationAsync(ContactRequestNotification notification)
        {
            // Validate input
            if (!ValidateNotification(notification, _emailValidator.IsValidContactRequestNotification))
            {
                return false;
            }

            try
            {
                var subject = $"{notification.Sender.Username} wants to contact you";
                var recipientName = notification.Recipient.FullName ?? notification.Recipient.Username;
                var (htmlBody, textBody) = _templateEngine.GenerateContactRequestTemplate(notification);

                var emailMessage = CreateEmailMessage(
                    notification.Recipient.Email,
                    recipientName,
                    subject,
                    htmlBody,
                    textBody);

                return await _smtpClientService.SendEmailAsync(emailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send contact request notification to {RecipientEmail}", 
                    notification.Recipient.Email);
                return false;
            }
        }

        private bool ValidateNotification<T>(T notification, Func<T, bool> validationFunc)
        {
            if (!validationFunc(notification))
            {
                _logger.LogWarning("Validation failed for {NotificationType} notification", typeof(T).Name);
                return false;
            }
            return true;
        }

        private EmailMessage CreateEmailMessage(
            string recipientEmail,
            string recipientName,
            string subject,
            string htmlBody,
            string textBody)
        {
            var fromEmail = _configuration["Email:FromAddress"] ?? "noreply@goalkeeper.com";
            var fromName = _configuration["Email:FromName"] ?? "GoalKeeper Messages";

            return new EmailMessage
            {
                RecipientEmail = recipientEmail,
                RecipientName = recipientName,
                Subject = subject,
                HtmlBody = htmlBody,
                TextBody = textBody,
                FromEmail = fromEmail,
                FromName = fromName
            };
        }


    }
} 