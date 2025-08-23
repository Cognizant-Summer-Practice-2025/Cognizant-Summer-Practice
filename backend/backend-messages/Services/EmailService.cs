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
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    </head>
                    <body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
                        <div style='max-width: 600px; margin: 40px auto; background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); overflow: hidden;'>
                            <!-- Header -->
                            <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center;'>
                                <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: 300; letter-spacing: 1px;'>📧 New Message</h1>
                            </div>
                            
                            <!-- Content -->
                            <div style='padding: 40px 30px;'>
                                <div style='background-color: #f8f9ff; border-radius: 8px; padding: 25px; margin-bottom: 20px; border-left: 4px solid #667eea;'>
                                    <div style='display: flex; align-items: center; margin-bottom: 15px;'>
                                        <div style='width: 40px; height: 40px; background-color: #667eea; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin-right: 15px;'>
                                            <span style='color: white; font-weight: bold; font-size: 16px;'>👤</span>
                                        </div>
                                        <div>
                                            <p style='margin: 0; color: #666; font-size: 14px; text-transform: uppercase; letter-spacing: 0.5px;'>FROM</p>
                                            <p style='margin: 5px 0 0 0; color: #333; font-size: 18px; font-weight: 600;'>{sender.FullName}</p>
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
                            
                            <!-- Footer -->
                            <div style='background-color: #f8f9fa; padding: 20px 30px; text-align: center; border-top: 1px solid #e9ecef;'>
                                <p style='margin: 0; color: #6c757d; font-size: 12px;'>Message notification • {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                bodyBuilder.TextBody = $@"
📧 NEW MESSAGE NOTIFICATION

👤 FROM: {sender.FullName}
💬 MESSAGES: 1

Sent: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC";
                
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
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <meta charset='utf-8'>
                        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    </head>
                    <body style='margin: 0; padding: 0; background-color: #f4f4f4; font-family: Arial, sans-serif;'>
                        <div style='max-width: 600px; margin: 40px auto; background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); overflow: hidden;'>
                            <!-- Header -->
                            <div style='background: linear-gradient(135deg, #ff6b6b 0%, #ee5a24 100%); padding: 30px; text-align: center;'>
                                <h1 style='color: #ffffff; margin: 0; font-size: 28px; font-weight: 300; letter-spacing: 1px;'>📬 Unread Messages</h1>
                            </div>
                            
                            <!-- Content -->
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
                            
                            <!-- Footer -->
                            <div style='background-color: #f8f9fa; padding: 20px 30px; text-align: center; border-top: 1px solid #e9ecef;'>
                                <p style='margin: 0; color: #6c757d; font-size: 12px;'>Daily digest • {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC</p>
                            </div>
                        </div>
                    </body>
                    </html>";
                
                bodyBuilder.TextBody = $@"
📬 UNREAD MESSAGES DIGEST

👥 FROM: {senderNamesText}
💬 UNREAD MESSAGES: {unreadCount}

Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC";
                
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