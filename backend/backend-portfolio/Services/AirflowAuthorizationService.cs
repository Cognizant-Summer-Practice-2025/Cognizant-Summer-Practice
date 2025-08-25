using System;
using System.Linq;
using backend_portfolio.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace backend_portfolio.Services
{
    public class AirflowAuthorizationService : IAirflowAuthorizationService
    {
        private readonly ILogger<AirflowAuthorizationService> _logger;

        public AirflowAuthorizationService(ILogger<AirflowAuthorizationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool IsServiceToServiceCall(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress;
            var isLocalhost = remoteIp?.Equals(System.Net.IPAddress.Loopback) == true ||
                             remoteIp?.Equals(System.Net.IPAddress.IPv6Loopback) == true ||
                             context.Request.Host.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                             context.Request.Host.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);

            var isFromBackendAI = context.Request.Headers.ContainsKey("X-Service-Name") &&
                                  context.Request.Headers["X-Service-Name"].ToString().Equals("backend-AI", StringComparison.OrdinalIgnoreCase);

            return isLocalhost || isFromBackendAI;
        }

        public bool IsAuthorizedExternalCall(HttpContext context)
        {
            var airflowSecret = Environment.GetEnvironmentVariable("AIRFLOW_SECRET");
            if (string.IsNullOrEmpty(airflowSecret))
            {
                _logger.LogError("AIRFLOW_SECRET environment variable is not configured");
                return false;
            }

            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            var xAirflowSecretHeader = context.Request.Headers["X-Airflow-Secret"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                if (token == airflowSecret)
                {
                    return true;
                }
            }
            else if (!string.IsNullOrEmpty(xAirflowSecretHeader) && xAirflowSecretHeader == airflowSecret)
            {
                return true;
            }

            _logger.LogWarning("Unauthorized external Airflow call attempt");
            return false;
        }
    }
}


