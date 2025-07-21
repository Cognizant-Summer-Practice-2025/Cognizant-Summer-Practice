using backend_user.DTO.Authentication.Request;
using backend_user.DTO.Authentication.Response;
using backend_user.DTO.User.Response;
using backend_user.DTO.OAuthProvider.Request;
using backend_user.Models;
using backend_user.Repositories;
using backend_user.Services.Abstractions;
using backend_user.Services.Mappers;
using backend_user.Services.Validators;

namespace backend_user.Services
{
    /// <summary>
    /// Implementation of login service.
    /// </summary>
    public class LoginService : ILoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly IOAuthProviderRepository _oauthProviderRepository;

        public LoginService(IUserRepository userRepository, IOAuthProviderRepository oauthProviderRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _oauthProviderRepository = oauthProviderRepository ?? throw new ArgumentNullException(nameof(oauthProviderRepository));
        }

        public async Task<LoginResponseDto> LoginWithOAuthAsync(OAuthLoginRequestDto request)
        {
            var validation = OAuthValidator.ValidateLoginRequest(request);
            if (!validation.IsValid)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", validation.Errors),
                    User = null
                };
            }

            try
            {
                // Check if OAuth provider exists
                var oauthProvider = await _oauthProviderRepository.GetByProviderAndProviderIdAsync(
                    request.Provider, request.ProviderId);

                if (oauthProvider == null)
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "OAuth provider not found. Please register first.",
                        User = null
                    };
                }

                // Get the associated user
                var user = await _userRepository.GetUserById(oauthProvider.UserId);
                if (user == null || !user.IsActive)
                {
                    return new LoginResponseDto
                    {
                        Success = false,
                        Message = "User not found or inactive.",
                        User = null
                    };
                }

                // Update OAuth token information
                await UpdateOAuthTokenAsync(request.Provider, request.ProviderId, 
                    request.AccessToken, request.RefreshToken, request.TokenExpiresAt);

                // Create user response using mapper
                var userResponse = UserMapper.ToResponseDto(user);

                return new LoginResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    User = userResponse,
                    LastLoginAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = $"Login failed: {ex.Message}",
                    User = null
                };
            }
        }

        public async Task<bool> ValidateOAuthCredentialsAsync(OAuthProviderType provider, string providerId, string providerEmail)
        {
            var validation = OAuthValidator.ValidateProviderCredentials(provider, providerId, providerEmail);
            if (!validation.IsValid)
                return false;

            try
            {
                var oauthProvider = await _oauthProviderRepository.GetByProviderAndProviderIdAsync(provider, providerId);
                return oauthProvider != null && oauthProvider.ProviderEmail == providerEmail;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateOAuthTokenAsync(OAuthProviderType provider, string providerId, string accessToken, string? refreshToken = null, DateTime? tokenExpiresAt = null)
        {
            if (string.IsNullOrWhiteSpace(providerId) || string.IsNullOrWhiteSpace(accessToken))
                return false;

            try
            {
                var oauthProvider = await _oauthProviderRepository.GetByProviderAndProviderIdAsync(provider, providerId);
                if (oauthProvider == null)
                    return false;

                var updateRequest = new OAuthProviderUpdateRequestDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    TokenExpiresAt = tokenExpiresAt
                };

                var updatedProvider = await _oauthProviderRepository.UpdateAsync(oauthProvider.Id, updateRequest);
                return updatedProvider != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
