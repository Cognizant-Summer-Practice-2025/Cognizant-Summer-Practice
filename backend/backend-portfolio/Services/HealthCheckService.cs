using backend_portfolio.Data;
using backend_portfolio.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend_portfolio.Services
{
    /// <summary>
    /// Implementation of health check service.
    /// </summary>
    public class HealthCheckService : IHealthCheckService
    {
        private readonly PortfolioDbContext _context;
        private readonly ILogger<HealthCheckService> _logger;

        public HealthCheckService(PortfolioDbContext context, ILogger<HealthCheckService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<object> GetBasicHealthAsync()
        {
            return Task.FromResult<object>(new
            {
                status = "ok",
                service = "backend-portfolio",
                timestamp = DateTime.UtcNow
            });
        }

        public async Task<object> GetDatabaseHealthAsync()
        {
            try
            {
                // Test database connection by executing a simple query
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger.LogWarning("Database connection failed during health check");
                    return new
                    {
                        status = "error",
                        service = "backend-portfolio",
                        database = "unavailable",
                        message = "Database connection failed",
                        timestamp = DateTime.UtcNow
                    };
                }

                // Test if we can execute a simple query
                var portfolioCount = await _context.Portfolios.CountAsync();
                
                _logger.LogInformation("Database health check successful. Portfolio count: {PortfolioCount}", portfolioCount);
                return new
                {
                    status = "ok",
                    service = "backend-portfolio",
                    database = "available",
                    portfolioCount = portfolioCount,
                    timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database health check");
                return new
                {
                    status = "error",
                    service = "backend-portfolio",
                    database = "error",
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                };
            }
        }
    }
}
