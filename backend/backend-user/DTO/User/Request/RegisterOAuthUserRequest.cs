using System.ComponentModel.DataAnnotations;
using backend_user.Models;

namespace backend_user.DTO.User.Request
{
    /// <summary>
    /// Request DTO for OAuth user registration.
    /// Combines user registration data with OAuth provider information.
    /// </summary>
    public class RegisterOAuthUserRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? ProfessionalTitle { get; set; }

        public string? Bio { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        public string? ProfileImage { get; set; }

        // OAuth provider data
        [Required]
        public OAuthProviderType Provider { get; set; }

        [Required]
        public string ProviderId { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string ProviderEmail { get; set; } = string.Empty;

        [Required]
        public string AccessToken { get; set; } = string.Empty;

        public string? RefreshToken { get; set; }

        public DateTime? TokenExpiresAt { get; set; }
    }
}
