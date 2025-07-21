using backend_user.Models;

namespace backend_user.DTO.OAuthProvider.Response
{
    /// <summary>
    /// Summarized OAuth provider information for list views.
    /// </summary>
    public class OAuthProviderSummaryDto
    {
        public Guid Id { get; set; }
        public OAuthProviderType Provider { get; set; }
        public string ProviderEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
