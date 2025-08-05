using System.Security.Cryptography;
using System.Text;
using backend_user.Models;
using backend_user.Repositories;
using backend_user.Services.Abstractions;

namespace backend_user.Services
{
    /// <summary>
    /// Simplified OAuth 2.0 service using existing oauth_providers table.
    /// </summary>
    public class OAuth2Service : IOAuth2Service
    {
        private readonly IOAuthProviderRepository _oauthProviderRepository;
        private readonly IUserRepository _userRepository;

        public OAuth2Service(
            IOAuthProviderRepository oauthProviderRepository,
            IUserRepository userRepository)
        {
            _oauthProviderRepository = oauthProviderRepository ?? throw new ArgumentNullException(nameof(oauthProviderRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        /// <summary>
        /// Validates an OAuth provider's access token.
        /// </summary>
        /// <param name="token">The access token to validate.</param>
        /// <returns>The OAuth provider if token is valid, null otherwise.</returns>
        public async Task<OAuthProvider?> ValidateAccessTokenAsync(string token)
        {
            var oauthProvider = await _oauthProviderRepository.GetByAccessTokenAsync(token);
            
            // Check if token is expired
            if (oauthProvider?.TokenExpiresAt.HasValue == true && 
                oauthProvider.TokenExpiresAt.Value < DateTime.UtcNow)
            {
                return null;
            }

            return oauthProvider;
        }

        /// <summary>
        /// Gets user by access token.
        /// </summary>
        /// <param name="token">The access token.</param>
        /// <returns>The user if token is valid, null otherwise.</returns>
        public async Task<User?> GetUserByAccessTokenAsync(string token)
        {
            var oauthProvider = await ValidateAccessTokenAsync(token);
            if (oauthProvider == null)
                return null;

            return await _userRepository.GetUserById(oauthProvider.UserId);
        }

        /// <summary>
        /// Refreshes an access token using a refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns>True if refresh was successful, false otherwise.</returns>
        public async Task<bool> RefreshAccessTokenAsync(string refreshToken)
        {
            var oauthProvider = await _oauthProviderRepository.GetByRefreshTokenAsync(refreshToken);
            if (oauthProvider == null)
                return false;

            // In a real implementation, you would call the OAuth provider's refresh endpoint
            // For now, we'll just extend the token expiry
            oauthProvider.TokenExpiresAt = DateTime.UtcNow.AddHours(1);
            await _oauthProviderRepository.UpdateAsync(oauthProvider);

            return true;
        }
    }
}