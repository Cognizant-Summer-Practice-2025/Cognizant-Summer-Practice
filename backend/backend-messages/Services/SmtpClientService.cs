using BackendMessages.Models.Email;
using BackendMessages.Services.Abstractions;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace BackendMessages.Services
{
    public class SmtpClientService : ISmtpClientService
    {
        private readonly ILogger<SmtpClientService> _logger;
        private readonly EmailSettings _emailSettings;

        public SmtpClientService(ILogger<SmtpClientService> logger, IOptions<EmailSettings> emailSettings)
        {
            _logger = logger;
            _emailSettings = emailSettings.Value;
        }

        public async Task<bool> SendEmailAsync(EmailMessage emailMessage)
        {
            if (emailMessage == null)
            {
                _logger.LogWarning("Attempted to send email with null EmailMessage");
                return false;
            }

            var stopwatch = Stopwatch.StartNew();

            try
            {
                var mimeMessage = CreateMimeMessage(emailMessage);
                
                using var client = new SmtpClient();
                await ConfigureSmtpClientAsync(client);
                await client.SendAsync(mimeMessage);
                await client.DisconnectAsync(true);
                
                stopwatch.Stop();

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
                : _emailSettings.FromAddress;
            
            var fromName = !string.IsNullOrEmpty(emailMessage.FromName) 
                ? emailMessage.FromName 
                : _emailSettings.FromName;

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
            var host = _emailSettings.SmtpHost;
            var port = _emailSettings.SmtpPort;
            var useSSL = _emailSettings.UseSSL;
            var username = _emailSettings.SmtpUsername;
            var password = _emailSettings.SmtpPassword;
            var isGmail = host.Contains("gmail.com", StringComparison.OrdinalIgnoreCase);
            var hasCredentials = !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);

            client.Timeout = _emailSettings.TimeoutSeconds * 1000; // Convert to milliseconds
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