using System.ComponentModel.DataAnnotations;
using backend_user.Models;

namespace backend_user.DTO.Authentication.Request
{
    /// <summary>
    /// Request DTO for OAuth login operations.
    /// </summary>
    public class OAuthLoginRequestDto
    {
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
