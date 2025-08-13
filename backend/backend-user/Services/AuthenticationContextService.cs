using backend_user.Models;
using backend_user.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace backend_user.Services
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

        /// <summary>
        /// Attempts to authenticate the request using the first suitable strategy.
        /// </summary>
        /// <param name="context">The HTTP context to authenticate.</param>
        /// <returns>The authenticated user if successful, null otherwise.</returns>
        public async Task<User?> AuthenticateAsync(HttpContext context)
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
                        var user = await strategy.AuthenticateAsync(context);
                        if (user != null)
                        {
                            _logger.LogInformation("Successfully authenticated user {UserId} using {Strategy}", 
                                user.Id, strategy.GetType().Name);
                            return user;
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
