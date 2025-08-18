using backend_portfolio.Config;
using backend_portfolio.Data;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Xunit;

namespace backend_portfolio.tests.Config
{
    public class DatabaseConfigurationTests
    {
        [Fact]
        public void AddDatabaseServices_WithValidConnection_ShouldRegisterDbContextAndDataSource()
        {
            var dict = new Dictionary<string, string?>
            {
                {"ConnectionStrings:Database_Portfolio", "Host=localhost;Username=test;Password=test;Database=test"}
            };
            var cfg = new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
            var services = new ServiceCollection();

            services.AddDatabaseServices(cfg);

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<PortfolioDbContext>().Should().NotBeNull();
            provider.GetRequiredService<NpgsqlDataSource>().Should().NotBeNull();
        }

        [Fact]
        public void AddDatabaseServices_WithoutConnection_ShouldThrow()
        {
            var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>()!).Build();
            var services = new ServiceCollection();

            var act = () => services.AddDatabaseServices(cfg);
            act.Should().Throw<InvalidOperationException>();
        }
    }
}


