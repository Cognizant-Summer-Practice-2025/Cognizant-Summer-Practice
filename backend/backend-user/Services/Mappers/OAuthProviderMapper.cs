using backend_user.DTO.OAuthProvider.Request;
using backend_user.DTO.OAuthProvider.Response;
using backend_user.DTO.User.Request;
using backend_user.DTO.Authentication.Request;
using backend_user.Models;

namespace backend_user.Services.Mappers
{
    /// <summary>
    /// Mapper for OAuthProvider entity and related DTOs.
    /// </summary>
    public static class OAuthProviderMapper
    {
        /// <summary>
        /// Maps OAuthProvider entity to OAuthProviderResponseDto.
        /// </summary>
        public static OAuthProviderResponseDto ToResponseDto(OAuthProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

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

        /// <summary>
        /// Maps OAuthProvider entity to OAuthProviderSummaryDto.
        /// </summary>
        public static OAuthProviderSummaryDto ToSummaryDto(OAuthProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            return new OAuthProviderSummaryDto
            {
                Id = provider.Id,
                Provider = provider.Provider,
                ProviderEmail = provider.ProviderEmail,
                CreatedAt = provider.CreatedAt
            };
        }

        /// <summary>
        /// Maps RegisterOAuthUserRequest to OAuthProviderCreateRequestDto.
        /// </summary>
        public static OAuthProviderCreateRequestDto ToCreateRequest(RegisterOAuthUserRequest request, Guid userId)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return new OAuthProviderCreateRequestDto
            {
                UserId = userId,
                Provider = request.Provider,
                ProviderId = request.ProviderId,
                ProviderEmail = request.ProviderEmail,
                AccessToken = request.AccessToken,
                RefreshToken = request.RefreshToken,
                TokenExpiresAt = request.TokenExpiresAt
            };
        }

        /// <summary>
        /// Maps OAuthLoginRequestDto to OAuthProviderUpdateRequestDto.
        /// </summary>
        public static OAuthProviderUpdateRequestDto ToUpdateRequest(OAuthLoginRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return new OAuthProviderUpdateRequestDto
            {
                AccessToken = request.AccessToken,
                RefreshToken = request.RefreshToken,
                TokenExpiresAt = request.TokenExpiresAt
            };
        }
    }
}
