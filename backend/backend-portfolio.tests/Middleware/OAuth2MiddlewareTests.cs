using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using backend_portfolio.Middleware;
using backend_portfolio.Services.Abstractions;

namespace backend_portfolio.tests.Middleware
{
    public class OAuth2MiddlewareTests
    {
        private readonly Mock<RequestDelegate> _mockNext;
        private readonly Mock<ILogger<OAuth2Middleware>> _mockLogger;
        private readonly Mock<IUserAuthenticationService> _mockUserAuthService;
        private readonly OAuth2Middleware _middleware;

        public OAuth2MiddlewareTests()
        {
            _mockNext = new Mock<RequestDelegate>();
            _mockLogger = new Mock<ILogger<OAuth2Middleware>>();
            _mockUserAuthService = new Mock<IUserAuthenticationService>();
            _middleware = new OAuth2Middleware(_mockNext.Object, _mockLogger.Object);
        }

        private static HttpContext CreateHttpContext()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/api/test";
            context.Request.Method = "POST";
            return context;
        }

        [Fact]
        public async Task InvokeAsync_WithValidToken_ShouldCallNext()
        {
            // Arrange
            var context = CreateHttpContext();
            context.Request.Headers["Authorization"] = "Bearer valid-token";
            
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user-123"),
                new Claim(ClaimTypes.Email, "user@example.com")
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
            
            _mockUserAuthService.Setup(x => x.ValidateTokenAsync("valid-token"))
                .ReturnsAsync(principal);
            _mockNext.Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(context, _mockUserAuthService.Object);

            // Assert
            _mockNext.Verify(x => x(context), Times.Once);
            context.User.Should().Be(principal);
        }

