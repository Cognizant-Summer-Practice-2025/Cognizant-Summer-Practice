using BackendMessages.Services.Abstractions;
using BackendMessages.Models;
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

        public async Task<bool> SendMessageReceivedNotificationAsync(Message message, SearchUser recipient, SearchUser sender)
        {
            if (message == null || recipient == null || sender == null)
            {
                _logger.LogWarning("Cannot send email notification: null parameters provided");
                return false;
            }

            if (string.IsNullOrEmpty(recipient.Email))
            {
                _logger.LogWarning("Cannot send email notification: recipient email is empty");
                return false;
            }

            _logger.LogInformation("Sending email notification for message {MessageId} to {RecipientEmail}", 
                message.Id, recipient.Email);
            
            var emailContent = CreateMessageNotificationContent(sender, recipient);
            return await SendEmailAsync(recipient.Email, recipient.FullName ?? recipient.Username, 
                $"New message from {sender.Username}", emailContent);
        }

        public async Task<bool> SendUnreadMessagesNotificationAsync(string recipientEmail, string recipientName, 
            int unreadCount, List<string> senderNames)
        {
            if (string.IsNullOrEmpty(recipientEmail))
            {
                _logger.LogWarning("Cannot send email notification: recipient email is empty");
                return false;
            }

            if (unreadCount <= 0)
            {
                _logger.LogWarning("Cannot send email notification: invalid unread count {UnreadCount}", unreadCount);
                return false;
            }

            _logger.LogInformation("Sending unread messages notification to {RecipientEmail} for {UnreadCount} messages", 
                recipientEmail, unreadCount);

            var emailContent = CreateUnreadMessagesContent(senderNames, unreadCount);
            var subject = $"You have {unreadCount} unread message{(unreadCount > 1 ? "s" : "")}";
            
            return await SendEmailAsync(recipientEmail, recipientName, subject, emailContent);
        }

        public async Task<bool> SendContactRequestNotificationAsync(SearchUser recipient, SearchUser sender)
        {
            if (recipient == null || sender == null)
            {
                _logger.LogWarning("Cannot send contact request notification: null parameters provided");
                return false;
            }

            if (string.IsNullOrEmpty(recipient.Email))
            {
                _logger.LogWarning("Cannot send contact request notification: recipient email is empty");
                return false;
            }

            _logger.LogInformation("Sending contact request notification from {SenderUsername} to {RecipientEmail}", 
                sender.Username, recipient.Email);

            var emailContent = CreateContactRequestContent(sender, recipient);
            var subject = $"{sender.Username} wants to contact you";
            
            return await SendEmailAsync(recipient.Email, recipient.FullName ?? recipient.Username, subject, emailContent);
        }

        private EmailContent CreateMessageNotificationContent(SearchUser sender, SearchUser recipient)
        {
            var htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 40px auto; background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); overflow: hidden;'>
                        <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center;'>
                            <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: 300; letter-spacing: 1px;'>📧 New Message</h1>
                        </div>
                        
                        <div style='padding: 40px 30px;'>
                            <div style='background-color: #f8f9ff; border-radius: 8px; padding: 25px; margin-bottom: 20px; border-left: 4px solid #667eea;'>
                                <div style='display: flex; align-items: center; margin-bottom: 15px;'>
                                    <div style='width: 40px; height: 40px; background-color: #667eea; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin-right: 15px;'>
                                        <span style='color: white; font-weight: bold; font-size: 16px;'>👤</span>
                                    </div>
                                    <div>
                                        <p style='margin: 0; color: #666; font-size: 14px; text-transform: uppercase; letter-spacing: 0.5px;'>FROM</p>
                                        <p style='margin: 5px 0 0 0; color: #333; font-size: 18px; font-weight: 600;'>{sender.Username}</p>
                                    </div>
                                </div>
                            </div>
                            
                            <div style='background-color: #fff5f5; border-radius: 8px; padding: 25px; border-left: 4px solid #ff6b6b;'>
                                <div style='display: flex; align-items: center;'>
                                    <div style='width: 40px; height: 40px; background-color: #ff6b6b; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin-right: 15px;'>
                                        <span style='color: white; font-weight: bold; font-size: 16px;'>💬</span>
                                    </div>
                                    <div>
                                        <p style='margin: 0; color: #666; font-size: 14px; text-transform: uppercase; letter-spacing: 0.5px;'>MESSAGES</p>
                                        <p style='margin: 5px 0 0 0; color: #333; font-size: 24px; font-weight: 700;'>1</p>
                                    </div>
                            </div>
                            </div>
                        </div>
                        
                        <div style='background-color: #f8f9fa; padding: 20px 30px; text-align: center; border-top: 1px solid #e9ecef;'>
                            <p style='margin: 0; color: #6c757d; font-size: 12px;'>Message notification • {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>
                        </div>
                    </div>
                </body>
                </html>";

            var textBody = $@"
📧 NEW MESSAGE NOTIFICATION

👤 FROM: {sender.Username}
💬 MESSAGES: 1

Sent: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC";

            return new EmailContent(htmlBody, textBody);
        }

        private EmailContent CreateContactRequestContent(SearchUser sender, SearchUser recipient)
        {
            var htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 40px auto; background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); overflow: hidden;'>
                        <div style='background: linear-gradient(135deg, #4CAF50 0%, #45a049 100%); padding: 30px; text-align: center;'>
                            <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: 300; letter-spacing: 1px;'>💬 Contact Request</h1>
                        </div>
                        
                        <div style='padding: 40px 30px;'>
                            <div style='text-align: center; margin-bottom: 30px;'>
                                <h2 style='color: #333; margin: 0 0 10px 0; font-size: 24px;'>{sender.Username} wants to contact you</h2>
                                <p style='color: #666; margin: 0; font-size: 16px;'>Someone new would like to start a conversation with you.</p>
                            </div>
                            
                            <div style='background-color: #f8f9ff; border-radius: 8px; padding: 25px; margin-bottom: 20px; border-left: 4px solid #4CAF50;'>
                                <div style='display: flex; align-items: center; margin-bottom: 15px;'>
                                    <div style='width: 50px; height: 50px; background-color: #4CAF50; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin-right: 20px;'>
                                        <span style='color: white; font-weight: bold; font-size: 20px;'>👤</span>
                                    </div>
                                    <div>
                                        <p style='margin: 0; color: #666; font-size: 14px; text-transform: uppercase; letter-spacing: 0.5px;'>FROM</p>
                                        <p style='margin: 5px 0 0 0; color: #333; font-size: 20px; font-weight: 600;'>{sender.Username}</p>
                                        {(!string.IsNullOrEmpty(sender.FullName) && sender.FullName != sender.Username ? $"<p style='margin: 2px 0 0 0; color: #666; font-size: 14px;'>{sender.FullName}</p>" : "")}
                                    </div>
                                </div>
                            </div>
                            
                            <div style='text-align: center; margin: 30px 0;'>
                                <p style='color: #666; font-size: 16px; line-height: 1.5;'>
                                    This person has started a new conversation with you. 
                                    Log in to your account to view their message and respond.
                                </p>
                            </div>
                        </div>
                        
                        <div style='background-color: #f8f9fa; padding: 20px 30px; text-align: center; border-top: 1px solid #e9ecef;'>
                            <p style='margin: 0; color: #6c757d; font-size: 12px;'>Contact request • {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>
                        </div>
                    </div>
                </body>
                </html>";

            var textBody = $@"
💬 CONTACT REQUEST

{sender.Username} wants to contact you

FROM: {sender.Username}{(!string.IsNullOrEmpty(sender.FullName) && sender.FullName != sender.Username ? $" ({sender.FullName})" : "")}

This person has started a new conversation with you. 
Log in to your account to view their message and respond.

Sent: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC";

            return new EmailContent(htmlBody, textBody);
        }

        private EmailContent CreateUnreadMessagesContent(List<string> senderNames, int unreadCount)
        {
            var senderNamesText = senderNames?.Count > 0 ? string.Join(", ", senderNames) : "various users";

            var htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                </head>
                <body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 40px auto; background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); overflow: hidden;'>
                        <div style='background: linear-gradient(135deg, #ff6b6b 0%, #ee5a24 100%); padding: 30px; text-align: center;'>
                            <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: 300; letter-spacing: 1px;'>📬 Unread Messages</h1>
                        </div>
                        
                        <div style='padding: 40px 30px;'>
                            <div style='background-color: #f8f9ff; border-radius: 8px; padding: 25px; margin-bottom: 20px; border-left: 4px solid #667eea;'>
                                <div style='display: flex; align-items: center; margin-bottom: 15px;'>
                                    <div style='width: 40px; height: 40px; background-color: #667eea; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin-right: 15px;'>
                                        <span style='color: white; font-weight: bold; font-size: 16px;'>👥</span>
                                    </div>
                                    <div style='flex: 1;'>
                                        <p style='margin: 0; color: #666; font-size: 14px; text-transform: uppercase; letter-spacing: 0.5px;'>FROM</p>
                                        <p style='margin: 5px 0 0 0; color: #333; font-size: 16px; font-weight: 600; word-wrap: break-word;'>{senderNamesText}</p>
                                    </div>
                                </div>
                            </div>
                            
                            <div style='background-color: #fff5f5; border-radius: 8px; padding: 25px; border-left: 4px solid #ff6b6b;'>
                                <div style='display: flex; align-items: center;'>
                                    <div style='width: 40px; height: 40px; background-color: #ff6b6b; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin-right: 15px;'>
                                        <span style='color: white; font-weight: bold; font-size: 16px;'>💬</span>
                                    </div>
                                    <div>
                                        <p style='margin: 0; color: #666; font-size: 14px; text-transform: uppercase; letter-spacing: 0.5px;'>UNREAD MESSAGES</p>
                                        <p style='margin: 5px 0 0 0; color: #333; font-size: 24px; font-weight: 700;'>{unreadCount}</p>
                                    </div>
                                </div>
                            </div>
                        </div>
                        
                        <div style='background-color: #f8f9fa; padding: 20px 30px; text-align: center; border-top: 1px solid #e9ecef;'>
                            <p style='margin: 0; color: #6c757d; font-size: 12px;'>Twice daily digest • {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>
                        </div>
                    </div>
                </body>
                </html>";

            var textBody = $@"
📬 UNREAD MESSAGES DIGEST

👥 FROM: {senderNamesText}
💬 UNREAD MESSAGES: {unreadCount}

Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC";

            return new EmailContent(htmlBody, textBody);
        }

        private async Task<bool> SendEmailAsync(string recipientEmail, string recipientName, string subject, EmailContent content)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var message = CreateMimeMessage(recipientEmail, recipientName, subject, content);
                var smtpConfig = GetSmtpConfiguration();

                using var client = new SmtpClient();
                await ConfigureSmtpClient(client, smtpConfig);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                
                stopwatch.Stop();
                _logger.LogInformation("Email sent successfully to {RecipientEmail} in {ElapsedMs}ms", 
                    recipientEmail, stopwatch.ElapsedMilliseconds);
                
                return true;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed to send email to {RecipientEmail} after {ElapsedMs}ms: {ErrorMessage}", 
                    recipientEmail, stopwatch.ElapsedMilliseconds, ex.Message);
                return false;
            }
        }

        private MimeMessage CreateMimeMessage(string recipientEmail, string recipientName, string subject, EmailContent content)
        {
                var message = new MimeMessage();
                
                // From address
                var fromEmail = _configuration["Email:FromAddress"] ?? "noreply@messages.com";
                var fromName = _configuration["Email:FromName"] ?? "Messages Service";
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                
                // To address
                message.To.Add(new MailboxAddress(recipientName, recipientEmail));
                
                // Subject
            message.Subject = subject;
                
                // Body
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = content.HtmlBody,
                TextBody = content.TextBody
            };
            message.Body = bodyBuilder.ToMessageBody();

            return message;
        }

        private SmtpConfiguration GetSmtpConfiguration()
        {
            return new SmtpConfiguration
            {
                Host = _configuration["Email:SmtpHost"] ?? "localhost",
                Port = int.Parse(_configuration["Email:SmtpPort"] ?? "587"),
                Username = _configuration["Email:SmtpUsername"],
                Password = _configuration["Email:SmtpPassword"],
                UseSSL = bool.Parse(_configuration["Email:UseSSL"] ?? "true")
            };
        }

        private async Task ConfigureSmtpClient(SmtpClient client, SmtpConfiguration config)
        {
                client.Timeout = 60000; // 60 seconds timeout
                client.ServerCertificateValidationCallback = (s, c, h, e) => true; // Accept all certificates for development
                
            _logger.LogInformation("Connecting to SMTP server {SmtpHost}:{SmtpPort} (SSL: {UseSSL})", 
                config.Host, config.Port, config.UseSSL);
                
                // Gmail-specific connection handling
            if (config.Host.Contains("gmail.com"))
                {
                await client.ConnectAsync(config.Host, config.Port, SecureSocketOptions.StartTls);
                }
                else
                {
                await client.ConnectAsync(config.Host, config.Port, 
                    config.UseSSL ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                }
                
            if (!string.IsNullOrEmpty(config.Username) && !string.IsNullOrEmpty(config.Password))
                {
                await client.AuthenticateAsync(config.Username, config.Password);
                    _logger.LogDebug("Successfully authenticated with SMTP server");
                }
                else
                {
                    _logger.LogDebug("No SMTP authentication credentials provided - using anonymous connection");
                }
        }

        private record EmailContent(string HtmlBody, string TextBody);

        private record SmtpConfiguration
        {
            public string Host { get; init; } = string.Empty;
            public int Port { get; init; }
            public string? Username { get; init; }
            public string? Password { get; init; }
            public bool UseSSL { get; init; }
        }
    }
} 