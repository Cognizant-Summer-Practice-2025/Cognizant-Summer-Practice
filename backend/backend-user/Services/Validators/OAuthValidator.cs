using backend_user.DTO.OAuthProvider.Request;
using backend_user.DTO.Authentication.Request;
using backend_user.Models;

namespace backend_user.Services.Validators
{
    /// <summary>
    /// Validator for OAuth-related operations.
    /// </summary>
    public static class OAuthValidator
    {
        /// <summary>
        /// Validates OAuthLoginRequestDto.
        /// </summary>
        public static ValidationResult ValidateLoginRequest(OAuthLoginRequestDto request)
        {
            if (request == null)
                return ValidationResult.Failure("Request cannot be null");

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.ProviderId))
                errors.Add("Provider ID is required");

            if (string.IsNullOrWhiteSpace(request.ProviderEmail))
                errors.Add("Provider email is required");
            else if (!IsValidEmail(request.ProviderEmail))
                errors.Add("Provider email format is invalid");

            if (string.IsNullOrWhiteSpace(request.AccessToken))
                errors.Add("Access token is required");

            return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
        }

        /// <summary>
        /// Validates OAuthProviderCreateRequestDto.
        /// </summary>
        public static ValidationResult ValidateCreateRequest(OAuthProviderCreateRequestDto request)
        {
            if (request == null)
                return ValidationResult.Failure("Request cannot be null");

            var errors = new List<string>();

            if (request.UserId == Guid.Empty)
                errors.Add("User ID is required");

            if (string.IsNullOrWhiteSpace(request.ProviderId))
                errors.Add("Provider ID is required");

            if (string.IsNullOrWhiteSpace(request.ProviderEmail))
                errors.Add("Provider email is required");
            else if (!IsValidEmail(request.ProviderEmail))
                errors.Add("Provider email format is invalid");

            if (string.IsNullOrWhiteSpace(request.AccessToken))
                errors.Add("Access token is required");

            return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
        }

        /// <summary>
        /// Validates OAuthProviderUpdateRequestDto.
        /// </summary>
        public static ValidationResult ValidateUpdateRequest(OAuthProviderUpdateRequestDto request)
        {
            if (request == null)
                return ValidationResult.Failure("Request cannot be null");

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.AccessToken))
                errors.Add("Access token is required");

            return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
        }

        /// <summary>
        /// Validates OAuth provider credentials.
        /// </summary>
        public static ValidationResult ValidateProviderCredentials(OAuthProviderType provider, string providerId, string providerEmail)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(providerId))
                errors.Add("Provider ID is required");

            if (string.IsNullOrWhiteSpace(providerEmail))
                errors.Add("Provider email is required");
            else if (!IsValidEmail(providerEmail))
                errors.Add("Provider email format is invalid");

            return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
        }

        /// <summary>
        /// Validates OAuth provider ID.
        /// </summary>
        public static ValidationResult ValidateOAuthProviderId(Guid providerId)
        {
            if (providerId == Guid.Empty)
                return ValidationResult.Failure("OAuth provider ID cannot be empty");

            return ValidationResult.Success();
        }

        /// <summary>
        /// Validates email format.
        /// </summary>
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
