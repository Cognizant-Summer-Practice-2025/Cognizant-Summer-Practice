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
            _logger.LogInformation("Starting real-time email notification for message {MessageId} to {RecipientEmail}", 
                message.Id, recipient.Email);
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                var mimeMessage = new MimeMessage();
                
                // From address
                var fromEmail = _configuration["Email:FromAddress"] ?? "noreply@messages.com";
                var fromName = _configuration["Email:FromName"] ?? "Messages Service";
                mimeMessage.From.Add(new MailboxAddress(fromName, fromEmail));
                
                // To address
                mimeMessage.To.Add(new MailboxAddress(recipient.FullName, recipient.Email));
                
                // Subject
                mimeMessage.Subject = $"New message from {sender.FullName}";
                
                // Body
                var bodyBuilder = new BodyBuilder();
                var messagePreview = message.Content?.Length > 100 
                    ? message.Content.Substring(0, 100) + "..." 
                    : message.Content;
                
                bodyBuilder.HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin-bottom: 20px;'>
                            <h2 style='color: #333; margin: 0 0 10px 0;'>New Message Received</h2>
                            <p style='color: #666; margin: 0;'>You have received a new message from <strong>{sender.FullName}</strong></p>
                        </div>
                        
                        <div style='background-color: #ffffff; border: 1px solid #e9ecef; border-radius: 8px; padding: 20px; margin-bottom: 20px;'>
                            <div style='margin-bottom: 15px;'>
                                <strong style='color: #333;'>From:</strong> {sender.FullName}
                                {(!string.IsNullOrEmpty(sender.ProfessionalTitle) ? $" ({sender.ProfessionalTitle})" : "")}
                            </div>
                            <div style='margin-bottom: 15px;'>
                                <strong style='color: #333;'>Message:</strong>
                            </div>
                            <div style='background-color: #f8f9fa; padding: 15px; border-radius: 4px; border-left: 4px solid #007bff;'>
                                <p style='margin: 0; color: #333; line-height: 1.5;'>{messagePreview}</p>
                            </div>
                        </div>
                        
                        <div style='text-align: center; margin-bottom: 20px;'>
                            <a href='#' style='background-color: #007bff; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block;'>
                                View Message
                            </a>
                        </div>
                        
                        <div style='border-top: 1px solid #e9ecef; padding-top: 20px; text-align: center; color: #666; font-size: 14px;'>
                            <p>This is an automated notification. You're receiving this because you have new messages in your account.</p>
                            <p>Message sent at: {message.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC</p>
                        </div>
                    </div>";

                bodyBuilder.TextBody = $@"
New Message Received

From: {sender.FullName}{(!string.IsNullOrEmpty(sender.ProfessionalTitle) ? $" ({sender.ProfessionalTitle})" : "")}

Message: {messagePreview}

Message sent at: {message.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC

This is an automated notification. You're receiving this because you have new messages in your account.";
                
                mimeMessage.Body = bodyBuilder.ToMessageBody();
                
                // Send email using the same logic as the daily notifications
                using var client = new SmtpClient();
                
                // Configure client for better Gmail compatibility
                client.Timeout = 60000; // 60 seconds timeout
                client.ServerCertificateValidationCallback = (s, c, h, e) => true; // Accept all certificates for development
                
                var smtpHost = _configuration["Email:SmtpHost"] ?? "localhost";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:SmtpUsername"];
                var smtpPassword = _configuration["Email:SmtpPassword"];
                var useSSL = bool.Parse(_configuration["Email:UseSSL"] ?? "true");
                
                _logger.LogInformation("Connecting to SMTP server {SmtpHost}:{SmtpPort} (SSL: {UseSSL})", smtpHost, smtpPort, useSSL);
                
                // Gmail-specific connection handling
                if (smtpHost.Contains("gmail.com"))
                {
                    // Gmail requires STARTTLS on port 587
                    await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                }
                else
                {
                    await client.ConnectAsync(smtpHost, smtpPort, useSSL ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                }
                _logger.LogDebug("Successfully connected to SMTP server");
                
                if (!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogDebug("Authenticating with SMTP server using username: {SmtpUsername}", smtpUsername);
                    await client.AuthenticateAsync(smtpUsername, smtpPassword);
                    _logger.LogDebug("Successfully authenticated with SMTP server");
                }
                else
                {
                    _logger.LogDebug("No SMTP authentication credentials provided - using anonymous connection");
                }
                
                var emailSentTime = DateTime.UtcNow;
                await client.SendAsync(mimeMessage);
                var emailDeliveredTime = DateTime.UtcNow;
                
                await client.DisconnectAsync(true);
                _logger.LogDebug("Disconnected from SMTP server");
                
                stopwatch.Stop();
                _logger.LogInformation("📧 REAL-TIME EMAIL SENT SUCCESSFULLY 📧");
                _logger.LogInformation("Recipient: {RecipientEmail}", recipient.Email);
                _logger.LogInformation("Message ID: {MessageId}", message.Id);
                _logger.LogInformation("Sender: {SenderName}", sender.FullName);
                _logger.LogInformation("Total process time: {TotalElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                
                return true;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed to send real-time message notification to {RecipientEmail} for message {MessageId} after {ElapsedMilliseconds}ms. Error: {ErrorMessage}", 
                    recipient.Email, message.Id, stopwatch.ElapsedMilliseconds, ex.Message);
                return false;
            }
        }

        public async Task<bool> SendUnreadMessagesNotificationAsync(string recipientEmail, string recipientName, int unreadCount, List<string> senderNames)
        {
            _logger.LogInformation("Starting email notification process for {RecipientEmail}", recipientEmail);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                _logger.LogDebug("Building email message for {RecipientName} with {UnreadCount} unread messages", recipientName, unreadCount);
                
                var message = new MimeMessage();
                
                // From address
                var fromEmail = _configuration["Email:FromAddress"] ?? "noreply@messages.com";
                var fromName = _configuration["Email:FromName"] ?? "Messages Service";
                message.From.Add(new MailboxAddress(fromName, fromEmail));
                _logger.LogDebug("Email from: {FromName} <{FromEmail}>", fromName, fromEmail);
                
                // To address
                message.To.Add(new MailboxAddress(recipientName, recipientEmail));
                
                // Subject
                message.Subject = $"You have {unreadCount} unread message{(unreadCount > 1 ? "s" : "")}";
                _logger.LogDebug("Email subject: {Subject}", message.Subject);
                
                // Body
                var bodyBuilder = new BodyBuilder();
                var senderNamesText = senderNames.Count > 0 ? string.Join(", ", senderNames) : "various users";
                _logger.LogDebug("Senders in notification: {SenderNames}", senderNamesText);
                
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
                _logger.LogDebug("Email message body constructed successfully");
                
                // Send email
                using var client = new SmtpClient();
                
                // Configure client for better Gmail compatibility
                client.Timeout = 60000; // 60 seconds timeout
                client.ServerCertificateValidationCallback = (s, c, h, e) => true; // Accept all certificates for development
                
                var smtpHost = _configuration["Email:SmtpHost"] ?? "localhost";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["Email:SmtpUsername"];
                var smtpPassword = _configuration["Email:SmtpPassword"];
                var useSSL = bool.Parse(_configuration["Email:UseSSL"] ?? "true");
                
                _logger.LogInformation("Connecting to SMTP server {SmtpHost}:{SmtpPort} (SSL: {UseSSL})", smtpHost, smtpPort, useSSL);
                
                // Gmail-specific connection handling
                if (smtpHost.Contains("gmail.com"))
                {
                    // Gmail requires STARTTLS on port 587
                    await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                }
                else
                {
                    await client.ConnectAsync(smtpHost, smtpPort, useSSL ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
                }
                _logger.LogDebug("Successfully connected to SMTP server");
                
                if (!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogDebug("Authenticating with SMTP server using username: {SmtpUsername}", smtpUsername);
                    await client.AuthenticateAsync(smtpUsername, smtpPassword);
                    _logger.LogDebug("Successfully authenticated with SMTP server");
                }
                else
                {
                    _logger.LogDebug("No SMTP authentication credentials provided - using anonymous connection");
                }
                
                _logger.LogInformation("Sending email to {RecipientEmail}...", recipientEmail);
                var emailSentTime = DateTime.UtcNow;
                await client.SendAsync(message);
                var emailDeliveredTime = DateTime.UtcNow;
                
                await client.DisconnectAsync(true);
                _logger.LogDebug("Disconnected from SMTP server");
                
                stopwatch.Stop();
                _logger.LogInformation("📧 EMAIL SENT SUCCESSFULLY 📧");
                _logger.LogInformation("Recipient: {RecipientEmail}", recipientEmail);
                _logger.LogInformation("Email sent at: {EmailSentTime} UTC", emailSentTime);
                _logger.LogInformation("Email delivered at: {EmailDeliveredTime} UTC", emailDeliveredTime);
                _logger.LogInformation("Send duration: {SendDurationMs}ms", (emailDeliveredTime - emailSentTime).TotalMilliseconds);
                _logger.LogInformation("Total process time: {TotalElapsedMs}ms", stopwatch.ElapsedMilliseconds);
                _logger.LogInformation("Unread messages count: {UnreadCount}", unreadCount);
                return true;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed to send unread messages notification to {RecipientEmail} after {ElapsedMilliseconds}ms. Error: {ErrorMessage}", 
                    recipientEmail, stopwatch.ElapsedMilliseconds, ex.Message);
                return false;
            }
        }
    }
} 