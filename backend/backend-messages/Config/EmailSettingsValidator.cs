using BackendMessages.Models.Email;
using Microsoft.Extensions.Options;

namespace BackendMessages.Config
{
    /// <summary>
    /// Validator for EmailSettings configuration
    /// </summary>
    public class EmailSettingsValidator : IValidateOptions<EmailSettings>
    {
        public ValidateOptionsResult Validate(string? name, EmailSettings options)
        {
            try
            {
                options.Validate();
                return ValidateOptionsResult.Success;
            }
            catch (Exception ex)
            {
                return ValidateOptionsResult.Fail(ex.Message);
            }
        }
    }
} 