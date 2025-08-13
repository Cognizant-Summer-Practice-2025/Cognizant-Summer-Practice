using System.Linq;
using backend_portfolio.Config;
using backend_portfolio.Repositories;
using backend_portfolio.Services;
using backend_portfolio.Services.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace backend_portfolio.tests.Config
{
    public class ServiceRegistrationConfigurationTests
    {
        [Fact]
        public void AddApplicationServices_ShouldRegisterCoreServices()
        {
            var services = new ServiceCollection();

            services.AddApplicationServices();

            services.Any(sd => sd.ServiceType == typeof(IPortfolioRepository)).Should().BeTrue();
            services.Any(sd => sd.ServiceType == typeof(IPortfolioMapper)).Should().BeTrue();
            services.Any(sd => sd.ServiceType == typeof(IValidationService<backend_portfolio.DTO.Portfolio.Request.PortfolioCreateRequest>)).Should().BeTrue();
            services.Any(sd => sd.ServiceType == typeof(IExternalUserService)).Should().BeTrue();
            services.Any(sd => sd.ServiceType == typeof(IUserAuthenticationService)).Should().BeTrue();
            services.Any(sd => sd.ServiceType == typeof(ICacheService)).Should().BeTrue();
        }
    }
}


