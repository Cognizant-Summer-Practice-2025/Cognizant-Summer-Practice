using BackendMessages.Config;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace backend_messages.tests.Config
{
    public class HttpClientConfigurationTests
    {
        [Fact]
        public void AddHttpClientConfiguration_ShouldRegisterFactoryAndNamedClient()
        {
            var dict = new Dictionary<string, string?>
            {
                {"HttpClient:Timeout", "00:00:05"},
                {"HttpClient:UserAgent", "TestAgent/1.0"},
                {"HttpClient:MaxConnectionsPerServer", "20"},
                {"HttpClient:UseCookies", "false"},
                {"HttpClient:HandlerLifetime", "00:10:00"}
            };
            var cfg = new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
            var services = new ServiceCollection();

            services.AddHttpClientConfiguration(cfg);
            var provider = services.BuildServiceProvider();

            var factory = provider.GetRequiredService<IHttpClientFactory>();
            var client = factory.CreateClient(HttpClientConfiguration.UserServiceClientName);
            client.Should().NotBeNull();
        }
    }
}


