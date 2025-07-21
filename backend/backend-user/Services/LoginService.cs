using backend_user.DTO;
using backend_user.Models;
using backend_user.Repositories;

namespace backend_user.Services
{
    /// <summary>
    /// Implementation of login service.
    /// Follows Single Responsibility Principle by handling only login operations.
    /// Follows Dependency Inversion Principle by depending on abstractions (interfaces).
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
            if (request == null)
                throw new ArgumentNullException(nameof(request));

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

                // Create user response
                var userResponse = new UserResponseDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ProfessionalTitle = user.ProfessionalTitle,
                    Bio = user.Bio,
                    Location = user.Location,
                    AvatarUrl = user.AvatarUrl,
                    IsActive = user.IsActive,
                    IsAdmin = user.IsAdmin,
                    LastLoginAt = user.LastLoginAt
                };

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
            if (string.IsNullOrWhiteSpace(providerId) || string.IsNullOrWhiteSpace(providerEmail))
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
