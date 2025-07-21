using backend_user.DTO;
using backend_user.Models;
using backend_user.Repositories;

namespace backend_user.Services
{
    /// <summary>
    /// Implementation of OAuth provider service.
    /// Follows Single Responsibility Principle by handling only OAuth provider operations.
    /// Follows Dependency Inversion Principle by depending on abstractions (interfaces).
    /// </summary>
    public class OAuthProviderService : IOAuthProviderService
    {
        private readonly IOAuthProviderRepository _oauthProviderRepository;

        public OAuthProviderService(IOAuthProviderRepository oauthProviderRepository)
        {
            _oauthProviderRepository = oauthProviderRepository ?? throw new ArgumentNullException(nameof(oauthProviderRepository));
        }

        public async Task<IEnumerable<OAuthProviderSummaryDto>> GetUserOAuthProvidersAsync(Guid userId)
        {
            // Extracted logic from controller's GetUserOAuthProviders endpoint
            var providers = await _oauthProviderRepository.GetByUserIdAsync(userId);
            var response = providers.Select(p => new OAuthProviderSummaryDto
            {
                Id = p.Id,
                Provider = p.Provider,
                ProviderEmail = p.ProviderEmail,
                CreatedAt = p.CreatedAt
            });
            return response;
        }

        public async Task<OAuthProviderResponseDto> CreateOAuthProviderAsync(OAuthProviderCreateRequestDto request)
        {
            // Extracted logic from controller's CreateOAuthProvider endpoint
            var provider = await _oauthProviderRepository.CreateAsync(request);
            var response = new OAuthProviderResponseDto
            {
                Id = provider.Id,
                UserId = provider.UserId,
                Provider = provider.Provider,
                ProviderId = provider.ProviderId,
                ProviderEmail = provider.ProviderEmail,
                TokenExpiresAt = provider.TokenExpiresAt,
                CreatedAt = provider.CreatedAt,
                UpdatedAt = provider.UpdatedAt
            };
            return response;
        }

        public async Task<OAuthProviderResponseDto?> UpdateOAuthProviderAsync(Guid id, OAuthProviderUpdateRequestDto request)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("OAuth provider ID cannot be empty", nameof(id));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var provider = await _oauthProviderRepository.UpdateAsync(id, request);
            if (provider == null)
                return null;

            return new OAuthProviderResponseDto
            {
                Id = provider.Id,
                UserId = provider.UserId,
                Provider = provider.Provider,
                ProviderId = provider.ProviderId,
                ProviderEmail = provider.ProviderEmail,
                TokenExpiresAt = provider.TokenExpiresAt,
                CreatedAt = provider.CreatedAt,
                UpdatedAt = provider.UpdatedAt
            };
        }

        public async Task<bool> DeleteOAuthProviderAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("OAuth provider ID cannot be empty", nameof(id));

            return await _oauthProviderRepository.DeleteAsync(id);
        }

        public async Task<object> CheckOAuthProviderAsync(OAuthProviderType provider, string providerId)
        {
            // Extracted logic from controller's CheckOAuthProvider endpoint
            var exists = await _oauthProviderRepository.ExistsAsync(provider, providerId);
            var oauthProvider = await _oauthProviderRepository.GetByProviderAndProviderIdAsync(provider, providerId);
            return new { exists = exists, provider = oauthProvider };
        }

        public async Task<object> GetUserOAuthProviderByTypeAsync(Guid userId, OAuthProviderType provider)
        {
            // Extracted logic from controller's GetUserOAuthProviderByType endpoint
            var oauthProvider = await _oauthProviderRepository.GetByUserIdAndProviderAsync(userId, provider);
            if (oauthProvider == null)
            {
                return new { exists = false, provider = (OAuthProvider?)null };
            }

            var response = new OAuthProviderResponseDto
            {
                Id = oauthProvider.Id,
                UserId = oauthProvider.UserId,
                Provider = oauthProvider.Provider,
                ProviderId = oauthProvider.ProviderId,
                ProviderEmail = oauthProvider.ProviderEmail,
                TokenExpiresAt = oauthProvider.TokenExpiresAt,
                CreatedAt = oauthProvider.CreatedAt,
                UpdatedAt = oauthProvider.UpdatedAt
            };
            return new { exists = true, provider = response };
        }
    }
}
