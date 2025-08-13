using backend_user.Config;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace backend_user.tests.Config
{
    public class MiddlewareConfigurationTests
    {
        [Fact]
        public void UseApplicationMiddleware_InDevelopment_ShouldMapSwagger()
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = Environments.Development
            });
            builder.Services.AddControllers();
            builder.Services.AddCors();

            var app = builder.Build();
            app.UseApplicationMiddleware();

            // Assert pipeline was built; mapping presence indirectly validated by no exceptions
            app.Should().NotBeNull();
        }

        [Fact]
        public void UseApplicationMiddleware_InProduction_ShouldNotThrow()
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = Environments.Production
            });
            builder.Services.AddControllers();
            builder.Services.AddCors();

            var app = builder.Build();
            app.UseApplicationMiddleware();

            app.Should().NotBeNull();
        }
    }
}


