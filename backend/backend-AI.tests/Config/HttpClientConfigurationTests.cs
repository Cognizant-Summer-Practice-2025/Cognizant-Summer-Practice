using backend_AI.Config;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace backend_AI.tests.Config
{
    public class HttpClientConfigurationTests
    {
        [Fact]
        public void AddHttpClientConfiguration_ShouldRegisterFactoryAndNamedClients()
        {
            var dict = new Dictionary<string, string?>
            {
                {"HttpClient:UserAgent", "TestAgent/1.0"},
                {"HttpClient:AIProviderTimeout", "00:00:05"},
                {"HttpClient:AIProviderMaxConnections", "5"},
                {"HttpClient:PortfolioServiceTimeout", "00:00:07"},
                {"HttpClient:PortfolioServiceMaxConnections", "10"},
                {"HttpClient:UserServiceTimeout", "00:00:03"},
                {"HttpClient:UserServiceMaxConnections", "12"},
                {"HttpClient:HandlerLifetime", "00:10:00"}
            };
            var cfg = new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
            var services = new ServiceCollection();

            services.AddHttpClientConfiguration(cfg);
            var provider = services.BuildServiceProvider();

            var factory = provider.GetRequiredService<IHttpClientFactory>();
            factory.CreateClient(HttpClientConfiguration.AIProviderClientName).Should().NotBeNull();
            factory.CreateClient(HttpClientConfiguration.PortfolioServiceClientName).Should().NotBeNull();
            factory.CreateClient(HttpClientConfiguration.UserServiceClientName).Should().NotBeNull();
        }

        [Fact]
        public void AddHttpClientConfiguration_AIProviderTimeout_FromEnvOverridesConfig()
        {
            try
            {
                Environment.SetEnvironmentVariable("OPENROUTER_TIMEOUT_SECONDS", "2");
                var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"OpenRouter:TimeoutSeconds", "10"}
                }!).Build();
                var services = new ServiceCollection();

                services.AddHttpClientConfiguration(cfg);
                var provider = services.BuildServiceProvider();
                var factory = provider.GetRequiredService<IHttpClientFactory>();

                var client = factory.CreateClient(HttpClientConfiguration.AIProviderClientName);
                client.Timeout.Should().Be(TimeSpan.FromSeconds(2));
            }
            finally
            {
                Environment.SetEnvironmentVariable("OPENROUTER_TIMEOUT_SECONDS", null);
            }
        }

        [Fact]
        public void AddHttpClientConfiguration_AIProviderTimeout_FromConfigWhenEnvMissing()
        {
            try
            {
                Environment.SetEnvironmentVariable("OPENROUTER_TIMEOUT_SECONDS", null);
                var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"OpenRouter:TimeoutSeconds", "9"}
                }!).Build();
                var services = new ServiceCollection();

                services.AddHttpClientConfiguration(cfg);
                var provider = services.BuildServiceProvider();
                var factory = provider.GetRequiredService<IHttpClientFactory>();
                var client = factory.CreateClient(HttpClientConfiguration.AIProviderClientName);
                client.Timeout.Should().Be(TimeSpan.FromSeconds(9));
            }
            finally
            {
                Environment.SetEnvironmentVariable("OPENROUTER_TIMEOUT_SECONDS", null);
            }
        }

        [Fact]
        public void AddHttpClientConfiguration_AIProviderTimeout_DefaultWhenNoEnvOrConfig()
        {
            try
            {
                Environment.SetEnvironmentVariable("OPENROUTER_TIMEOUT_SECONDS", null);
                var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()!).Build();
                var services = new ServiceCollection();

                services.AddHttpClientConfiguration(cfg);
                var provider = services.BuildServiceProvider();
                var factory = provider.GetRequiredService<IHttpClientFactory>();
                var client = factory.CreateClient(HttpClientConfiguration.AIProviderClientName);
                client.Timeout.Should().Be(TimeSpan.FromMinutes(2));
            }
            finally
            {
                Environment.SetEnvironmentVariable("OPENROUTER_TIMEOUT_SECONDS", null);
            }
        }
    }
}


