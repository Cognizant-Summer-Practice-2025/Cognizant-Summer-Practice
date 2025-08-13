using backend_user.Models;
using backend_user.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace backend_user.Services
{
    /// <summary>
    /// OAuth2 Bearer token authentication strategy.
    /// </summary>
    public class OAuth2AuthenticationStrategy : IAuthenticationStrategy
    {
        private readonly IOAuth2Service _oauth2Service;
        private readonly ILogger<OAuth2AuthenticationStrategy> _logger;

        public OAuth2AuthenticationStrategy(
            IOAuth2Service oauth2Service,
            ILogger<OAuth2AuthenticationStrategy> logger)
        {
            _oauth2Service = oauth2Service ?? throw new ArgumentNullException(nameof(oauth2Service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Determines if this strategy can handle OAuth2 Bearer token authentication.
        /// </summary>
        public bool CanHandle(HttpContext context)
        {
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            return authHeader != null && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Authenticates using OAuth2 Bearer token.
        /// </summary>
        public async Task<User?> AuthenticateAsync(HttpContext context)
        {
            try
            {
                var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                if (authHeader == null || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Missing or invalid Authorization header format");
                    return null;
                }

                var token = authHeader.Substring("Bearer ".Length).Trim();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Empty access token provided");
                    return null;
                }

                var user = await _oauth2Service.GetUserByAccessTokenAsync(token);
                if (user == null)
                {
                    _logger.LogWarning("Invalid or expired access token");
                    return null;
                }

                _logger.LogInformation("Successfully authenticated user with ID: {UserId}", user.Id);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during OAuth2 authentication");
                return null;
            }
        }
    }
}
