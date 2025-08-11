using backend_user.Models;
using backend_user.DTO.OAuthProvider.Request;
using backend_user.DTO.OAuthProvider.Response;

namespace backend_user.Repositories
{
    public interface IOAuthProviderRepository
    {
        Task<OAuthProvider?> GetByIdAsync(Guid id);
        Task<IEnumerable<OAuthProvider>> GetByUserIdAsync(Guid userId);
        Task<OAuthProvider?> GetByUserIdAndProviderAsync(Guid userId, OAuthProviderType provider);
        Task<OAuthProvider?> GetByProviderAndProviderIdAsync(OAuthProviderType provider, string providerId);
        Task<OAuthProvider?> GetByProviderAndEmailAsync(OAuthProviderType provider, string email);
        Task<OAuthProvider?> GetByAccessTokenAsync(string accessToken);
        Task<OAuthProvider?> GetByRefreshTokenAsync(string refreshToken);
        Task<OAuthProvider> CreateAsync(OAuthProviderCreateRequestDto request);
        Task<OAuthProvider?> UpdateAsync(Guid id, OAuthProviderUpdateRequestDto request);
        Task<OAuthProvider> UpdateAsync(OAuthProvider oauthProvider);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(OAuthProviderType provider, string providerId);
    }
} 