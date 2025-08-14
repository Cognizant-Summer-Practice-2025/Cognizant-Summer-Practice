namespace backend_AI.Config;

/// <summary>
/// Configuration class for logging setup 
/// </summary>
public static class LoggingConfiguration
{
    /// <summary>
    /// Configures logging services with environment variable support
    /// </summary>
    /// <param name="builder">Web application builder to configure</param>
    /// <returns>Configured web application builder</returns>
    public static WebApplicationBuilder AddLoggingConfiguration(this WebApplicationBuilder builder)
    {
        builder.Logging.AddConsole();
        
        ConfigureGlobalLogLevel(builder);
        ConfigureAiServiceLogLevel(builder);
        ConfigureRankingServiceLogLevel(builder);

        return builder;
    }

    /// <summary>
    /// Configures global log level from environment variable
    /// </summary>
    /// <param name="builder">Web application builder to configure</param>
    private static void ConfigureGlobalLogLevel(WebApplicationBuilder builder)
    {
        var logLevelEnv = Environment.GetEnvironmentVariable("LOG_LEVEL");
        if (!string.IsNullOrWhiteSpace(logLevelEnv) && 
            Enum.TryParse<LogLevel>(logLevelEnv, true, out var minLevel))
        {
            builder.Logging.SetMinimumLevel(minLevel);
        }
    }

    /// <summary>
    /// Configures AI chat service specific log level
    /// </summary>
    /// <param name="builder">Web application builder to configure</param>
    private static void ConfigureAiServiceLogLevel(WebApplicationBuilder builder)
    {
        var aiLogLevelEnv = Environment.GetEnvironmentVariable("AI_LOG_LEVEL");
        if (!string.IsNullOrWhiteSpace(aiLogLevelEnv) && 
            Enum.TryParse<LogLevel>(aiLogLevelEnv, true, out var aiLevel))
        {
            builder.Logging.AddFilter("backend_AI.Services.AiChatService", aiLevel);
        }
    }

    /// <summary>
    /// Configures portfolio ranking service specific log level
    /// </summary>
    /// <param name="builder">Web application builder to configure</param>
    private static void ConfigureRankingServiceLogLevel(WebApplicationBuilder builder)
    {
        var rankingLevelEnv = Environment.GetEnvironmentVariable("RANKING_LOG_LEVEL");
        if (!string.IsNullOrWhiteSpace(rankingLevelEnv) && 
            Enum.TryParse<LogLevel>(rankingLevelEnv, true, out var rankingLevel))
        {
            builder.Logging.AddFilter("backend_AI.Services.PortfolioRankingService", rankingLevel);
        }
        else if (builder.Environment.IsDevelopment())
        {
            builder.Logging.AddFilter("backend_AI.Services.PortfolioRankingService", LogLevel.Debug);
        }
    }
}
