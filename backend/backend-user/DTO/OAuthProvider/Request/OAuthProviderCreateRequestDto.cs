using System.ComponentModel.DataAnnotations;
using backend_user.Models;

namespace backend_user.DTO.OAuthProvider.Request
{
    /// <summary>
    /// Request DTO for creating OAuth provider connections.
    /// </summary>
    public class OAuthProviderCreateRequestDto
    {
        [Required]
        public Guid UserId { get; set; }

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
