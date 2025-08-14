using System.Linq;
using backend_AI.Config;
using backend_AI.Services;
using backend_AI.Services.Abstractions;
using backend_AI.Services.External;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace backend_AI.tests.Config
{
    public class ServiceRegistrationConfigurationTests
    {
        [Fact]
        public void AddApplicationServices_ShouldRegisterCoreServices()
        {
            var services = new ServiceCollection();

            services.AddApplicationServices();

            services.Any(sd => sd.ServiceType == typeof(IAiChatService)).Should().BeTrue();
            services.Any(sd => sd.ServiceType == typeof(IPortfolioRankingService)).Should().BeTrue();
            services.Any(sd => sd.ServiceType == typeof(IPortfolioApiClient)).Should().BeTrue();
            services.Any(sd => sd.ServiceType == typeof(IUserAuthenticationService)).Should().BeTrue();
            services.Any(sd => sd.ServiceType == typeof(ISecurityHeadersService)).Should().BeTrue();
            services.Any(sd => sd.ServiceType == typeof(IAuthorizationPathService)).Should().BeTrue();
            services.Any(sd => sd.ServiceType == typeof(IAuthenticationStrategy)).Should().BeTrue();
            services.Any(sd => sd.ServiceType == typeof(IAuthenticationContextService)).Should().BeTrue();
        }
    }
}


