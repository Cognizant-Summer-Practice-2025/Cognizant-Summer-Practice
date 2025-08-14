using backend_AI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace backend_AI.tests.Services
{
    public class AuthorizationPathServiceTests
    {
        private readonly AuthorizationPathService _service = new();

        private static HttpContext Ctx(string path, string method = "GET")
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Path = path;
            ctx.Request.Method = method;
            return ctx;
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/openapi")]
        [InlineData("/swagger")]
        [InlineData("/health")]
        public void PublicPaths_ShouldNotRequireAuth(string path)
        {
            _service.RequiresAuthentication(Ctx(path)).Should().BeFalse();
        }

        [Theory]
        [InlineData("/openapi/v1.json")]
        [InlineData("/swagger/index.html")]
        [InlineData("/health/status")]
        public void PublicPrefixes_ShouldNotRequireAuth(string path)
        {
            _service.RequiresAuthentication(Ctx(path)).Should().BeFalse();
        }

        [Theory]
        [InlineData("/api/ai/generate")]
        [InlineData("/api/ai/generate-best-portfolio")]
        [InlineData("/api/secret")]
        public void OtherPaths_ShouldRequireAuth(string path)
        {
            _service.RequiresAuthentication(Ctx(path)).Should().BeTrue();
        }

        [Fact]
        public void EmptyPath_ShouldRequireAuth()
        {
            var ctx = new DefaultHttpContext();
            ctx.Request.Path = "";
            _service.RequiresAuthentication(ctx).Should().BeTrue();
        }
    }
}
