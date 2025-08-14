using backend_user.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

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
        /// Authenticates using OAuth2 Bearer token and returns a ClaimsPrincipal.
        /// </summary>
        public async Task<ClaimsPrincipal?> AuthenticateAsync(HttpContext context)
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

                // Build a ClaimsPrincipal directly here to align with other services' contracts
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Email, user.Email),
                    new(ClaimTypes.Name, user.Username),
                    new("IsAdmin", user.IsAdmin.ToString()),
                    new("IsActive", user.IsActive.ToString())
                };
                if (user.FirstName != null) claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
                if (user.LastName != null) claims.Add(new Claim(ClaimTypes.Surname, user.LastName));
                if (user.ProfessionalTitle != null) claims.Add(new Claim("ProfessionalTitle", user.ProfessionalTitle));
                if (user.Location != null) claims.Add(new Claim("Location", user.Location));
                if (user.LastLoginAt.HasValue) claims.Add(new Claim("LastLogin", user.LastLoginAt.Value.ToString("O")));

                var identity = new ClaimsIdentity(claims, "OAuth2");
                var principal = new ClaimsPrincipal(identity);

                _logger.LogInformation("Successfully authenticated user with ID: {UserId}", user.Id);
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during OAuth2 authentication");
                return null;
            }
        }
    }
}
