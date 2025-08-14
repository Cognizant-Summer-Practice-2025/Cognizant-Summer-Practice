using System;
using Microsoft.AspNetCore.Http;
using Xunit;
using FluentAssertions;
using backend_portfolio.Services;

namespace backend_portfolio.tests.Services
{
    public class AuthorizationPathServiceTests
    {
        private readonly AuthorizationPathService _service;

        public AuthorizationPathServiceTests()
        {
            _service = new AuthorizationPathService();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            _service.Should().NotBeNull();
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/openapi")]
        [InlineData("/swagger")]
        [InlineData("/health")]
        public void RequiresAuthentication_WithPublicPaths_ShouldReturnFalse(string path)
        {
            var context = CreateHttpContext(path, "GET");
            var result = _service.RequiresAuthentication(context);
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("/api/portfolio")]
        [InlineData("/api/portfoliotemplate")]
        [InlineData("/api/project")]
        [InlineData("/api/bookmark")]
        [InlineData("/api/image")]
        public void RequiresAuthentication_WithPublicResourcePaths_ShouldReturnFalse(string path)
        {
            var context = CreateHttpContext(path, "GET");
            var result = _service.RequiresAuthentication(context);
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("/api/portfolio/123/view", "POST")]
        [InlineData("/api/portfoliotemplate/seed", "POST")]
        public void RequiresAuthentication_WithSpecificPublicPosts_ShouldReturnFalse(string path, string method)
        {
            var context = CreateHttpContext(path, method);
            var result = _service.RequiresAuthentication(context);
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("/api/portfolio/detailed-all", "GET")]
        [InlineData("/api/portfolio/123/edit", "PUT")]
        [InlineData("/api/portfolio/123", "DELETE")]
        [InlineData("/api/project/123", "POST")]
        [InlineData("/api/portfolio", "POST")]
        public void RequiresAuthentication_WithProtectedPaths_ShouldReturnTrue(string path, string method)
        {
            var context = CreateHttpContext(path, method);
            var result = _service.RequiresAuthentication(context);
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("/api/unknown")]
        [InlineData("/api/user/profile")]
        [InlineData("/admin")]
        [InlineData("/private")]
        public void RequiresAuthentication_WithUnknownPaths_ShouldReturnTrue(string path)
        {
            var context = CreateHttpContext(path, "GET");
            var result = _service.RequiresAuthentication(context);
            result.Should().BeTrue();
        }

        [Fact]
        public void RequiresAuthentication_WithNullPath_ShouldReturnTrue()
        {
            var context = CreateHttpContext(null, "GET");
            var result = _service.RequiresAuthentication(context);
            result.Should().BeTrue();
        }

        [Fact]
        public void RequiresAuthentication_WithEmptyPath_ShouldReturnTrue()
        {
            var context = CreateHttpContext("", "GET");
            var result = _service.RequiresAuthentication(context);
            result.Should().BeTrue();
        }

        [Fact]
        public void RequiresAuthentication_WithWhitespacePath_ShouldReturnTrue()
        {
            var context = CreateHttpContext("/   ", "GET");
            var result = _service.RequiresAuthentication(context);
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("/api/portfolio/123", "GET")]
        [InlineData("/api/portfoliotemplate/456", "GET")]
        [InlineData("/api/project/789", "GET")]
        [InlineData("/api/bookmark/101", "GET")]
        [InlineData("/api/image/202", "GET")]
        public void RequiresAuthentication_WithPublicGetRequests_ShouldReturnFalse(string path, string method)
        {
            var context = CreateHttpContext(path, method);
            var result = _service.RequiresAuthentication(context);
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("/api/portfolio/123", "POST")]
        [InlineData("/api/portfolio/123", "PUT")]
        [InlineData("/api/portfolio/123", "DELETE")]
        [InlineData("/api/project/456", "POST")]
        [InlineData("/api/project/456", "PUT")]
        [InlineData("/api/project/456", "DELETE")]
        public void RequiresAuthentication_WithNonGetRequests_ShouldReturnTrue(string path, string method)
        {
            var context = CreateHttpContext(path, method);
            var result = _service.RequiresAuthentication(context);
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("/api/portfolio/123/view", "GET")]
        [InlineData("/api/portfolio/123/view", "POST")]
        public void RequiresAuthentication_WithViewPaths_ShouldReturnFalse(string path, string method)
        {
            var context = CreateHttpContext(path, method);
            var result = _service.RequiresAuthentication(context);
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("/api/portfoliotemplate/seed", "GET")]
        [InlineData("/api/portfoliotemplate/seed", "POST")]
        public void RequiresAuthentication_WithSeedPaths_ShouldReturnFalse(string path, string method)
        {
            var context = CreateHttpContext(path, method);
            var result = _service.RequiresAuthentication(context);
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("/api/portfolio/detailed-all", "GET")]
        [InlineData("/api/portfolio/detailed-all", "POST")]
        public void RequiresAuthentication_WithDetailedAllPaths_ShouldReturnTrue(string path, string method)
        {
            var context = CreateHttpContext(path, method);
            var result = _service.RequiresAuthentication(context);
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("/api/portfolio/123", "GET")]
        [InlineData("/api/portfolio/456", "GET")]
        [InlineData("/api/portfolio/789", "GET")]
        public void RequiresAuthentication_WithPortfolioGetRequests_ShouldReturnFalse(string path, string method)
        {
            var context = CreateHttpContext(path, method);
            var result = _service.RequiresAuthentication(context);
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("/api/portfolio/123", "POST")]
        [InlineData("/api/portfolio/456", "PUT")]
        [InlineData("/api/portfolio/789", "DELETE")]
        public void RequiresAuthentication_WithPortfolioNonGetRequests_ShouldReturnTrue(string path, string method)
        {
            var context = CreateHttpContext(path, method);
            var result = _service.RequiresAuthentication(context);
            result.Should().BeTrue();
        }

        private static HttpContext CreateHttpContext(string? path, string method)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = path;
            context.Request.Method = method;
            return context;
        }
    }
}
