using System;
using backend_portfolio.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend_backend_portfolio.tests.Services
{
    public class AirflowAuthorizationServiceTests
    {
        private AirflowAuthorizationService CreateService()
        {
            var logger = new Mock<ILogger<AirflowAuthorizationService>>();
            return new AirflowAuthorizationService(logger.Object);
        }

        [Fact]
        public void IsServiceToServiceCall_ShouldReturnTrue_ForLocalhostOrHeader()
        {
            // Localhost host
            var service = CreateService();
            var ctx1 = new DefaultHttpContext();
            ctx1.Request.Host = new HostString("localhost");
            service.IsServiceToServiceCall(ctx1).Should().BeTrue();

            // Header present
            var ctx2 = new DefaultHttpContext();
            ctx2.Request.Headers["X-Service-Name"] = "backend-AI";
            service.IsServiceToServiceCall(ctx2).Should().BeTrue();
        }

        [Fact]
        public void IsServiceToServiceCall_ShouldReturnFalse_WhenExternalNoHeader()
        {
            var service = CreateService();
            var ctx = new DefaultHttpContext();
            ctx.Request.Host = new HostString("example.com");
            service.IsServiceToServiceCall(ctx).Should().BeFalse();
        }

        [Fact]
        public void IsAuthorizedExternalCall_ShouldReturnTrue_WhenBearerMatchesEnv()
        {
            var service = CreateService();
            Environment.SetEnvironmentVariable("AIRFLOW_SECRET", "secret");
            var ctx = new DefaultHttpContext();
            ctx.Request.Headers["Authorization"] = "Bearer secret";
            service.IsAuthorizedExternalCall(ctx).Should().BeTrue();
        }

        [Fact]
        public void IsAuthorizedExternalCall_ShouldReturnTrue_WhenHeaderMatchesEnv()
        {
            var service = CreateService();
            Environment.SetEnvironmentVariable("AIRFLOW_SECRET", "secret");
            var ctx = new DefaultHttpContext();
            ctx.Request.Headers["X-Airflow-Secret"] = "secret";
            service.IsAuthorizedExternalCall(ctx).Should().BeTrue();
        }

        [Fact]
        public void IsAuthorizedExternalCall_ShouldReturnFalse_WhenEnvMissing()
        {
            var service = CreateService();
            Environment.SetEnvironmentVariable("AIRFLOW_SECRET", null);
            var ctx = new DefaultHttpContext();
            service.IsAuthorizedExternalCall(ctx).Should().BeFalse();
        }

        [Fact]
        public void IsAuthorizedExternalCall_ShouldReturnFalse_WhenNoMatch()
        {
            var service = CreateService();
            Environment.SetEnvironmentVariable("AIRFLOW_SECRET", "secret");
            var ctx = new DefaultHttpContext();
            ctx.Request.Headers["Authorization"] = "Bearer wrong";
            service.IsAuthorizedExternalCall(ctx).Should().BeFalse();
        }
    }
}


