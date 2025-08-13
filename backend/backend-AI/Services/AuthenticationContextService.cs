using System.Security.Claims;
using backend_AI.Services.Abstractions;

namespace backend_AI.Services
{
    /// <summary>
    /// Coordinates authentication across configured strategies.
    /// </summary>
    public class AuthenticationContextService : IAuthenticationContextService
    {
        private readonly IEnumerable<IAuthenticationStrategy> _strategies;
        private readonly ILogger<AuthenticationContextService> _logger;

        public AuthenticationContextService(IEnumerable<IAuthenticationStrategy> strategies, ILogger<AuthenticationContextService> logger)
        {
            _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ClaimsPrincipal?> AuthenticateAsync(HttpContext context)
        {
            if (context == null)
            {
                _logger.LogWarning("HttpContext is null in AuthenticateAsync.");
                return null;
            }

            foreach (var strategy in _strategies)
            {
                if (!strategy.CanHandle(context))
                {
                    continue;
                }

                _logger.LogDebug("Using authentication strategy: {Strategy}", strategy.GetType().Name);
                try
                {
                    var principal = await strategy.AuthenticateAsync(context);
                    if (principal != null)
                    {
                        _logger.LogInformation("Successfully authenticated using {Strategy}", strategy.GetType().Name);
                        return principal;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Authentication strategy {Strategy} failed", strategy.GetType().Name);
                }
            }

            _logger.LogWarning("No authentication strategy could handle the request or authentication failed.");
            return null;
        }
    }
}


