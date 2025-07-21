using backend_user.DTO.User.Request;

namespace backend_user.Services.Validators
{
    /// <summary>
    /// Validator for User-related operations.
    /// </summary>
    public static class UserValidator
    {
        /// <summary>
        /// Validates RegisterUserRequest.
        /// </summary>
        public static ValidationResult ValidateRegisterRequest(RegisterUserRequest request)
        {
            if (request == null)
                return ValidationResult.Failure("Request cannot be null");

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.Email))
                errors.Add("Email is required");
            else if (!IsValidEmail(request.Email))
                errors.Add("Email format is invalid");

            if (string.IsNullOrWhiteSpace(request.FirstName))
                errors.Add("First name is required");

            if (string.IsNullOrWhiteSpace(request.LastName))
                errors.Add("Last name is required");

            if (!string.IsNullOrWhiteSpace(request.ProfessionalTitle) && request.ProfessionalTitle.Length > 200)
                errors.Add("Professional title cannot exceed 200 characters");

            if (!string.IsNullOrWhiteSpace(request.Location) && request.Location.Length > 100)
                errors.Add("Location cannot exceed 100 characters");

            return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
        }

        /// <summary>
        /// Validates RegisterOAuthUserRequest.
        /// </summary>
        public static ValidationResult ValidateOAuthRegisterRequest(RegisterOAuthUserRequest request)
        {
            if (request == null)
                return ValidationResult.Failure("Request cannot be null");

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(request.Email))
                errors.Add("Email is required");
            else if (!IsValidEmail(request.Email))
                errors.Add("Email format is invalid");

            if (string.IsNullOrWhiteSpace(request.FirstName))
                errors.Add("First name is required");

            if (string.IsNullOrWhiteSpace(request.LastName))
                errors.Add("Last name is required");

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
        /// Validates UpdateUserRequest.
        /// </summary>
        public static ValidationResult ValidateUpdateRequest(UpdateUserRequest request)
        {
            if (request == null)
                return ValidationResult.Failure("Request cannot be null");

            var errors = new List<string>();

            if (!string.IsNullOrWhiteSpace(request.ProfessionalTitle) && request.ProfessionalTitle.Length > 200)
                errors.Add("Professional title cannot exceed 200 characters");

            if (!string.IsNullOrWhiteSpace(request.Location) && request.Location.Length > 100)
                errors.Add("Location cannot exceed 100 characters");

            if (!string.IsNullOrWhiteSpace(request.FirstName) && request.FirstName.Length > 100)
                errors.Add("First name cannot exceed 100 characters");

            if (!string.IsNullOrWhiteSpace(request.LastName) && request.LastName.Length > 100)
                errors.Add("Last name cannot exceed 100 characters");

            return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
        }

        /// <summary>
        /// Validates user ID.
        /// </summary>
        public static ValidationResult ValidateUserId(Guid userId)
        {
            if (userId == Guid.Empty)
                return ValidationResult.Failure("User ID cannot be empty");

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

    /// <summary>
    /// Represents the result of a validation operation.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; private set; }
        public List<string> Errors { get; private set; } = new List<string>();

        private ValidationResult(bool isValid, List<string> errors)
        {
            IsValid = isValid;
            Errors = errors ?? new List<string>();
        }

        public static ValidationResult Success() => new ValidationResult(true, new List<string>());
        public static ValidationResult Failure(string error) => new ValidationResult(false, new List<string> { error });
        public static ValidationResult Failure(List<string> errors) => new ValidationResult(false, errors);
    }
}
