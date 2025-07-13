using System.ComponentModel.DataAnnotations;
using backend_user.Models;

namespace backend_user.DTO
{
    // Request DTO
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

    public class OAuthProviderUpdateRequestDto
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? TokenExpiresAt { get; set; }
    }

    // Response DTO
    public class OAuthProviderResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public OAuthProviderType Provider { get; set; }
        public string ProviderId { get; set; } = string.Empty;
        public string ProviderEmail { get; set; } = string.Empty;
        public DateTime? TokenExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class OAuthProviderSummaryDto
    {
        public Guid Id { get; set; }
        public OAuthProviderType Provider { get; set; }
        public string ProviderEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
} 