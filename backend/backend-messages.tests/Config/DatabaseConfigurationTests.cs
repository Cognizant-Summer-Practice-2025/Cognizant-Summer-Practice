using BackendMessages.Config;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace BackendMessages.Tests.Config
{
    public class DatabaseConfigurationTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly IServiceCollection _services;

        public DatabaseConfigurationTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _services = new ServiceCollection();
        }

        [Fact]
        public void AddDatabaseServices_WithValidConfiguration_ShouldAddDbContext()
        {
            // Arrange
            _configurationMock.Setup(x => x.GetConnectionString("DefaultConnection"))
                .Returns("Host=localhost;Database=testdb;Username=test;Password=test");

            // Act
            var result = _services.AddDatabaseServices(_configurationMock.Object);

            // Assert
            result.Should().NotBeNull();
            _services.Should().NotBeEmpty();
        }

        [Fact]
        public void AddDatabaseServices_WithNullConfiguration_ShouldThrowException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                _services.AddDatabaseServices(null!));
        }

        [Fact]
        public void AddDatabaseServices_WithEmptyConnectionString_ShouldStillAddServices()
        {
            // Arrange
            _configurationMock.Setup(x => x.GetConnectionString("DefaultConnection"))
                .Returns("");

            // Act
            var result = _services.AddDatabaseServices(_configurationMock.Object);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void AddDatabaseServices_WithNullConnectionString_ShouldStillAddServices()
        {
            // Arrange
            _configurationMock.Setup(x => x.GetConnectionString("DefaultConnection"))
                .Returns((string?)null);

            // Act
            var result = _services.AddDatabaseServices(_configurationMock.Object);

            // Assert
            result.Should().NotBeNull();
        }

        [Theory]
        [InlineData("Host=localhost;Database=messages;Username=user;Password=pass")]
        [InlineData("Server=.;Database=MessagesDB;Trusted_Connection=true")]
        [InlineData("Data Source=localhost;Initial Catalog=Messages;Integrated Security=true")]
        public void AddDatabaseServices_WithVariousConnectionStrings_ShouldAddServices(string connectionString)
        {
            // Arrange
            _configurationMock.Setup(x => x.GetConnectionString("DefaultConnection"))
                .Returns(connectionString);

            // Act
            var result = _services.AddDatabaseServices(_configurationMock.Object);

            // Assert
            result.Should().NotBeNull();
            _services.Should().NotBeEmpty();
        }

        [Fact]
        public void AddDatabaseServices_ShouldReturnSameServiceCollection()
        {
            // Arrange
            _configurationMock.Setup(x => x.GetConnectionString("DefaultConnection"))
                .Returns("Host=localhost;Database=testdb");

            // Act
            var result = _services.AddDatabaseServices(_configurationMock.Object);

            // Assert
            result.Should().BeSameAs(_services);
        }
    }
}


