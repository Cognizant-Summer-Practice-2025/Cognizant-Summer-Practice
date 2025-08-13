using backend_AI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace backend_AI.tests.Services
{
    public class SecurityHeadersServiceTests
    {
        private readonly SecurityHeadersService _service = new();

        [Fact]
        public void ApplySecurityHeaders_NullContext_NoThrow()
        {
            var act = () => _service.ApplySecurityHeaders(null!);
            act.Should().NotThrow();
        }

        [Fact]
        public void ApplySecurityHeaders_DefaultContext_NoThrow()
        {
            var ctx = new DefaultHttpContext();
            var act = () => _service.ApplySecurityHeaders(ctx);
            act.Should().NotThrow();
        }

        [Fact]
        public void ApplySecurityHeaders_MultipleCalls_NoThrow()
        {
            var ctx = new DefaultHttpContext();
            _service.ApplySecurityHeaders(ctx);
            var act = () => _service.ApplySecurityHeaders(ctx);
            act.Should().NotThrow();
        }
    }
}
