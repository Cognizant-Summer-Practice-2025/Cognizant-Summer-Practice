using BackendMessages.Config;
using FluentAssertions;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace backend_messages.tests.Config
{
    public class CorsConfigurationTests
    {
        [Fact]
        public void AddCorsConfiguration_WithConfiguredUrls_ShouldUseConfigured()
        {
            var dict = new Dictionary<string, string?>
            {
                {"FrontendUrls:0", "http://example.com"},
                {"FrontendUrls:1", "http://another.com"}
            };
            var cfg = new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
            var services = new ServiceCollection();

            services.AddCorsConfiguration(cfg);
            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<CorsOptions>>().Value;

            var defaultName = options.DefaultPolicyName;
            options.GetPolicy(defaultName)!.Origins.Should().BeEquivalentTo(new[]{"http://example.com","http://another.com"});
        }

        [Fact]
        public void AddCorsConfiguration_WithoutConfiguredUrls_ShouldFallbackToDefaults()
        {
            var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>()!).Build();
            var services = new ServiceCollection();

            services.AddCorsConfiguration(cfg);
            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<CorsOptions>>().Value;

            var defaultName = options.DefaultPolicyName;
            options.GetPolicy(defaultName)!.Origins.Should().Contain(new[]{
                "http://localhost:3000",
                "http://localhost:3001",
                "http://localhost:3002",
                "http://localhost:3003"
            });
        }
    }
}


