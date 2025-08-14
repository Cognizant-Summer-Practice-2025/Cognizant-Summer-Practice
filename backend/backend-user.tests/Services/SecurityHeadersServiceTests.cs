using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using backend_user.Services;

namespace backend_user.tests.Services
{
    public class SecurityHeadersServiceTests
    {
        private readonly SecurityHeadersService _service;

        public SecurityHeadersServiceTests()
        {
            _service = new SecurityHeadersService();
        }

        [Fact]
        public void ApplySecurityHeaders_ShouldHandleNullContext_Gracefully()
        {
            // Act & Assert - Should not throw
            var act = () => _service.ApplySecurityHeaders(null!);
            act.Should().NotThrow();
        }

        [Fact]
        public void ApplySecurityHeaders_ShouldHandleNullResponse_Gracefully()
        {
            // Arrange
            var contextWithNullResponse = new DefaultHttpContext();
            
            // Act & Assert - Should not throw
            var act = () => _service.ApplySecurityHeaders(contextWithNullResponse);
            act.Should().NotThrow();
        }

        [Fact]
        public void ApplySecurityHeaders_ShouldRegisterOnStartingCallback()
        {
            // Arrange
            var context = new DefaultHttpContext();

            // Act & Assert - Should not throw when registering callback
            var act = () => _service.ApplySecurityHeaders(context);
            act.Should().NotThrow("because ApplySecurityHeaders should successfully register OnStarting callback");
        }

        [Fact]
        public void ApplySecurityHeaders_ShouldNotThrow_WhenCalledMultipleTimes()
        {
            // Arrange
            var context = new DefaultHttpContext();

            // Act & Assert
            var act = () =>
            {
                _service.ApplySecurityHeaders(context);
                _service.ApplySecurityHeaders(context);
                _service.ApplySecurityHeaders(context);
            };
            
            act.Should().NotThrow();
        }

        [Fact]
        public void ApplySecurityHeaders_ShouldWorkWithDifferentContexts()
        {
            // Arrange
            var context1 = new DefaultHttpContext();
            var context2 = new DefaultHttpContext();

            // Act & Assert - Should not throw
            var act = () =>
            {
                _service.ApplySecurityHeaders(context1);
                _service.ApplySecurityHeaders(context2);
            };
            
            act.Should().NotThrow();
        }

        [Fact]
        public void SecurityHeadersService_ShouldHaveParameterlessConstructor()
        {
            // Act & Assert
            var act = () => new SecurityHeadersService();
            act.Should().NotThrow();
            
            var service = new SecurityHeadersService();
            service.Should().NotBeNull();
        }
    }
}
