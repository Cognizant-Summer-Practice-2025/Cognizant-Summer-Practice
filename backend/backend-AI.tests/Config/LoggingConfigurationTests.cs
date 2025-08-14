using backend_AI.Config;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace backend_AI.tests.Config
{
    public class LoggingConfigurationTests
    {
        [Fact]
        public void AddLoggingConfiguration_GlobalLevelFromEnv_ShouldNotThrow()
        {
            try
            {
                Environment.SetEnvironmentVariable("LOG_LEVEL", "Warning");
                var builder = WebApplication.CreateBuilder(new WebApplicationOptions
                {
                    EnvironmentName = Environments.Production
                });

                builder.AddLoggingConfiguration();

                builder.Logging.Should().NotBeNull();
            }
            finally
            {
                Environment.SetEnvironmentVariable("LOG_LEVEL", null);
            }
        }

        [Fact]
        public void AddLoggingConfiguration_ServiceSpecificLevels_ShouldNotThrow()
        {
            try
            {
                Environment.SetEnvironmentVariable("AI_LOG_LEVEL", "Information");
                Environment.SetEnvironmentVariable("RANKING_LOG_LEVEL", "Debug");

                var builder = WebApplication.CreateBuilder(new WebApplicationOptions
                {
                    EnvironmentName = Environments.Development
                });

                builder.AddLoggingConfiguration();

                builder.Logging.Should().NotBeNull();
            }
            finally
            {
                Environment.SetEnvironmentVariable("AI_LOG_LEVEL", null);
                Environment.SetEnvironmentVariable("RANKING_LOG_LEVEL", null);
            }
        }

        [Fact]
        public void AddLoggingConfiguration_DevDefaultRankingLevel_ShouldNotThrow()
        {
            try
            {
                Environment.SetEnvironmentVariable("RANKING_LOG_LEVEL", null);

                var builder = WebApplication.CreateBuilder(new WebApplicationOptions
                {
                    EnvironmentName = Environments.Development
                });

                builder.AddLoggingConfiguration();

                builder.Logging.Should().NotBeNull();
            }
            finally
            {
                Environment.SetEnvironmentVariable("RANKING_LOG_LEVEL", null);
            }
        }
    }
}


