using BackendMessages.Models.Email;
using Microsoft.Extensions.Options;

namespace BackendMessages.Services
{
    public interface IStartupValidationService
    {
        void ValidateConfiguration();
    }

    public class StartupValidationService : IStartupValidationService
    {
        private readonly ILogger<StartupValidationService> _logger;
        private readonly IOptions<EmailSettings> _emailSettings;
        private readonly IWebHostEnvironment _environment;

        public StartupValidationService(
            ILogger<StartupValidationService> logger,
            IOptions<EmailSettings> emailSettings,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _emailSettings = emailSettings;
            _environment = environment;
        }

        public void ValidateConfiguration()
        {
            ValidateEmailConfiguration();
        }

        private void ValidateEmailConfiguration()
        {
            try
            {
                var emailSettings = _emailSettings.Value;
                emailSettings.Validate();

                var hasUsername = !string.IsNullOrEmpty(emailSettings.SmtpUsername);
                var hasPassword = !string.IsNullOrEmpty(emailSettings.SmtpPassword);

                _logger.LogInformation("Email configuration validated successfully. Has credentials: {HasCredentials}", 
                    hasUsername && hasPassword);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email configuration validation failed: {Message}", ex.Message);
                
                if (_environment.IsProduction())
                {
                    _logger.LogCritical("Application cannot start with invalid email configuration in production");
                    Environment.Exit(1);
                }
                
                // Re-throw in non-production for debugging
                throw;
            }
        }
    }
} 