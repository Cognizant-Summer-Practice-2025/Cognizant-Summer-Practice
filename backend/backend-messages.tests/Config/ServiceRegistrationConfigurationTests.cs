using System.Linq;
using BackendMessages.Config;
using BackendMessages.Repositories;
using BackendMessages.Services;
using BackendMessages.Services.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace backend_messages.tests.Config
{
    public class ServiceRegistrationConfigurationTests
    {
        [Fact]
        public void AddApplicationServices_ShouldRegisterCoreServices()
        {
            var services = new ServiceCollection();

            services.AddLogging();
            services.AddApplicationServices();

            services.Any(sd => sd.ServiceType == typeof(IMessageRepository)).Should().BeTrue();
            services.Any(sd => sd.ServiceType == typeof(IConversationRepository)).Should().BeTrue();
            services.Any(sd => sd.ServiceType == typeof(IMessageReportRepository)).Should().BeTrue();
            services.Any(sd => sd.ServiceType == typeof(IConversationService)).Should().BeTrue();
            services.Any(sd => sd.ServiceType == typeof(IMessageService)).Should().BeTrue();
            services.Any(sd => sd.ServiceType == typeof(IUserAuthenticationService)).Should().BeTrue();
        }
    }
}


