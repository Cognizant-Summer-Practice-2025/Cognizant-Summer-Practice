using BackendMessages.Config;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace backend_messages.tests.Config
{
    public class SignalRConfigurationTests
    {
        [Fact]
        public void AddSignalRConfiguration_ShouldRegisterSignalR()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSignalRConfiguration();
            var provider = services.BuildServiceProvider();

            provider.GetRequiredService<Microsoft.AspNetCore.SignalR.IHubContext<BackendMessages.Hubs.MessageHub>>().Should().NotBeNull();
        }

        [Fact]
        public void MapSignalRHubs_ShouldMapHubWithoutThrowing()
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = Environments.Development
            });
            builder.Services.AddLogging();
            builder.Services.AddSignalR();
            var app = builder.Build();

            app.MapSignalRHubs();

            app.Should().NotBeNull();
        }
    }
}


