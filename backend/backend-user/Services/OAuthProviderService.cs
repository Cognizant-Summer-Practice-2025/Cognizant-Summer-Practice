using backend_user.DTO.OAuthProvider.Request;
using backend_user.DTO.OAuthProvider.Response;
using backend_user.Models;
using backend_user.Repositories;
using backend_user.Services.Abstractions;
using backend_user.Services.Mappers;
using backend_user.Services.Validators;

namespace backend_user.Services
{
    /// <summary>
    /// Implementation of OAuth provider service.
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
            var validation = UserValidator.ValidateUserId(userId);
            if (!validation.IsValid)
                throw new ArgumentException(string.Join(", ", validation.Errors));

            var providers = await _oauthProviderRepository.GetByUserIdAsync(userId);
            return providers.Select(OAuthProviderMapper.ToSummaryDto);
        }

        public async Task<OAuthProviderResponseDto> CreateOAuthProviderAsync(OAuthProviderCreateRequestDto request)
        {
            var validation = OAuthValidator.ValidateCreateRequest(request);
            if (!validation.IsValid)
                throw new ArgumentException(string.Join(", ", validation.Errors));

            var provider = await _oauthProviderRepository.CreateAsync(request);
            return OAuthProviderMapper.ToResponseDto(provider);
        }

        public async Task<OAuthProviderResponseDto?> UpdateOAuthProviderAsync(Guid id, OAuthProviderUpdateRequestDto request)
        {
            var idValidation = OAuthValidator.ValidateOAuthProviderId(id);
            if (!idValidation.IsValid)
                throw new ArgumentException(string.Join(", ", idValidation.Errors));

            var requestValidation = OAuthValidator.ValidateUpdateRequest(request);
            if (!requestValidation.IsValid)
                throw new ArgumentException(string.Join(", ", requestValidation.Errors));

            var provider = await _oauthProviderRepository.UpdateAsync(id, request);
            return provider == null ? null : OAuthProviderMapper.ToResponseDto(provider);
        }

        public async Task<bool> DeleteOAuthProviderAsync(Guid id)
        {
            var validation = OAuthValidator.ValidateOAuthProviderId(id);
            if (!validation.IsValid)
                throw new ArgumentException(string.Join(", ", validation.Errors));

            return await _oauthProviderRepository.DeleteAsync(id);
        }

        public async Task<object> CheckOAuthProviderAsync(OAuthProviderType provider, string providerId)
        {
            var exists = await _oauthProviderRepository.ExistsAsync(provider, providerId);
            var oauthProvider = await _oauthProviderRepository.GetByProviderAndProviderIdAsync(provider, providerId);
            return new { exists = exists, provider = oauthProvider };
        }

        public async Task<object> GetUserOAuthProviderByTypeAsync(Guid userId, OAuthProviderType provider)
        {
            var validation = UserValidator.ValidateUserId(userId);
            if (!validation.IsValid)
                throw new ArgumentException(string.Join(", ", validation.Errors));

            var oauthProvider = await _oauthProviderRepository.GetByUserIdAndProviderAsync(userId, provider);
            if (oauthProvider == null)
            {
                return new { exists = false, provider = (OAuthProvider?)null };
            }

            var response = OAuthProviderMapper.ToResponseDto(oauthProvider);
            return new { exists = true, provider = response };
        }
    }
}
