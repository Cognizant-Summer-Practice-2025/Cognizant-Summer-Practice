using BackendMessages.Config;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Xunit;

namespace BackendMessages.Tests.Config
{
    public class DatabaseConfigurationTests
    {
        private readonly IServiceCollection _services;

        public DatabaseConfigurationTests()
        {
            _services = new ServiceCollection();
        }

        [Fact]
        public void AddDatabaseServices_WithValidConfiguration_ShouldAddDbContext()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Database_Messages"] = "Host=localhost;Database=testdb;Username=test;Password=test"
                })
                .Build();

            // Act
            var result = _services.AddDatabaseServices(configuration);

            // Assert
            result.Should().NotBeNull();
            _services.Should().NotBeEmpty();
        }

        [Fact]
        public void AddDatabaseServices_WithNullConfiguration_ShouldThrowException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => 
                _services.AddDatabaseServices(null!));
        }

        [Fact]
        public void AddDatabaseServices_WithEmptyConnectionString_ShouldAddServices()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Database_Messages"] = ""
                })
                .Build();

            // Act
            var result = _services.AddDatabaseServices(configuration);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void AddDatabaseServices_WithNullConnectionString_ShouldThrowException()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Database_Messages"] = null
                })
                .Build();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => 
                _services.AddDatabaseServices(configuration));
        }

        [Theory]
        [InlineData("Host=localhost;Database=messages;Username=user;Password=pass")]
        [InlineData("Server=.;Database=MessagesDB;Trusted_Connection=true")]
        [InlineData("Data Source=localhost;Initial Catalog=Messages;Integrated Security=true")]
        public void AddDatabaseServices_WithVariousConnectionStrings_ShouldAddServices(string connectionString)
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Database_Messages"] = connectionString
                })
                .Build();

            // Act
            var result = _services.AddDatabaseServices(configuration);

            // Assert
            result.Should().NotBeNull();
            _services.Should().NotBeEmpty();
        }

        [Fact]
        public void AddDatabaseServices_ShouldReturnSameServiceCollection()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Database_Messages"] = "Host=localhost;Database=testdb"
                })
                .Build();

            // Act
            var result = _services.AddDatabaseServices(configuration);

            // Assert
            result.Should().BeSameAs(_services);
        }
    }
}


