using BackendMessages.Services.Abstractions;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace BackendMessages.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> SendUnreadMessagesNotificationAsync(string recipientEmail, string recipientName, int unreadCount, List<string> senderNames)
        {
            try
            {
                var message = new MimeMessage();
                
                // From address
                var fromEmail = _configuration["Email:FromAddress"] ?? "noreply@messages.com";
                var fromName = _configuration["Email:FromName"] ?? "Messages Service";
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                
                // To address
                message.To.Add(new MailboxAddress(recipientName, recipientEmail));
                
                // Subject
                message.Subject = $"You have {unreadCount} unread message{(unreadCount > 1 ? "s" : "")}";
                
                // Body
                var bodyBuilder = new BodyBuilder();
                var senderNamesText = senderNames.Count > 0 ? string.Join(", ", senderNames) : "various users";
                
                bodyBuilder.HtmlBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <h2 style='color: #2c3e50;'>Hello {recipientName}!</h2>
                            <p>You have <strong>{unreadCount}</strong> unread message{(unreadCount > 1 ? "s" : "")} waiting for you.</p>
                            <p>Messages from: <strong>{senderNamesText}</strong></p>
                            <p>Don't miss out on important conversations. Log in to your account to read your messages.</p>
                            <div style='margin: 30px 0; text-align: center;'>
                                <a href='#' style='background-color: #3498db; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                                    Read Messages
                                </a>
                            </div>
                            <p style='color: #7f8c8d; font-size: 12px; margin-top: 30px;'>
                                This is an automated notification. You're receiving this because you have unread messages in your account.
                            </p>
                        </div>
                    </body>
                    </html>";
                
                bodyBuilder.TextBody = $@"
Hello {recipientName}!

You have {unreadCount} unread message{(unreadCount > 1 ? "s" : "")} waiting for you.
Messages from: {senderNamesText}

Don't miss out on important conversations. Log in to your account to read your messages.

This is an automated notification. You're receiving this because you have unread messages in your account.";
                
                message.Body = bodyBuilder.ToMessageBody();
                
                // Send email
                using var client = new SmtpClient();
                
                var smtpHost = _configuration["Email:SmtpHost"] ?? "localhost";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:SmtpUsername"];
                var smtpPassword = _configuration["Email:SmtpPassword"];
                var useSSL = bool.Parse(_configuration["Email:UseSSL"] ?? "true");
                
                await client.ConnectAsync(smtpHost, smtpPort, useSSL ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                
                if (!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
                {
                    await client.AuthenticateAsync(smtpUsername, smtpPassword);
                }
                
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                
                _logger.LogInformation("Successfully sent unread messages notification to {RecipientEmail} for {UnreadCount} messages", recipientEmail, unreadCount);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send unread messages notification to {RecipientEmail}", recipientEmail);
                return false;
            }
        }
    }
} 