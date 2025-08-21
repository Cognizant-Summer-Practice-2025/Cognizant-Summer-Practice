using BackendMessages.Config;
using BackendMessages.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace backend_messages.tests.Config
{
    public class DatabaseConfigurationTests
    {
        [Fact]
        public void AddDatabaseServices_WithValidConnection_ShouldRegisterDbContext()
        {
            var dict = new Dictionary<string, string?>
            {
                {"ConnectionStrings:Database_Messages", "Host=localhost;Username=test;Password=test;Database=test"}
            };
            var cfg = new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
            var services = new ServiceCollection();

            services.AddDatabaseServices(cfg);

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<MessagesDbContext>().Should().NotBeNull();
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


