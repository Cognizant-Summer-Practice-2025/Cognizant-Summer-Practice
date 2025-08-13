using System.Linq;
using backend_user.Config;
using backend_user.Repositories;
using backend_user.Services;
using backend_user.Services.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace backend_user.tests.Config
{
    public class ServiceRegistrationConfigurationTests
    {
        [Fact]
        public void AddApplicationServices_ShouldRegisterCoreServices()
        {
            var services = new ServiceCollection();

            services.AddApplicationServices();

            services.Any(sd => sd.ServiceType == typeof(IUserRepository)).Should().BeTrue();
            services.Any(sd => sd.ServiceType == typeof(IOAuthProviderRepository)).Should().BeTrue();
            services.Any(sd => sd.ServiceType == typeof(IUserService)).Should().BeTrue();
            services.Any(sd => sd.ServiceType == typeof(IAuthenticationService)).Should().BeTrue();
            services.Any(sd => sd.ServiceType == typeof(IAuthenticationContextService)).Should().BeTrue();
        }
    }
}


