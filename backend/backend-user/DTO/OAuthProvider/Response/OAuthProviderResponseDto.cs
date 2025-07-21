using backend_user.Models;

namespace backend_user.DTO.OAuthProvider.Response
{
    /// <summary>
    /// Complete OAuth provider response DTO with all provider information.
    /// </summary>
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
}
