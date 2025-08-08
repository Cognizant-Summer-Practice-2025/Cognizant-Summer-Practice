using System.Security.Claims;
using BackendMessages.Middleware;
using BackendMessages.Services.Abstractions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackendMessages.Tests.Middleware
{
    public class OAuth2MiddlewareTests
    {
        private readonly Mock<RequestDelegate> _nextMock;
        private readonly Mock<ILogger<OAuth2Middleware>> _loggerMock;
        private readonly Mock<IUserAuthenticationService> _userAuthServiceMock;
        private readonly OAuth2Middleware _middleware;
        private readonly DefaultHttpContext _httpContext;

        public OAuth2MiddlewareTests()
        {
            _nextMock = new Mock<RequestDelegate>();
            _loggerMock = new Mock<ILogger<OAuth2Middleware>>();
            _userAuthServiceMock = new Mock<IUserAuthenticationService>();
            _middleware = new OAuth2Middleware(_nextMock.Object, _loggerMock.Object);
            _httpContext = new DefaultHttpContext();
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/swagger")]
        [InlineData("/swagger/index.html")]
        [InlineData("/health")]
        [InlineData("/health/ready")]
        [InlineData("/messagehub")]
        [InlineData("/messagehub/negotiate")]
        [InlineData("/api/users/test")]
        public async Task InvokeAsync_WithPublicEndpoints_ShouldSkipAuthAndCallNext(string path)
        {
            // Arrange
            _httpContext.Request.Path = path;
            _httpContext.Request.Method = "GET";

            // Act
            await _middleware.InvokeAsync(_httpContext, _userAuthServiceMock.Object);

            // Assert
            _nextMock.Verify(next => next(_httpContext), Times.Once);
            _userAuthServiceMock.Verify(auth => auth.ValidateTokenAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_WithMissingAuthorizationHeader_ShouldReturn401()
        {
            // Arrange
            _httpContext.Request.Path = "/api/messages";
            _httpContext.Request.Method = "GET";
            _httpContext.Response.Body = new MemoryStream();

            // Act
            await _middleware.InvokeAsync(_httpContext, _userAuthServiceMock.Object);

            // Assert
            _httpContext.Response.StatusCode.Should().Be(401);
            _nextMock.Verify(next => next(_httpContext), Times.Never);
            
            // Verify response body
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(_httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            responseBody.Should().Be("Unauthorized: Missing or invalid Authorization header");
        }

        [Theory]
        [InlineData("")]
        [InlineData("InvalidScheme token")]
        [InlineData("Basic dXNlcjpwYXNz")]
        [InlineData("Token abc123")]
        public async Task InvokeAsync_WithInvalidAuthorizationHeader_ShouldReturn401(string authHeader)
        {
            // Arrange
            _httpContext.Request.Path = "/api/messages";
            _httpContext.Request.Method = "GET";
            _httpContext.Request.Headers.Authorization = authHeader;
            _httpContext.Response.Body = new MemoryStream();

            // Act
            await _middleware.InvokeAsync(_httpContext, _userAuthServiceMock.Object);

            // Assert
            _httpContext.Response.StatusCode.Should().Be(401);
            _nextMock.Verify(next => next(_httpContext), Times.Never);
            
            // Verify response body
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(_httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            responseBody.Should().Be("Unauthorized: Missing or invalid Authorization header");
        }

        [Theory]
        [InlineData("Bearer ")]
        [InlineData("Bearer   ")]
        [InlineData("Bearer \t\n")]
        public async Task InvokeAsync_WithEmptyToken_ShouldReturn401(string authHeader)
        {
            // Arrange
            _httpContext.Request.Path = "/api/messages";
            _httpContext.Request.Method = "GET";
            _httpContext.Request.Headers.Authorization = authHeader;
            _httpContext.Response.Body = new MemoryStream();

            // Act
            await _middleware.InvokeAsync(_httpContext, _userAuthServiceMock.Object);

            // Assert
            _httpContext.Response.StatusCode.Should().Be(401);
            _nextMock.Verify(next => next(_httpContext), Times.Never);
            
            // Verify response body
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(_httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            responseBody.Should().Be("Unauthorized: Empty access token");
        }

        [Fact]
        public async Task InvokeAsync_WithInvalidToken_ShouldReturn401()
        {
            // Arrange
            var token = "invalid-token";
            _httpContext.Request.Path = "/api/messages";
            _httpContext.Request.Method = "GET";
            _httpContext.Request.Headers.Authorization = $"Bearer {token}";
            _httpContext.Response.Body = new MemoryStream();

            _userAuthServiceMock
                .Setup(auth => auth.ValidateTokenAsync(token))
                .ReturnsAsync((ClaimsPrincipal?)null);

            // Act
            await _middleware.InvokeAsync(_httpContext, _userAuthServiceMock.Object);

            // Assert
            _httpContext.Response.StatusCode.Should().Be(401);
            _nextMock.Verify(next => next(_httpContext), Times.Never);
            _userAuthServiceMock.Verify(auth => auth.ValidateTokenAsync(token), Times.Once);
            
            // Verify response body
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(_httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            responseBody.Should().Be("Unauthorized: Invalid or expired access token");
        }

        [Fact]
        public async Task InvokeAsync_WithValidToken_ShouldSetUserContextAndCallNext()
        {
            // Arrange
            var token = "valid-token";
            var userId = Guid.NewGuid().ToString();
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, "test@example.com"),
                new Claim(ClaimTypes.Name, "testuser")
            };
            var identity = new ClaimsIdentity(claims, "Bearer");
            var principal = new ClaimsPrincipal(identity);

            _httpContext.Request.Path = "/api/messages";
            _httpContext.Request.Method = "GET";
            _httpContext.Request.Headers.Authorization = $"Bearer {token}";

            _userAuthServiceMock
                .Setup(auth => auth.ValidateTokenAsync(token))
                .ReturnsAsync(principal);

            // Act
            await _middleware.InvokeAsync(_httpContext, _userAuthServiceMock.Object);

            // Assert
            _httpContext.Response.StatusCode.Should().Be(200);
            _httpContext.User.Should().NotBeNull();
            _httpContext.User.Should().BeSameAs(principal);
            _httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be(userId);
            
            _nextMock.Verify(next => next(_httpContext), Times.Once);
            _userAuthServiceMock.Verify(auth => auth.ValidateTokenAsync(token), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithPublicEndpoint_ShouldAllowRequestToPassThrough()
        {
            // Arrange
            _httpContext.Request.Path = "/";
            _httpContext.Request.Method = "GET";
            _httpContext.Response.Body = new MemoryStream();
            
            // Setup next delegate to verify it's called
            var nextCalled = false;
            _nextMock.Setup(next => next(It.IsAny<HttpContext>()))
                .Returns(Task.CompletedTask)
                .Callback(() => nextCalled = true);

            // Act
            await _middleware.InvokeAsync(_httpContext, _userAuthServiceMock.Object);

            // Assert
            nextCalled.Should().BeTrue("because public endpoints should pass through middleware");
            _httpContext.Response.StatusCode.Should().Be(200);
            _userAuthServiceMock.Verify(auth => auth.ValidateTokenAsync(It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        [InlineData("PATCH")]
        public async Task InvokeAsync_WithValidTokenAndDifferentMethods_ShouldWork(string httpMethod)
        {
            // Arrange
            var token = "valid-token";
            var userId = Guid.NewGuid().ToString();
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, "test@example.com")
            };
            var identity = new ClaimsIdentity(claims, "Bearer");
            var principal = new ClaimsPrincipal(identity);

            _httpContext.Request.Path = "/api/messages";
            _httpContext.Request.Method = httpMethod;
            _httpContext.Request.Headers.Authorization = $"Bearer {token}";

            _userAuthServiceMock
                .Setup(auth => auth.ValidateTokenAsync(token))
                .ReturnsAsync(principal);

            // Act
            await _middleware.InvokeAsync(_httpContext, _userAuthServiceMock.Object);

            // Assert
            _httpContext.Response.StatusCode.Should().Be(200);
            _httpContext.User.Should().NotBeNull();
            _nextMock.Verify(next => next(_httpContext), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithTokenValidationException_ShouldReturn401()
        {
            // Arrange
            var token = "problematic-token";
            _httpContext.Request.Path = "/api/messages";
            _httpContext.Request.Method = "GET";
            _httpContext.Request.Headers.Authorization = $"Bearer {token}";
            _httpContext.Response.Body = new MemoryStream();

            _userAuthServiceMock
                .Setup(auth => auth.ValidateTokenAsync(token))
                .ThrowsAsync(new HttpRequestException("Service unavailable"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(
                () => _middleware.InvokeAsync(_httpContext, _userAuthServiceMock.Object));
            
            exception.Message.Should().Be("Service unavailable");
            _nextMock.Verify(next => next(_httpContext), Times.Never);
        }

        [Theory]
        [InlineData("/API/MESSAGES")]
        [InlineData("/Api/Messages")]
        [InlineData("/api/MESSAGES")]
        public async Task InvokeAsync_WithCaseInsensitivePaths_ShouldRequireAuth(string path)
        {
            // Arrange
            _httpContext.Request.Path = path;
            _httpContext.Request.Method = "GET";
            _httpContext.Response.Body = new MemoryStream();

            // Act
            await _middleware.InvokeAsync(_httpContext, _userAuthServiceMock.Object);

            // Assert
            _httpContext.Response.StatusCode.Should().Be(401);
            _nextMock.Verify(next => next(_httpContext), Times.Never);
        }

        [Theory]
        [InlineData("/SWAGGER")]
        [InlineData("/Swagger")]
        [InlineData("/HEALTH")]
        [InlineData("/Health")]
        public async Task InvokeAsync_WithCaseInsensitivePublicPaths_ShouldSkipAuth(string path)
        {
            // Arrange
            _httpContext.Request.Path = path;
            _httpContext.Request.Method = "GET";

            // Act
            await _middleware.InvokeAsync(_httpContext, _userAuthServiceMock.Object);

            // Assert
            _nextMock.Verify(next => next(_httpContext), Times.Once);
            _userAuthServiceMock.Verify(auth => auth.ValidateTokenAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task InvokeAsync_WithBearerTokenContainingSpaces_ShouldTrimAndValidate()
        {
            // Arrange
            var token = "valid-token-with-spaces";
            var userId = Guid.NewGuid().ToString();
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
            var identity = new ClaimsIdentity(claims, "Bearer");
            var principal = new ClaimsPrincipal(identity);

            _httpContext.Request.Path = "/api/messages";
            _httpContext.Request.Method = "GET";
            _httpContext.Request.Headers.Authorization = $"Bearer   {token}   ";

            _userAuthServiceMock
                .Setup(auth => auth.ValidateTokenAsync(token))
                .ReturnsAsync(principal);

            // Act
            await _middleware.InvokeAsync(_httpContext, _userAuthServiceMock.Object);

            // Assert
            _httpContext.Response.StatusCode.Should().Be(200);
            _nextMock.Verify(next => next(_httpContext), Times.Once);
            _userAuthServiceMock.Verify(auth => auth.ValidateTokenAsync(token), Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_WithNullPath_ShouldRequireAuth()
        {
            // Arrange
            _httpContext.Request.Path = new PathString();
            _httpContext.Request.Method = "GET";
            _httpContext.Response.Body = new MemoryStream();

            // Act
            await _middleware.InvokeAsync(_httpContext, _userAuthServiceMock.Object);

            // Assert
            _httpContext.Response.StatusCode.Should().Be(401);
            _nextMock.Verify(next => next(_httpContext), Times.Never);
        }

        [Theory]
        [InlineData("POST", "/api/users/test")]
        [InlineData("PUT", "/api/users/test")]
        [InlineData("DELETE", "/api/users/test")]
        public async Task InvokeAsync_WithNonGetMethodOnTestEndpoint_ShouldRequireAuth(string method, string path)
        {
            // Arrange
            _httpContext.Request.Path = path;
            _httpContext.Request.Method = method;
            _httpContext.Response.Body = new MemoryStream();

            // Act
            await _middleware.InvokeAsync(_httpContext, _userAuthServiceMock.Object);

            // Assert
            _httpContext.Response.StatusCode.Should().Be(401);
            _nextMock.Verify(next => next(_httpContext), Times.Never);
        }
    }
}
