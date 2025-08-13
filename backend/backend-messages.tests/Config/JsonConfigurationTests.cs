using System.Text.Json;
using BackendMessages.Config;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace backend_messages.tests.Config
{
    public class JsonConfigurationTests
    {
        [Fact]
        public void AddJsonConfiguration_ShouldConfigureControllers()
        {
            var services = new ServiceCollection();

            services.AddJsonConfiguration();

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<Microsoft.AspNetCore.Mvc.JsonOptions>>().Value;
            options.JsonSerializerOptions.PropertyNamingPolicy.Should().Be(JsonNamingPolicy.CamelCase);
            options.JsonSerializerOptions.WriteIndented.Should().BeFalse();
        }
    }
}


