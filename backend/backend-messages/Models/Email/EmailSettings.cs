using System.ComponentModel.DataAnnotations;

namespace BackendMessages.Models.Email
{
    /// <summary>
    /// Strongly-typed configuration for email settings
    /// </summary>
    public class EmailSettings
    {
        public const string SectionName = "Email";

        [Required]
        public string SmtpHost { get; set; } = "smtp.gmail.com";

        [Range(1, 65535)]
        public int SmtpPort { get; set; } = 587;

        public string SmtpUsername { get; set; } = string.Empty;

        public string SmtpPassword { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string FromAddress { get; set; } = string.Empty;

        [Required]
        public string FromName { get; set; } = string.Empty;

        public bool UseSSL { get; set; } = true;

        public bool EnableContactNotifications { get; set; } = true;

        public int TimeoutSeconds { get; set; } = 60;

        public int MaxRetryAttempts { get; set; } = 3;

        public int RetryDelaySeconds { get; set; } = 5;

        /// <summary>
        /// Validates the email configuration
        /// </summary>
        public void Validate()
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(this);
            
            if (!Validator.TryValidateObject(this, validationContext, validationResults, true))
            {
                var errors = string.Join(", ", validationResults.Select(r => r.ErrorMessage));
                throw new InvalidOperationException($"Email configuration is invalid: {errors}");
            }

            // Additional custom validation
            if (SmtpPort <= 0 || SmtpPort > 65535)
            {
                throw new InvalidOperationException("SMTP port must be between 1 and 65535");
            }

            if (TimeoutSeconds <= 0)
            {
                throw new InvalidOperationException("Timeout must be greater than 0 seconds");
            }

            if (MaxRetryAttempts < 0)
            {
                throw new InvalidOperationException("Max retry attempts cannot be negative");
            }
        }
    }
} 