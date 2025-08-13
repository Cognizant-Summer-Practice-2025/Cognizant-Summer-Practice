using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using backend_user.Services;

namespace backend_user.tests.Services
{
    public class AuthorizationPathServiceTests
    {
        private readonly AuthorizationPathService _service;
        private readonly DefaultHttpContext _context;

        public AuthorizationPathServiceTests()
        {
            _service = new AuthorizationPathService();
            _context = new DefaultHttpContext();
        }

        [Theory]
        [InlineData("/")]
        public void RequiresAuthentication_ShouldReturnFalse_ForExactPublicPaths(string path)
        {
            // Arrange
            _context.Request.Path = path;

            // Act
            var result = _service.RequiresAuthentication(_context);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("/api/users/login")]
        [InlineData("/api/users/register")]
        [InlineData("/api/users/oauth-providers/check")]
        [InlineData("/api/users/check-email")]
        [InlineData("/api/oauth/callback")]
        [InlineData("/api/oauth2/token")]
        [InlineData("/openapi")]
        [InlineData("/swagger")]
        [InlineData("/health")]
        [InlineData("/API/USERS/LOGIN")] // Case insensitive
        [InlineData("/Health/Check")] // Case insensitive with subpath
        public void RequiresAuthentication_ShouldReturnFalse_ForPublicPathPrefixes(string path)
        {
            // Arrange
            _context.Request.Path = path;

            // Act
            var result = _service.RequiresAuthentication(_context);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void RequiresAuthentication_ShouldReturnFalse_ForUserEmailPath()
        {
            // Arrange
            _context.Request.Path = "/api/users/email/test@example.com";

            // Act
            var result = _service.RequiresAuthentication(_context);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("get")]
        [InlineData("Get")]
        public void RequiresAuthentication_ShouldReturnFalse_ForOAuthProvidersGetRequests(string method)
        {
            // Arrange
            _context.Request.Path = "/api/users/123/oauth-providers/google";
            _context.Request.Method = method;

            // Act
            var result = _service.RequiresAuthentication(_context);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        [InlineData("PATCH")]
        public void RequiresAuthentication_ShouldReturnTrue_ForOAuthProvidersNonGetRequests(string method)
        {
            // Arrange
            _context.Request.Path = "/api/users/123/oauth-providers/google";
            _context.Request.Method = method;

            // Act
            var result = _service.RequiresAuthentication(_context);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("get")]
        [InlineData("Get")]
        public void RequiresAuthentication_ShouldReturnFalse_ForPortfolioInfoGetRequests(string method)
        {
            // Arrange
            _context.Request.Path = "/api/users/123/portfolio-info";
            _context.Request.Method = method;

            // Act
            var result = _service.RequiresAuthentication(_context);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        [InlineData("PATCH")]
        public void RequiresAuthentication_ShouldReturnTrue_ForPortfolioInfoNonGetRequests(string method)
        {
            // Arrange
            _context.Request.Path = "/api/users/123/portfolio-info";
            _context.Request.Method = method;

            // Act
            var result = _service.RequiresAuthentication(_context);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("/api/users/123")]
        [InlineData("/api/users/456/profile")]
        public void RequiresAuthentication_ShouldReturnFalse_ForUserGetRequestsWithoutRestrictedPaths(string path)
        {
            // Arrange
            _context.Request.Path = path;
            _context.Request.Method = "GET";

            // Act
            var result = _service.RequiresAuthentication(_context);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("/api/users/123/bookmarks/")]
        public void RequiresAuthentication_ShouldReturnTrue_ForUserGetRequestsWithRestrictedPaths(string path)
        {
            // Arrange
            _context.Request.Path = path;
            _context.Request.Method = "GET";

            // Act
            var result = _service.RequiresAuthentication(_context);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        [InlineData("PATCH")]
        public void RequiresAuthentication_ShouldReturnTrue_ForUserNonGetRequests(string method)
        {
            // Arrange
            _context.Request.Path = "/api/users/123";
            _context.Request.Method = method;

            // Act
            var result = _service.RequiresAuthentication(_context);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("/api/secure")]
        [InlineData("/api/private")]
        [InlineData("/admin")]
        [InlineData("/api/users/create")]
        [InlineData("/api/posts")]
        public void RequiresAuthentication_ShouldReturnTrue_ForPrivatePaths(string path)
        {
            // Arrange
            _context.Request.Path = path;

            // Act
            var result = _service.RequiresAuthentication(_context);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void RequiresAuthentication_ShouldReturnTrue_ForEmptyPath()
        {
            // Arrange
            _context.Request.Path = "";

            // Act
            var result = _service.RequiresAuthentication(_context);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void RequiresAuthentication_ShouldReturnTrue_ForNullPath()
        {
            // Arrange
            _context.Request.Path = new PathString();

            // Act
            var result = _service.RequiresAuthentication(_context);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("/API/USERS/LOGIN")]
        [InlineData("/Api/Users/Register")]
        [InlineData("/HEALTH")]
        [InlineData("/SwAgGeR")]
        public void RequiresAuthentication_ShouldBeCaseInsensitive(string path)
        {
            // Arrange
            _context.Request.Path = path;

            // Act
            var result = _service.RequiresAuthentication(_context);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void RequiresAuthentication_ShouldHandleTrailingSlashes()
        {
            // Arrange
            _context.Request.Path = "/api/users/login/";

            // Act
            var result = _service.RequiresAuthentication(_context);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void RequiresAuthentication_ShouldHandleQueryParameters()
        {
            // Arrange
            _context.Request.Path = "/api/users/login";
            _context.Request.QueryString = new QueryString("?redirect=/dashboard");

            // Act
            var result = _service.RequiresAuthentication(_context);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("/api/users/email/user@example.com")]
        [InlineData("/api/users/email/test.user+tag@domain.co.uk")]
        public void RequiresAuthentication_ShouldHandleComplexEmailPaths(string path)
        {
            // Arrange
            _context.Request.Path = path;

            // Act
            var result = _service.RequiresAuthentication(_context);

            // Assert
            result.Should().BeFalse();
        }
    }
}
