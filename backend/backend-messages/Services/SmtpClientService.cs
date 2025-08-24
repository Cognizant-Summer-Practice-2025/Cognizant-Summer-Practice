using BackendMessages.Models.Email;
using BackendMessages.Services.Abstractions;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Diagnostics;

namespace BackendMessages.Services
{
    public class SmtpClientService : ISmtpClientService
    {
        private readonly ILogger<SmtpClientService> _logger;
        private readonly IConfiguration _configuration;

        public SmtpClientService(ILogger<SmtpClientService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(EmailMessage emailMessage)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var mimeMessage = CreateMimeMessage(emailMessage);
                
                using var client = new SmtpClient();
                await ConfigureSmtpClientAsync(client);
                await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(true);
                
                stopwatch.Stop();
                _logger.LogInformation("Email sent successfully to {RecipientEmail} in {ElapsedMs}ms", 
                    emailMessage.RecipientEmail, stopwatch.ElapsedMilliseconds);
                
                return true;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed to send email to {RecipientEmail} after {ElapsedMs}ms", 
                    emailMessage.RecipientEmail, stopwatch.ElapsedMilliseconds);
                return false;
            }
        }

        private MimeMessage CreateMimeMessage(EmailMessage emailMessage)
        {
            var fromEmail = !string.IsNullOrEmpty(emailMessage.FromEmail) 
                ? emailMessage.FromEmail 
                : _configuration["Email:FromAddress"] ?? "noreply@goalkeeper.com";
            
            var fromName = !string.IsNullOrEmpty(emailMessage.FromName) 
                ? emailMessage.FromName 
                : _configuration["Email:FromName"] ?? "GoalKeeper Messages";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(emailMessage.RecipientName, emailMessage.RecipientEmail));
            message.Subject = emailMessage.Subject;
            
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = emailMessage.HtmlBody,
                TextBody = emailMessage.TextBody
            };
            message.Body = bodyBuilder.ToMessageBody();

            return message;
        }

        private async Task ConfigureSmtpClientAsync(SmtpClient client)
        {
            var host = _configuration["Email:SmtpHost"] ?? "localhost";
            var port = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var useSSL = bool.Parse(_configuration["Email:UseSSL"] ?? "true");
            var username = _configuration["Email:SmtpUsername"];
            var password = _configuration["Email:SmtpPassword"];
            var isGmail = host.Contains("gmail.com", StringComparison.OrdinalIgnoreCase);
            var hasCredentials = !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);

            client.Timeout = 60000; // 60 seconds
            client.ServerCertificateValidationCallback = (s, c, h, e) => true; // Accept all certificates for development
            
            // Gmail-specific connection handling
            var secureSocketOptions = isGmail || useSSL 
                ? SecureSocketOptions.StartTls 
                : SecureSocketOptions.None;
            
            try
            {
                await client.ConnectAsync(host, port, secureSocketOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to SMTP server {SmtpHost}:{SmtpPort}", host, port);
                throw;
            }
            
            if (hasCredentials)
            {
                try
                {
                    await client.AuthenticateAsync(username, password);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to authenticate with SMTP server using username: {Username}", username);
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("No SMTP authentication credentials provided - using anonymous connection");
            }
        }
    }
} 