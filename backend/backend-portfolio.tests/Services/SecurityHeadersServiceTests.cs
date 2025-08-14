using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Xunit;
using FluentAssertions;
using backend_portfolio.Services;

namespace backend_portfolio.tests.Services
{
    public class SecurityHeadersServiceTests
    {
        private readonly SecurityHeadersService _service;

        public SecurityHeadersServiceTests()
        {
            _service = new SecurityHeadersService();
        }

        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            _service.Should().NotBeNull();
        }

        [Fact]
        public void ApplySecurityHeaders_WithValidContext_ShouldNotThrow()
        {
            var context = new DefaultHttpContext();

            var action = () => _service.ApplySecurityHeaders(context);

            action.Should().NotThrow();
        }

        [Fact]
        public void ApplySecurityHeaders_WithNullContext_ShouldNotThrow()
        {
            var action = () => _service.ApplySecurityHeaders(null!);
            action.Should().NotThrow();
        }

        [Fact]
        public void ApplySecurityHeaders_WithEmptyContext_ShouldHandleGracefully()
        {
            var context = new DefaultHttpContext();

            var action = () => _service.ApplySecurityHeaders(context);
            action.Should().NotThrow();
        }

        [Fact]
        public void ApplySecurityHeaders_ShouldCompleteWithoutThrowing()
        {
            var context = new DefaultHttpContext();

            var action = () => _service.ApplySecurityHeaders(context);
            action.Should().NotThrow();
        }

        [Fact]
        public void ApplySecurityHeaders_WithMultipleCalls_ShouldNotThrow()
        {
            var context = new DefaultHttpContext();

            var action = () => {
                _service.ApplySecurityHeaders(context);
                _service.ApplySecurityHeaders(context);
            };

            action.Should().NotThrow();
        }

        [Fact]
        public void ApplySecurityHeaders_WithNullResponse_ShouldNotThrow()
        {
            var context = new DefaultHttpContext();
            // We can't set Response to null in DefaultHttpContext, so we'll test the null context case
            var action = () => _service.ApplySecurityHeaders(null!);
            action.Should().NotThrow();
        }
    }
}
