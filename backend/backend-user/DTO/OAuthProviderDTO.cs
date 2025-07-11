using backend_user.Models;

namespace backend_user.DTO;

// OAuthProvider Response DTO
public class OAuthProviderResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Provider Provider { get; set; }
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderEmail { get; set; } = string.Empty;
    public DateTime? TokenExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// OAuthProvider Request DTO
public class OAuthProviderRequestDto
{
    public Guid UserId { get; set; }
    public Provider Provider { get; set; }
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderEmail { get; set; } = string.Empty;
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
}

// OAuthProvider Summary DTO
public class OAuthProviderSummaryDto
{
    public Guid Id { get; set; }
    public Provider Provider { get; set; }
    public string ProviderEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}