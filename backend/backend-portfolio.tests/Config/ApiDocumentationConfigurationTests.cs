using backend_portfolio.Config;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace backend_portfolio.tests.Config
{
    public class ApiDocumentationConfigurationTests
    {
        [Fact]
        public void AddApiDocumentation_ShouldReturnServices()
        {
            var services = new ServiceCollection();

            var result = services.AddApiDocumentation();

            result.Should().BeSameAs(services);
        }
    }
}


