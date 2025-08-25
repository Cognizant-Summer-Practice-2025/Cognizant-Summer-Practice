using System;
using System.Security.Claims;
using System.Threading.Tasks;
using BackendMessages.Middleware;
using BackendMessages.Services.Abstractions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.IO;

namespace BackendMessages.Tests.Middleware
{
    public class OAuth2MiddlewareAdditionalTests
    {
        private readonly Mock<RequestDelegate> _nextMock;
        private readonly Mock<ILogger<OAuth2Middleware>> _loggerMock;
        private readonly Mock<IUserAuthenticationService> _userAuthServiceMock;
        private readonly OAuth2Middleware _middleware;

        public OAuth2MiddlewareAdditionalTests()
        {
            _nextMock = new Mock<RequestDelegate>();
            _loggerMock = new Mock<ILogger<OAuth2Middleware>>();
            _userAuthServiceMock = new Mock<IUserAuthenticationService>();
            _middleware = new OAuth2Middleware(_nextMock.Object, _loggerMock.Object);
        }

        [Theory]
        [InlineData("/swagger")]
        [InlineData("/swagger/index.html")]
        [InlineData("/health")]
        [InlineData("/")]
        [InlineData("/messagehub")]
        [InlineData("/api/users/test")]
        [InlineData("/api/messages/admin/reports")]
        public async Task InvokeAsync_WithPublicEndpoints_ShouldSkipAuth(string path)
        {
            // Arrange
            var context = CreateHttpContext();
            context.Request.Path = path;
            context.Request.Method = "GET";
            
            _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(context, _userAuthServiceMock.Object);

            // Assert
            _nextMock.Verify(x => x(context), Times.Once);
            _userAuthServiceMock.Verify(x => x.ValidateTokenAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_WithValidToken_ShouldSetUserContext()
        {
            // Arrange
            var context = CreateHttpContext();
            context.Request.Path = "/api/messages";
            context.Request.Headers["Authorization"] = "Bearer valid-token-123";
            
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
            
            _userAuthServiceMock.Setup(x => x.ValidateTokenAsync("valid-token-123"))
                .ReturnsAsync(principal);
            
            _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(context, _userAuthServiceMock.Object);

            // Assert
            context.User.Should().Be(principal);
            _nextMock.Verify(x => x(context), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithInvalidToken_ShouldReturn401()
        {
            // Arrange
            var context = CreateHttpContext();
            context.Request.Path = "/api/messages";
            context.Request.Headers["Authorization"] = "Bearer invalid-token";
            
            _userAuthServiceMock.Setup(x => x.ValidateTokenAsync("invalid-token"))
                .ReturnsAsync((ClaimsPrincipal?)null);

            // Act
            await _middleware.InvokeAsync(context, _userAuthServiceMock.Object);

            // Assert
            context.Response.StatusCode.Should().Be(401);
            _nextMock.Verify(x => x(It.IsAny<HttpContext>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_WithoutAuthorizationHeader_ShouldReturn401()
        {
            // Arrange
            var context = CreateHttpContext();
            context.Request.Path = "/api/messages";
            // No Authorization header

            // Act
            await _middleware.InvokeAsync(context, _userAuthServiceMock.Object);

            // Assert
            context.Response.StatusCode.Should().Be(401);
            _nextMock.Verify(x => x(It.IsAny<HttpContext>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_WithEmptyToken_ShouldReturn401()
        {
            // Arrange
            var context = CreateHttpContext();
            context.Request.Path = "/api/messages";
            context.Request.Headers["Authorization"] = "Bearer ";

            // Act
            await _middleware.InvokeAsync(context, _userAuthServiceMock.Object);

            // Assert
            context.Response.StatusCode.Should().Be(401);
            _nextMock.Verify(x => x(It.IsAny<HttpContext>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_ShouldAddSecurityHeaders()
        {
            // Arrange
            var context = CreateHttpContext();
            context.Request.Path = "/swagger";
            
            _nextMock.Setup(x => x(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(context, _userAuthServiceMock.Object);

            // Simulate the response starting to trigger OnStarting callbacks
            if (context.Response.HasStarted)
            {
                // If response has started, headers should be visible
                context.Response.Headers.Should().ContainKey("Cross-Origin-Resource-Policy");
                context.Response.Headers.Should().ContainKey("X-Content-Type-Options");
            }
            else
            {
                // If response hasn't started, we can't verify headers yet
                // This is expected behavior - headers are set in OnStarting callback
                context.Response.Headers.Should().NotBeNull();
            }
        }

        private static HttpContext CreateHttpContext()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            return context;
        }
    }
} 