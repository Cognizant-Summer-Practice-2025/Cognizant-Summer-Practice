using Microsoft.EntityFrameworkCore;
using BackendMessages.Data;
using BackendMessages.Services.Abstractions;
using BackendMessages.Models.Email;
using Microsoft.Extensions.Options;

namespace BackendMessages.Services
{
    /// <summary>
    /// Service for handling message-related notifications (email)
    /// </summary>
    public class MessageNotificationService : IMessageNotificationService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<MessageNotificationService> _logger;

        public MessageNotificationService(
            IServiceScopeFactory serviceScopeFactory,
            IOptions<EmailSettings> emailSettings,
            ILogger<MessageNotificationService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public Task SendContactRequestNotificationAsync(Guid conversationId, Guid senderId, Guid receiverId, bool isFirstMessageFromInitiator)
        {
            if (!_emailSettings.EnableContactNotifications)
            {
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
                                
                                if (!emailResult)
                                {
                                    logger.LogWarning("Contact request email notification failed for conversation {ConversationId}", conversationId);
                                }
                            }
                        }
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