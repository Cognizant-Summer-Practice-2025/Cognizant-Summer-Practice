using BackendMessages.Data;
using BackendMessages.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BackendMessages.Services
{
    /// <summary>
    /// Implementation of health check service.
    /// </summary>
    public class HealthCheckService : IHealthCheckService
    {
        private readonly MessagesDbContext _context;
        private readonly ILogger<HealthCheckService> _logger;

        public HealthCheckService(MessagesDbContext context, ILogger<HealthCheckService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<object> GetBasicHealthAsync()
        {
            return Task.FromResult<object>(new
            {
                status = "ok",
                service = "backend-messages",
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
                        service = "backend-messages",
                        database = "unavailable",
                        message = "Database connection failed",
                        timestamp = DateTime.UtcNow
                    };
                }

                // Test if we can execute a simple query
                var messageCount = await _context.Messages.CountAsync();
                
                _logger.LogInformation("Database health check successful. Message count: {MessageCount}", messageCount);
                return new
                {
                    status = "ok",
                    service = "backend-messages",
                    database = "available",
                    messageCount = messageCount,
                    timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database health check");
                return new
                {
                    status = "error",
                    service = "backend-messages",
                    database = "error",
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                };
            }
        }
    }
}
