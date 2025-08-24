using backend_user.Data;
using backend_user.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend_user.Services
{
    /// <summary>
    /// Implementation of health check service.
    /// </summary>
    public class HealthCheckService : IHealthCheckService
    {
        private readonly UserDbContext _context;
        private readonly ILogger<HealthCheckService> _logger;

        public HealthCheckService(UserDbContext context, ILogger<HealthCheckService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<object> GetBasicHealthAsync()
        {
            return Task.FromResult<object>(new
            {
                status = "ok",
                service = "backend-user",
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
                        service = "backend-user",
                        database = "unavailable",
                        message = "Database connection failed",
                        timestamp = DateTime.UtcNow
                    };
                }

                // Test if we can execute a simple query
                var userCount = await _context.Users.CountAsync();
                
                _logger.LogInformation("Database health check successful. User count: {UserCount}", userCount);
                return new
                {
                    status = "ok",
                    service = "backend-user",
                    database = "available",
                    userCount = userCount,
                    timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database health check");
                return new
                {
                    status = "error",
                    service = "backend-user",
                    database = "error",
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                };
            }
        }
    }
}
