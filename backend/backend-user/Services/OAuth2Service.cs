using System.Security.Cryptography;
using System.Text;
using backend_user.Models;
using backend_user.Repositories;
using backend_user.Services.Abstractions;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace backend_user.Services
{
    /// <summary>
    /// Simplified OAuth 2.0 service using existing oauth_providers table.
    /// </summary>
    public class OAuth2Service : IOAuth2Service
    {
        private readonly IOAuthProviderRepository _oauthProviderRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<OAuth2Service> _logger;

        public OAuth2Service(
            IOAuthProviderRepository oauthProviderRepository,
            IUserRepository userRepository,
            ILogger<OAuth2Service> logger)
        {
            _oauthProviderRepository = oauthProviderRepository ?? throw new ArgumentNullException(nameof(oauthProviderRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        /// <returns>Updated OAuth provider data if refresh was successful, null otherwise.</returns>
        public async Task<OAuthProvider?> RefreshAccessTokenAsync(string refreshToken)
        {
            var oauthProvider = await _oauthProviderRepository.GetByRefreshTokenAsync(refreshToken);
            if (oauthProvider == null)
            {
                _logger.LogWarning("Refresh token not found in database");
                return null;
            }
            
            if (string.IsNullOrEmpty(oauthProvider.RefreshToken))
            {
                _logger.LogWarning("OAuth provider {Provider} has empty refresh token", oauthProvider.Provider);
                return null;
            }

            _logger.LogInformation("Found OAuth provider for refresh: {Provider} (User: {UserId})", oauthProvider.Provider, oauthProvider.UserId);

            try
            {
                // Call the appropriate OAuth provider's refresh endpoint
                var refreshResult = await RefreshTokenWithProvider(oauthProvider);
                if (refreshResult == null)
                {
                    _logger.LogWarning("Token refresh failed for provider {Provider}", oauthProvider.Provider);
                    return null;
                }

                _logger.LogInformation("Token refresh successful for provider {Provider}", oauthProvider.Provider);

                // Update the OAuth provider with new tokens
                oauthProvider.AccessToken = refreshResult.AccessToken;
                oauthProvider.TokenExpiresAt = refreshResult.ExpiresAt;
                
                // Update refresh token if provider returned a new one
                if (!string.IsNullOrEmpty(refreshResult.RefreshToken))
                {
                    oauthProvider.RefreshToken = refreshResult.RefreshToken;
                }

                await _oauthProviderRepository.UpdateAsync(oauthProvider);
                return oauthProvider;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during token refresh for provider {Provider}", oauthProvider.Provider);
                return null;
            }
        }

        /// <summary>
        /// Refreshes tokens with the appropriate OAuth provider.
        /// </summary>
        /// <param name="oauthProvider">The OAuth provider record.</param>
        /// <returns>New token information or null if refresh failed.</returns>
        private async Task<TokenRefreshResult?> RefreshTokenWithProvider(OAuthProvider oauthProvider)
        {
            using var httpClient = new HttpClient();
            
            switch (oauthProvider.Provider)
            {
                case OAuthProviderType.Google:
                    return await RefreshGoogleToken(httpClient, oauthProvider);
                
                case OAuthProviderType.GitHub:
                    // GitHub OAuth apps don't support refresh tokens
                    // GitHub Apps do support refresh tokens, but this is OAuth app flow
                    return null;
                
                case OAuthProviderType.Facebook:
                    return await RefreshFacebookToken(httpClient, oauthProvider);
                
                case OAuthProviderType.LinkedIn:
                    return await RefreshLinkedInToken(httpClient, oauthProvider);
                
                default:
                    return null;
            }
        }

        /// <summary>
        /// Refreshes a Google OAuth token.
        /// </summary>
        private async Task<TokenRefreshResult?> RefreshGoogleToken(HttpClient httpClient, OAuthProvider oauthProvider)
        {
            var tokenEndpoint = "https://oauth2.googleapis.com/token";
            var clientId = Environment.GetEnvironmentVariable("AUTH_GOOGLE_ID");
            var clientSecret = Environment.GetEnvironmentVariable("AUTH_GOOGLE_SECRET");

            _logger.LogDebug("Google refresh: ClientId exists: {HasClientId}, ClientSecret exists: {HasClientSecret}", 
                !string.IsNullOrEmpty(clientId), !string.IsNullOrEmpty(clientSecret));

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogError("Google OAuth credentials missing");
                return null;
            }

            var requestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", oauthProvider.RefreshToken!)
            });

            _logger.LogInformation("Calling Google token refresh endpoint: {TokenEndpoint}", tokenEndpoint);

            var response = await httpClient.PostAsync(tokenEndpoint, requestBody);
            
            _logger.LogInformation("Google refresh response: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Google refresh failed: {StatusCode} - {ErrorContent}", response.StatusCode, errorContent);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Google refresh response received: {ResponseLength} characters", responseContent.Length);
            
            var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<GoogleTokenResponse>(responseContent);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.access_token))
            {
                _logger.LogError("Google refresh response parsing failed or no access token");
                return null;
            }

            _logger.LogInformation("New Google token received, length: {TokenLength}", tokenResponse.access_token.Length);

            return new TokenRefreshResult
            {
                AccessToken = tokenResponse.access_token,
                RefreshToken = tokenResponse.refresh_token, // Google may not always return a new refresh token
                ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in ?? 3600)
            };
        }

        /// <summary>
        /// Refreshes a Facebook OAuth token.
        /// </summary>
        private async Task<TokenRefreshResult?> RefreshFacebookToken(HttpClient httpClient, OAuthProvider oauthProvider)
        {
            // Facebook doesn't use traditional refresh tokens for most OAuth flows
            // Instead, they use long-lived access tokens
            // For now, return null to indicate refresh is not supported
            await Task.CompletedTask;
            return null;
        }

        /// <summary>
        /// Refreshes a LinkedIn OAuth token.
        /// </summary>
        private async Task<TokenRefreshResult?> RefreshLinkedInToken(HttpClient httpClient, OAuthProvider oauthProvider)
        {
            var tokenEndpoint = "https://www.linkedin.com/oauth/v2/accessToken";
            var clientId = Environment.GetEnvironmentVariable("AUTH_LINKEDIN_ID");
            var clientSecret = Environment.GetEnvironmentVariable("AUTH_LINKEDIN_SECRET");

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
                return null;

            var requestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", oauthProvider.RefreshToken!)
            });

            var response = await httpClient.PostAsync(tokenEndpoint, requestBody);
            if (!response.IsSuccessStatusCode)
                return null;

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<LinkedInTokenResponse>(responseContent);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.access_token))
                return null;

            return new TokenRefreshResult
            {
                AccessToken = tokenResponse.access_token,
                RefreshToken = tokenResponse.refresh_token,
                ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.expires_in ?? 3600)
            };
        }

        /// <summary>
        /// Result of a token refresh operation.
        /// </summary>
        private class TokenRefreshResult
        {
            public string AccessToken { get; set; } = string.Empty;
            public string? RefreshToken { get; set; }
            public DateTime ExpiresAt { get; set; }
        }

        /// <summary>
        /// Google token response model.
        /// </summary>
        private class GoogleTokenResponse
        {
            public string access_token { get; set; } = string.Empty;
            public string? refresh_token { get; set; }
            public int? expires_in { get; set; }
            public string? token_type { get; set; }
        }

        /// <summary>
        /// LinkedIn token response model.
        /// </summary>
        private class LinkedInTokenResponse
        {
            public string access_token { get; set; } = string.Empty;
            public string? refresh_token { get; set; }
            public int? expires_in { get; set; }
            public string? token_type { get; set; }
        }
    }
}