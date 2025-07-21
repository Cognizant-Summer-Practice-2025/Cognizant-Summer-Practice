using backend_user.DTO.Authentication.Request;
using backend_user.DTO.Authentication.Response;
using backend_user.DTO.User.Request;
using backend_user.Models;
using backend_user.Repositories;
using backend_user.Services.Abstractions;
using backend_user.Services.Validators;

namespace backend_user.Services
{
    /// <summary>
    /// Implementation of authentication service.
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IOAuthProviderRepository _oauthProviderRepository;

        public AuthenticationService(IUserRepository userRepository, IOAuthProviderRepository oauthProviderRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _oauthProviderRepository = oauthProviderRepository ?? throw new ArgumentNullException(nameof(oauthProviderRepository));
        }

        public async Task<User?> AuthenticateOAuthUserAsync(OAuthProviderType provider, string providerId, string providerEmail)
        {
            var validation = OAuthValidator.ValidateProviderCredentials(provider, providerId, providerEmail);
            if (!validation.IsValid)
                throw new ArgumentException(string.Join(", ", validation.Errors));

            // Find the OAuth provider
            var oauthProvider = await _oauthProviderRepository.GetByProviderAndProviderIdAsync(provider, providerId);
            if (oauthProvider == null)
                return null;

            // Get the associated user
            var user = await _userRepository.GetUserById(oauthProvider.UserId);
            if (user == null || !user.IsActive)
                return null;

            // Update last login
            await UpdateLastLoginAsync(user.Id);

            return user;
        }

        public async Task<bool> UpdateLastLoginAsync(Guid userId)
        {
            try
            {
                return await _userRepository.UpdateLastLoginAsync(userId, DateTime.UtcNow);
            }
            catch (Exception)
            {
                // Log the exception in a real implementation
                return false;
            }
        }

        public async Task<bool> IsOAuthProviderLinkedAsync(OAuthProviderType provider, string providerId)
        {
            var validation = OAuthValidator.ValidateProviderCredentials(provider, providerId, "dummy@email.com");
            if (!validation.IsValid)
                return false;

            var oauthProvider = await _oauthProviderRepository.GetByProviderAndProviderIdAsync(provider, providerId);
            return oauthProvider != null;
        }
    }
}
