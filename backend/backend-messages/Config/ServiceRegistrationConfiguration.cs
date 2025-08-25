using BackendMessages.Repositories;
using BackendMessages.Services;
using BackendMessages.Services.Abstractions;

namespace BackendMessages.Config;

/// <summary>
/// Configuration class for dependency injection registration 
/// </summary>
public static class ServiceRegistrationConfiguration
{
    /// <summary>
    /// Registers all application services with dependency injection container
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddRepositoryServices();
        services.AddBusinessServices();
        services.AddAuthenticationServices();

        return services;
    }

    /// <summary>
    /// Registers repository services
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <returns>Configured service collection</returns>
    private static IServiceCollection AddRepositoryServices(this IServiceCollection services)
    {
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageReportRepository, MessageReportRepository>();

        return services;
    }

    /// <summary>
    /// Registers business services
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <returns>Configured service collection</returns>
    private static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IUserSearchService, UserSearchService>();
        services.AddScoped<IConversationService, ConversationServiceRefactored>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IEmailValidator, EmailValidator>();
        services.AddScoped<IEmailTemplateEngine, EmailTemplateEngine>();
        services.AddScoped<ISmtpClientService, SmtpClientService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IMessageCreationService, MessageCreationService>();
        services.AddScoped<IMessageBroadcastService, MessageBroadcastService>();
        services.AddScoped<IMessageNotificationService, MessageNotificationService>();

        return services;
    }

    /// <summary>
    /// Registers authentication services
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    /// <returns>Configured service collection</returns>
    private static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserAuthenticationService, UserAuthenticationService>();

        return services;
    }
}
