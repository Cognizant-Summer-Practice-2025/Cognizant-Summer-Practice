using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using backend_user.Services;
using backend_user.Services.Abstractions;
using backend_user.Models;

namespace backend_user.tests.Services
{
    public class AuthenticationContextServiceTests
    {
        private readonly Mock<ILogger<AuthenticationContextService>> _mockLogger;
        private readonly DefaultHttpContext _context;

        public AuthenticationContextServiceTests()
        {
            _mockLogger = new Mock<ILogger<AuthenticationContextService>>();
            _context = new DefaultHttpContext();
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenStrategiesIsNull()
        {
            // Act & Assert
            var act = () => new AuthenticationContextService(null!, _mockLogger.Object);
            act.Should().Throw<ArgumentNullException>().WithParameterName("strategies");
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            // Arrange
            var strategies = new List<IAuthenticationStrategy>();

            // Act & Assert
            var act = () => new AuthenticationContextService(strategies, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnNull_WhenContextIsNull()
        {
            // Arrange
            var strategies = new List<IAuthenticationStrategy>();
            var service = new AuthenticationContextService(strategies, _mockLogger.Object);

            // Act
            var result = await service.AuthenticateAsync(null!);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnNull_WhenNoStrategiesAvailable()
        {
            // Arrange
            var strategies = new List<IAuthenticationStrategy>();
            var service = new AuthenticationContextService(strategies, _mockLogger.Object);

            // Act
            var result = await service.AuthenticateAsync(_context);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnNull_WhenNoStrategyCanHandle()
        {
            // Arrange
            var mockStrategy1 = new Mock<IAuthenticationStrategy>();
            var mockStrategy2 = new Mock<IAuthenticationStrategy>();
            
            mockStrategy1.Setup(x => x.CanHandle(_context)).Returns(false);
            mockStrategy2.Setup(x => x.CanHandle(_context)).Returns(false);

            var strategies = new List<IAuthenticationStrategy> { mockStrategy1.Object, mockStrategy2.Object };
            var service = new AuthenticationContextService(strategies, _mockLogger.Object);

            // Act
            var result = await service.AuthenticateAsync(_context);

            // Assert
            result.Should().BeNull();
            mockStrategy1.Verify(x => x.CanHandle(_context), Times.Once);
            mockStrategy2.Verify(x => x.CanHandle(_context), Times.Once);
            mockStrategy1.Verify(x => x.AuthenticateAsync(It.IsAny<HttpContext>()), Times.Never);
            mockStrategy2.Verify(x => x.AuthenticateAsync(It.IsAny<HttpContext>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnUser_WhenFirstStrategySucceeds()
        {
            // Arrange
            var expectedUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser"
            };

            var mockStrategy1 = new Mock<IAuthenticationStrategy>();
            var mockStrategy2 = new Mock<IAuthenticationStrategy>();

            mockStrategy1.Setup(x => x.CanHandle(_context)).Returns(true);
            mockStrategy1.Setup(x => x.AuthenticateAsync(_context)).ReturnsAsync(expectedUser);
            mockStrategy2.Setup(x => x.CanHandle(_context)).Returns(true);

            var strategies = new List<IAuthenticationStrategy> { mockStrategy1.Object, mockStrategy2.Object };
            var service = new AuthenticationContextService(strategies, _mockLogger.Object);

            // Act
            var result = await service.AuthenticateAsync(_context);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedUser);
            mockStrategy1.Verify(x => x.CanHandle(_context), Times.Once);
            mockStrategy1.Verify(x => x.AuthenticateAsync(_context), Times.Once);
            mockStrategy2.Verify(x => x.CanHandle(_context), Times.Never);
            mockStrategy2.Verify(x => x.AuthenticateAsync(It.IsAny<HttpContext>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldTrySecondStrategy_WhenFirstStrategyFails()
        {
            // Arrange
            var expectedUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser"
            };

            var mockStrategy1 = new Mock<IAuthenticationStrategy>();
            var mockStrategy2 = new Mock<IAuthenticationStrategy>();

            mockStrategy1.Setup(x => x.CanHandle(_context)).Returns(true);
            mockStrategy1.Setup(x => x.AuthenticateAsync(_context)).ReturnsAsync((User?)null);
            mockStrategy2.Setup(x => x.CanHandle(_context)).Returns(true);
            mockStrategy2.Setup(x => x.AuthenticateAsync(_context)).ReturnsAsync(expectedUser);

            var strategies = new List<IAuthenticationStrategy> { mockStrategy1.Object, mockStrategy2.Object };
            var service = new AuthenticationContextService(strategies, _mockLogger.Object);

            // Act
            var result = await service.AuthenticateAsync(_context);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedUser);
            mockStrategy1.Verify(x => x.CanHandle(_context), Times.Once);
            mockStrategy1.Verify(x => x.AuthenticateAsync(_context), Times.Once);
            mockStrategy2.Verify(x => x.CanHandle(_context), Times.Once);
            mockStrategy2.Verify(x => x.AuthenticateAsync(_context), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldSkipStrategy_WhenCanHandleReturnsFalse()
        {
            // Arrange
            var expectedUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser"
            };

            var mockStrategy1 = new Mock<IAuthenticationStrategy>();
            var mockStrategy2 = new Mock<IAuthenticationStrategy>();

            mockStrategy1.Setup(x => x.CanHandle(_context)).Returns(false);
            mockStrategy2.Setup(x => x.CanHandle(_context)).Returns(true);
            mockStrategy2.Setup(x => x.AuthenticateAsync(_context)).ReturnsAsync(expectedUser);

            var strategies = new List<IAuthenticationStrategy> { mockStrategy1.Object, mockStrategy2.Object };
            var service = new AuthenticationContextService(strategies, _mockLogger.Object);

            // Act
            var result = await service.AuthenticateAsync(_context);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedUser);
            mockStrategy1.Verify(x => x.CanHandle(_context), Times.Once);
            mockStrategy1.Verify(x => x.AuthenticateAsync(It.IsAny<HttpContext>()), Times.Never);
            mockStrategy2.Verify(x => x.CanHandle(_context), Times.Once);
            mockStrategy2.Verify(x => x.AuthenticateAsync(_context), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldContinueToNextStrategy_WhenCurrentStrategyThrowsException()
        {
            // Arrange
            var expectedUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser"
            };

            var mockStrategy1 = new Mock<IAuthenticationStrategy>();
            var mockStrategy2 = new Mock<IAuthenticationStrategy>();

            mockStrategy1.Setup(x => x.CanHandle(_context)).Returns(true);
            mockStrategy1.Setup(x => x.AuthenticateAsync(_context)).ThrowsAsync(new Exception("Authentication failed"));
            mockStrategy2.Setup(x => x.CanHandle(_context)).Returns(true);
            mockStrategy2.Setup(x => x.AuthenticateAsync(_context)).ReturnsAsync(expectedUser);

            var strategies = new List<IAuthenticationStrategy> { mockStrategy1.Object, mockStrategy2.Object };
            var service = new AuthenticationContextService(strategies, _mockLogger.Object);

            // Act
            var result = await service.AuthenticateAsync(_context);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedUser);
            mockStrategy1.Verify(x => x.CanHandle(_context), Times.Once);
            mockStrategy1.Verify(x => x.AuthenticateAsync(_context), Times.Once);
            mockStrategy2.Verify(x => x.CanHandle(_context), Times.Once);
            mockStrategy2.Verify(x => x.AuthenticateAsync(_context), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnNull_WhenAllStrategiesFail()
        {
            // Arrange
            var mockStrategy1 = new Mock<IAuthenticationStrategy>();
            var mockStrategy2 = new Mock<IAuthenticationStrategy>();

            mockStrategy1.Setup(x => x.CanHandle(_context)).Returns(true);
            mockStrategy1.Setup(x => x.AuthenticateAsync(_context)).ReturnsAsync((User?)null);
            mockStrategy2.Setup(x => x.CanHandle(_context)).Returns(true);
            mockStrategy2.Setup(x => x.AuthenticateAsync(_context)).ReturnsAsync((User?)null);

            var strategies = new List<IAuthenticationStrategy> { mockStrategy1.Object, mockStrategy2.Object };
            var service = new AuthenticationContextService(strategies, _mockLogger.Object);

            // Act
            var result = await service.AuthenticateAsync(_context);

            // Assert
            result.Should().BeNull();
            mockStrategy1.Verify(x => x.CanHandle(_context), Times.Once);
            mockStrategy1.Verify(x => x.AuthenticateAsync(_context), Times.Once);
            mockStrategy2.Verify(x => x.CanHandle(_context), Times.Once);
            mockStrategy2.Verify(x => x.AuthenticateAsync(_context), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnNull_WhenAllStrategiesThrowExceptions()
        {
            // Arrange
            var mockStrategy1 = new Mock<IAuthenticationStrategy>();
            var mockStrategy2 = new Mock<IAuthenticationStrategy>();

            mockStrategy1.Setup(x => x.CanHandle(_context)).Returns(true);
            mockStrategy1.Setup(x => x.AuthenticateAsync(_context)).ThrowsAsync(new Exception("Strategy 1 failed"));
            mockStrategy2.Setup(x => x.CanHandle(_context)).Returns(true);
            mockStrategy2.Setup(x => x.AuthenticateAsync(_context)).ThrowsAsync(new Exception("Strategy 2 failed"));

            var strategies = new List<IAuthenticationStrategy> { mockStrategy1.Object, mockStrategy2.Object };
            var service = new AuthenticationContextService(strategies, _mockLogger.Object);

            // Act
            var result = await service.AuthenticateAsync(_context);

            // Assert
            result.Should().BeNull();
            mockStrategy1.Verify(x => x.CanHandle(_context), Times.Once);
            mockStrategy1.Verify(x => x.AuthenticateAsync(_context), Times.Once);
            mockStrategy2.Verify(x => x.CanHandle(_context), Times.Once);
            mockStrategy2.Verify(x => x.AuthenticateAsync(_context), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldProcessStrategiesInOrder()
        {
            // Arrange
            var callOrder = new List<string>();
            var expectedUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Username = "testuser"
            };

            var mockStrategy1 = new Mock<IAuthenticationStrategy>();
            var mockStrategy2 = new Mock<IAuthenticationStrategy>();
            var mockStrategy3 = new Mock<IAuthenticationStrategy>();

            mockStrategy1.Setup(x => x.CanHandle(_context)).Returns(true).Callback(() => callOrder.Add("Strategy1-CanHandle"));
            mockStrategy1.Setup(x => x.AuthenticateAsync(_context)).ReturnsAsync((User?)null).Callback(() => callOrder.Add("Strategy1-Authenticate"));
            
            mockStrategy2.Setup(x => x.CanHandle(_context)).Returns(true).Callback(() => callOrder.Add("Strategy2-CanHandle"));
            mockStrategy2.Setup(x => x.AuthenticateAsync(_context)).ReturnsAsync(expectedUser).Callback(() => callOrder.Add("Strategy2-Authenticate"));
            
            mockStrategy3.Setup(x => x.CanHandle(_context)).Returns(true).Callback(() => callOrder.Add("Strategy3-CanHandle"));

            var strategies = new List<IAuthenticationStrategy> { mockStrategy1.Object, mockStrategy2.Object, mockStrategy3.Object };
            var service = new AuthenticationContextService(strategies, _mockLogger.Object);

            // Act
            var result = await service.AuthenticateAsync(_context);

            // Assert
            result.Should().NotBeNull();
            callOrder.Should().Equal("Strategy1-CanHandle", "Strategy1-Authenticate", "Strategy2-CanHandle", "Strategy2-Authenticate");
            mockStrategy3.Verify(x => x.CanHandle(_context), Times.Never);
            mockStrategy3.Verify(x => x.AuthenticateAsync(It.IsAny<HttpContext>()), Times.Never);
        }
    }
}
