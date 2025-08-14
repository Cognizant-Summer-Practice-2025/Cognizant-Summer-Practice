using System.Security.Claims;
using backend_portfolio.Services.Abstractions;

namespace backend_portfolio.Services
{
    /// <summary>
    /// OAuth2 Bearer token authentication strategy.
    /// </summary>
    public class OAuth2AuthenticationStrategy : IAuthenticationStrategy
    {
        private readonly IUserAuthenticationService _userAuthenticationService;
        private readonly ILogger<OAuth2AuthenticationStrategy> _logger;

        public OAuth2AuthenticationStrategy(
            IUserAuthenticationService userAuthenticationService,
            ILogger<OAuth2AuthenticationStrategy> logger)
        {
            _userAuthenticationService = userAuthenticationService ?? throw new ArgumentNullException(nameof(userAuthenticationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool CanHandle(HttpContext context)
        {
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            return authHeader != null && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);
        }

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

                var principal = await _userAuthenticationService.ValidateTokenAsync(token);
                if (principal == null)
                {
                    _logger.LogWarning("Invalid or expired access token");
                    return null;
                }

                _logger.LogInformation("Successfully validated token for user: {UserId}",
                    principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
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


