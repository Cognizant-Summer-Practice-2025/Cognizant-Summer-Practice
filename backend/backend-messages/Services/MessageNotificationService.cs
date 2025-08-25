using Microsoft.EntityFrameworkCore;
using BackendMessages.Data;
using BackendMessages.Services.Abstractions;
using BackendMessages.Models.Email;

namespace BackendMessages.Services
{
    /// <summary>
    /// Service for handling message-related notifications (email, push, etc.)
    /// </summary>
    public class MessageNotificationService : IMessageNotificationService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MessageNotificationService> _logger;

        public MessageNotificationService(
            IServiceScopeFactory serviceScopeFactory,
            IConfiguration configuration,
            ILogger<MessageNotificationService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public Task SendContactRequestNotificationAsync(Guid conversationId, Guid senderId, Guid receiverId, bool isFirstMessageFromInitiator)
        {
            var enableContactNotifications = bool.Parse(_configuration["Email:EnableContactNotifications"] ?? "true");
            
            if (!enableContactNotifications)
            {
                _logger.LogInformation("Contact notifications are disabled via configuration");
                return Task.CompletedTask;
            }

            // Run in background to avoid blocking the main request
            return Task.Run(async () =>
            {
                try
                {
                    using var serviceContainer = _serviceScopeFactory.CreateScope();
                    var services = serviceContainer.ServiceProvider;
                    
                    var context = services.GetRequiredService<MessagesDbContext>();
                    var userSearchService = services.GetRequiredService<IUserSearchService>();
                    var emailService = services.GetRequiredService<IEmailService>();
                    var logger = services.GetRequiredService<ILogger<MessageNotificationService>>();
                    
                    // Only send notification if this is the first message from the conversation initiator
                    if (isFirstMessageFromInitiator)
                    {
                        // Check if this is actually the first message in the conversation
                        var messageCount = await context.Messages
                            .Where(m => m.ConversationId == conversationId && m.DeletedAt == null)
                            .CountAsync();
                        
                        if (messageCount == 1) // This is the first message
                        {
                            var recipient = await userSearchService.GetUserByIdAsync(receiverId);
                            var sender = await userSearchService.GetUserByIdAsync(senderId);
                            
                            if (recipient != null && sender != null && !string.IsNullOrEmpty(recipient.Email))
                            {
                                var notification = new ContactRequestNotification
                                {
                                    Recipient = recipient,
                                    Sender = sender
                                };
                                var emailResult = await emailService.SendContactRequestNotificationAsync(notification);
                                
                                if (emailResult)
                                {
                                    logger.LogInformation("Contact request email notification sent successfully for conversation {ConversationId}", conversationId);
                                }
                                else
                                {
                                    logger.LogWarning("Contact request email notification failed for conversation {ConversationId}", conversationId);
                                }
                            }
                        }
                    }
                    else
                    {
                        logger.LogInformation("Skipping contact request notification - sender {SenderId} is not the conversation initiator for conversation {ConversationId}", 
                            senderId, conversationId);
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Background notification task failed for conversation {ConversationId}", conversationId);
                }
            });
        }
    }
} 