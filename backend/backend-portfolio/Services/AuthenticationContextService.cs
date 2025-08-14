using System.Security.Claims;
using backend_portfolio.Services.Abstractions;

namespace backend_portfolio.Services
{
    /// <summary>
    /// Context service that coordinates authentication strategies.
    /// </summary>
    public class AuthenticationContextService : IAuthenticationContextService
    {
        private readonly IEnumerable<IAuthenticationStrategy> _strategies;
        private readonly ILogger<AuthenticationContextService> _logger;

        public AuthenticationContextService(
            IEnumerable<IAuthenticationStrategy> strategies,
            ILogger<AuthenticationContextService> logger)
        {
            _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ClaimsPrincipal?> AuthenticateAsync(HttpContext context)
        {
            if (context == null)
            {
                _logger.LogWarning("HttpContext is null, cannot authenticate");
                return null;
            }

            foreach (var strategy in _strategies)
            {
                if (strategy.CanHandle(context))
                {
                    _logger.LogDebug("Using authentication strategy: {StrategyType}", strategy.GetType().Name);

                    try
                    {
                        var principal = await strategy.AuthenticateAsync(context);
                        if (principal != null)
                        {
                            _logger.LogInformation("Successfully authenticated using {Strategy}",
                                strategy.GetType().Name);
                            return principal;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Authentication strategy {Strategy} failed", strategy.GetType().Name);
                    }
                }
            }

            _logger.LogWarning("No authentication strategy could handle the request");
            return null;
        }
    }
}