        [Fact]
        public async Task InvokeAsync_WithInvalidToken_ShouldReturn401()
        {
            // Arrange
            var context = CreateHttpContext();
            context.Request.Headers["Authorization"] = "Bearer invalid-token";
            
            _mockUserAuthService.Setup(x => x.ValidateTokenAsync("invalid-token"))
                .ReturnsAsync((ClaimsPrincipal)null!);

            // Act
            await _middleware.InvokeAsync(context, _mockUserAuthService.Object);

            // Assert
            context.Response.StatusCode.Should().Be(401);
            _mockNext.Verify(x => x(It.IsAny<HttpContext>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_WithMissingAuthorizationHeader_ShouldReturn401()
        {
            // Arrange
            var context = CreateHttpContext();

            // Act
            await _middleware.InvokeAsync(context, _mockUserAuthService.Object);

            // Assert
            context.Response.StatusCode.Should().Be(401);
            _mockNext.Verify(x => x(It.IsAny<HttpContext>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_WithNonBearerScheme_ShouldReturn401()
        {
            // Arrange
            var context = CreateHttpContext();
            context.Request.Headers["Authorization"] = "Basic dXNlcjpwYXNz";

            // Act
            await _middleware.InvokeAsync(context, _mockUserAuthService.Object);

            // Assert
            context.Response.StatusCode.Should().Be(401);
            _mockNext.Verify(x => x(It.IsAny<HttpContext>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_WithEmptyToken_ShouldReturn401()
        {
            // Arrange
            var context = CreateHttpContext();
            context.Request.Headers["Authorization"] = "Bearer ";

            // Act
            await _middleware.InvokeAsync(context, _mockUserAuthService.Object);

            // Assert
            context.Response.StatusCode.Should().Be(401);
            _mockNext.Verify(x => x(It.IsAny<HttpContext>()), Times.Never);
        }

        [Theory]
        [InlineData("/openapi")]
        [InlineData("/swagger")]
        [InlineData("/")]
        [InlineData("/health")]
        [InlineData("/api/portfolio")]
        [InlineData("/api/portfoliotemplate")]
        [InlineData("/api/project")]
        [InlineData("/api/bookmark")]
        [InlineData("/api/image")]
        public async Task InvokeAsync_WithPublicEndpoints_ShouldSkipAuth(string path)
        {
            // Arrange
            var context = CreateHttpContext();
            context.Request.Path = path;
            context.Request.Method = "GET";
            
            _mockNext.Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(context, _mockUserAuthService.Object);

            // Assert
            _mockNext.Verify(x => x(context), Times.Once);
            _mockUserAuthService.Verify(x => x.ValidateTokenAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_WithNullToken_ShouldReturn401()
        {
            // Arrange
            var context = CreateHttpContext();
            context.Request.Headers["Authorization"] = "Bearer";

            // Act
            await _middleware.InvokeAsync(context, _mockUserAuthService.Object);

            // Assert
            context.Response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task InvokeAsync_WhenUserServiceThrows_ShouldReturn401()
        {
            // Arrange
            var context = CreateHttpContext();
            context.Request.Headers["Authorization"] = "Bearer valid-token";
            
            _mockUserAuthService.Setup(x => x.ValidateTokenAsync("valid-token"))
                .ThrowsAsync(new Exception("Service error"));

            // Act
            await _middleware.InvokeAsync(context, _mockUserAuthService.Object);

            // Assert
            context.Response.StatusCode.Should().Be(401);
            _mockNext.Verify(x => x(It.IsAny<HttpContext>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_WhenNextThrows_ShouldPropagateException()
        {
            // Arrange
            var context = CreateHttpContext();
            context.Request.Headers["Authorization"] = "Bearer valid-token";
            
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
            
            _mockUserAuthService.Setup(x => x.ValidateTokenAsync("valid-token"))
                .ReturnsAsync(principal);
            
            var expectedException = new InvalidOperationException("Next middleware error");
            _mockNext.Setup(x => x(It.IsAny<HttpContext>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _middleware.InvokeAsync(context, _mockUserAuthService.Object));
            
            thrownException.Should().Be(expectedException);
        }

        [Fact]
        public async Task InvokeAsync_WithCaseInsensitiveBearerScheme_ShouldCallNext()
        {
            // Arrange
            var context = CreateHttpContext();
            context.Request.Headers["Authorization"] = "BEARER valid-token";
            
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user-123") };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
            
            _mockUserAuthService.Setup(x => x.ValidateTokenAsync("valid-token"))
                .ReturnsAsync(principal);
            _mockNext.Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(context, _mockUserAuthService.Object);

            // Assert
            _mockNext.Verify(x => x(context), Times.Once);
        }

        [Theory]
        [InlineData("Bearer ")]
        [InlineData("Bearer\t")]
        [InlineData("Bearer\n")]
        [InlineData("Bearer    ")]
        public async Task InvokeAsync_WithWhitespaceOnlyToken_ShouldReturn401(string authHeader)
        {
            // Arrange
            var context = CreateHttpContext();
            context.Request.Headers["Authorization"] = authHeader;

            // Act
            await _middleware.InvokeAsync(context, _mockUserAuthService.Object);

            // Assert
            context.Response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task InvokeAsync_WithValidTokenAndUserClaims_ShouldSetUserContext()
        {
            // Arrange
            var context = CreateHttpContext();
            context.Request.Headers["Authorization"] = "Bearer valid-token";
            
            var expectedUserId = "user-123";
            var expectedEmail = "user@example.com";
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, expectedUserId),
                new Claim(ClaimTypes.Email, expectedEmail)
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
            
            _mockUserAuthService.Setup(x => x.ValidateTokenAsync("valid-token"))
                .ReturnsAsync(principal);
            _mockNext.Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(context, _mockUserAuthService.Object);

            // Assert
            context.User.Should().NotBeNull();
            context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(expectedUserId);
            context.User.FindFirst(ClaimTypes.Email)?.Value.Should().Be(expectedEmail);
            _mockNext.Verify(x => x(context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_ShouldAddSecurityHeaders()
        {
            // Arrange
            var context = CreateHttpContext();
            context.Request.Path = "/";  // Public endpoint
            
            _mockNext.Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(context, _mockUserAuthService.Object);
            
            // Simulate response start to trigger OnStarting callback
            await context.Response.StartAsync();

            // Assert
            context.Response.Headers.Should().ContainKey("Cross-Origin-Resource-Policy");
            context.Response.Headers.Should().ContainKey("X-Content-Type-Options");
        }

        [Theory]
        [InlineData("/api/portfolio/123/view", "POST")]
        [InlineData("/api/portfoliotemplate/seed", "POST")]
        public async Task InvokeAsync_WithSpecificPublicPosts_ShouldSkipAuth(string path, string method)
        {
            // Arrange
            var context = CreateHttpContext();
            context.Request.Path = path;
            context.Request.Method = method;
            
            _mockNext.Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(context, _mockUserAuthService.Object);

            // Assert
            _mockNext.Verify(x => x(context), Times.Once);
            _mockUserAuthService.Verify(x => x.ValidateTokenAsync(It.IsAny<string>()), Times.Never);
        }
    }
} 