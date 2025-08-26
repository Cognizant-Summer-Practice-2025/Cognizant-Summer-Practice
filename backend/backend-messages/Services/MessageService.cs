using Microsoft.AspNetCore.SignalR;
using BackendMessages.DTO.Message.Request;
using BackendMessages.DTO.Message.Response;
using BackendMessages.Repositories;
using BackendMessages.Services.Abstractions;
using BackendMessages.Services.Mappers;
using BackendMessages.Services.Validators;
using BackendMessages.Hubs;

namespace BackendMessages.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IConversationRepository _conversationRepository;
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly ILogger<MessageService> _logger;
        private readonly SendMessageValidator _sendMessageValidator;
        private readonly MarkMessageAsReadValidator _markMessageAsReadValidator;
        private readonly MarkMessagesAsReadValidator _markMessagesAsReadValidator;
        private readonly DeleteMessageValidator _deleteMessageValidator;
        private readonly ReportMessageValidator _reportMessageValidator;

        public MessageService(
            IMessageRepository messageRepository,
            IConversationRepository conversationRepository,
            IHubContext<MessageHub> hubContext,
            ILogger<MessageService> logger)
        {
            _messageRepository = messageRepository;
            _conversationRepository = conversationRepository;
            _hubContext = hubContext;
            _logger = logger;
            _sendMessageValidator = new SendMessageValidator();
            _markMessageAsReadValidator = new MarkMessageAsReadValidator();
            _markMessagesAsReadValidator = new MarkMessagesAsReadValidator();
            _deleteMessageValidator = new DeleteMessageValidator();
            _reportMessageValidator = new ReportMessageValidator();
        }

        public async Task<SendMessageResponse> SendMessageAsync(SendMessageRequest request)
        {
            try
            {
                // Validate request
                var validation = _sendMessageValidator.Validate(request);
                if (!validation.IsValid)
                {
                    return MessageMapper.ToSendMessageResponse(new Models.Message(), false, validation.ErrorMessage);
                }

                // Verify conversation exists and user can access it
                var canAccess = await _conversationRepository.UserCanAccessConversationAsync(request.ConversationId, request.SenderId);
                if (!canAccess)
                {
                    return MessageMapper.ToSendMessageResponse(new Models.Message(), false, "Conversation not found or access denied");
                }

                // Verify reply-to message if specified
                if (request.ReplyToMessageId.HasValue)
                {
                    var canAccessReplyMessage = await _messageRepository.UserCanAccessMessageAsync(request.ReplyToMessageId.Value, request.SenderId);
                    if (!canAccessReplyMessage)
                    {
                        return MessageMapper.ToSendMessageResponse(new Models.Message(), false, "Reply-to message not found or access denied");
                    }
                }

                // Create and save message
                var message = MessageMapper.ToEntity(request);
                var savedMessage = await _messageRepository.CreateAsync(message);

                // Update conversation's last message
                await _conversationRepository.UpdateLastMessageAsync(request.ConversationId, savedMessage.Id);

                // Send real-time notification via SignalR
                await SendRealTimeNotification(savedMessage);

                _logger.LogInformation("Message {MessageId} sent successfully from {SenderId} to {ReceiverId}", 
                    savedMessage.Id, request.SenderId, request.ReceiverId);

                return MessageMapper.ToSendMessageResponse(savedMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message from {SenderId} to {ReceiverId}", request.SenderId, request.ReceiverId);
                return MessageMapper.ToSendMessageResponse(new Models.Message(), false, "An error occurred while sending the message");
            }
        }

        public async Task<MessageResponse?> GetMessageByIdAsync(Guid messageId, Guid userId)
        {
            try
            {
                var canAccess = await _messageRepository.UserCanAccessMessageAsync(messageId, userId);
                if (!canAccess)
                {
                    return null;
                }

                var message = await _messageRepository.GetByIdAsync(messageId);
                return message != null ? MessageMapper.ToResponse(message) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting message {MessageId} for user {UserId}", messageId, userId);
                return null;
            }
        }

        public async Task<MessagesPagedResponse> GetConversationMessagesAsync(Guid conversationId, Guid userId, int page = 1, int pageSize = 50, DateTime? before = null, DateTime? after = null)
        {
            try
            {
                var canAccess = await _conversationRepository.UserCanAccessConversationAsync(conversationId, userId);
                if (!canAccess)
                {
                    return new MessagesPagedResponse();
                }

                var messages = await _messageRepository.GetByConversationIdAsync(conversationId, before, after, page, pageSize);
                var totalCount = await _messageRepository.GetTotalCountByConversationIdAsync(conversationId);

                return MessageMapper.ToPagedResponse(messages, totalCount, page, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for conversation {ConversationId} and user {UserId}", conversationId, userId);
                return new MessagesPagedResponse();
            }
        }

        public async Task<MarkAsReadResponse> MarkMessageAsReadAsync(MarkMessageAsReadRequest request)
        {
            try
            {
                var validation = _markMessageAsReadValidator.Validate(request);
                if (!validation.IsValid)
                {
                    return MessageMapper.ToMarkAsReadResponse(0, false, validation.ErrorMessage);
                }

                var success = await _messageRepository.MarkAsReadAsync(request.MessageId, request.UserId);
                if (!success)
                {
                    return MessageMapper.ToMarkAsReadResponse(0, false, "Message not found or already read");
                }

                // Send read receipt via SignalR
                await SendReadReceiptNotification(request.MessageId, request.UserId);

                return MessageMapper.ToMarkAsReadResponse(1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message {MessageId} as read by user {UserId}", request.MessageId, request.UserId);
                return MessageMapper.ToMarkAsReadResponse(0, false, "An error occurred while marking message as read");
            }
        }

        public async Task<MarkAsReadResponse> MarkConversationAsReadAsync(MarkMessagesAsReadRequest request)
        {
            try
            {
                var validation = _markMessagesAsReadValidator.Validate(request);
                if (!validation.IsValid)
                {
                    return MessageMapper.ToMarkAsReadResponse(0, false, validation.ErrorMessage);
                }

                var canAccess = await _conversationRepository.UserCanAccessConversationAsync(request.ConversationId, request.UserId);
                if (!canAccess)
                {
                    return MessageMapper.ToMarkAsReadResponse(0, false, "Conversation not found or access denied");
                }

                var markedCount = await _messageRepository.MarkConversationAsReadAsync(request.ConversationId, request.UserId);
                
                // Send read receipts for all marked messages via SignalR
                if (markedCount > 0)
                {
                    await SendConversationReadReceiptNotification(request.ConversationId, request.UserId);
                }

                return MessageMapper.ToMarkAsReadResponse(markedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking conversation {ConversationId} as read by user {UserId}", request.ConversationId, request.UserId);
                return MessageMapper.ToMarkAsReadResponse(0, false, "An error occurred while marking messages as read");
            }
        }

        public async Task<DeleteMessageResponse> DeleteMessageAsync(DeleteMessageRequest request)
        {
            try
            {
                var validation = _deleteMessageValidator.Validate(request);
                if (!validation.IsValid)
                {
                    return MessageMapper.ToDeleteMessageResponse(false, validation.ErrorMessage);
                }

                var userOwnsMessage = await _messageRepository.UserOwnsMessageAsync(request.MessageId, request.UserId);
                if (!userOwnsMessage)
                {
                    return MessageMapper.ToDeleteMessageResponse(false, "Message not found or you don't have permission to delete it");
                }

                var success = await _messageRepository.SoftDeleteAsync(request.MessageId, request.UserId);
                if (!success)
                {
                    return MessageMapper.ToDeleteMessageResponse(false, "Message not found or already deleted");
                }

                // Send deletion notification via SignalR
                await SendMessageDeletionNotification(request.MessageId, request.UserId);

                return MessageMapper.ToDeleteMessageResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId} by user {UserId}", request.MessageId, request.UserId);
                return MessageMapper.ToDeleteMessageResponse(false, "An error occurred while deleting the message");
            }
        }

        public async Task<DeleteMessageResponse> ReportMessageAsync(ReportMessageRequest request)
        {
            try
            {
                var validation = _reportMessageValidator.Validate(request);
                if (!validation.IsValid)
                {
                    return MessageMapper.ToDeleteMessageResponse(false, validation.ErrorMessage);
                }

                var canAccess = await _messageRepository.UserCanAccessMessageAsync(request.MessageId, request.ReportedById);
                if (!canAccess)
                {
                    return MessageMapper.ToDeleteMessageResponse(false, "Message not found or access denied");
                }

                return MessageMapper.ToDeleteMessageResponse();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting message {MessageId} by user {UserId}", request.MessageId, request.ReportedById);
                return MessageMapper.ToDeleteMessageResponse(false, "An error occurred while reporting the message");
            }
        }

        public async Task<IEnumerable<MessageResponse>> GetUnreadMessagesAsync(Guid userId)
        {
            try
            {
                var messages = await _messageRepository.GetUnreadMessagesByUserIdAsync(userId);
                return MessageMapper.ToResponseList(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread messages for user {UserId}", userId);
                return new List<MessageResponse>();
            }
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            try
            {
                return await _messageRepository.GetUnreadCountByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
                return 0;
            }
        }

        public async Task<bool> UserCanAccessMessageAsync(Guid messageId, Guid userId)
        {
            try
            {
                return await _messageRepository.UserCanAccessMessageAsync(messageId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking message access for user {UserId} and message {MessageId}", userId, messageId);
                return false;
            }
        }

        public async Task<bool> UserOwnsMessageAsync(Guid messageId, Guid userId)
        {
            try
            {
                return await _messageRepository.UserOwnsMessageAsync(messageId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking message ownership for user {UserId} and message {MessageId}", userId, messageId);
                return false;
            }
        }

        // Private helper methods for SignalR notifications
        private async Task SendRealTimeNotification(Models.Message message)
        {
            try
            {
                var messageResponse = MessageMapper.ToResponse(message);
                
                // Send to receiver
                await _hubContext.Clients.Group($"user_{message.ReceiverId}")
                    .SendAsync("NewMessage", messageResponse);

                // Send to sender (for multi-device support)
                await _hubContext.Clients.Group($"user_{message.SenderId}")
                    .SendAsync("MessageSent", messageResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending real-time notification for message {MessageId}", message.Id);
            }
        }

        private async Task SendReadReceiptNotification(Guid messageId, Guid userId)
        {
            try
            {
                var message = await _messageRepository.GetByIdAsync(messageId);
                if (message != null)
                {
                    var readReceipt = new
                    {
                        messageId = messageId.ToString(),
                        conversationId = message.ConversationId.ToString(),
                        readByUserId = userId.ToString(),
                        readAt = DateTime.UtcNow
                    };

                    // Send to sender
                    await _hubContext.Clients.Group($"user_{message.SenderId}")
                        .SendAsync("MessageRead", readReceipt);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending read receipt notification for message {MessageId}", messageId);
            }
        }

        private async Task SendConversationReadReceiptNotification(Guid conversationId, Guid userId)
        {
            try
            {
                var readReceipt = new
                {
                    conversationId = conversationId.ToString(),
                    readByUserId = userId.ToString(),
                    readAt = DateTime.UtcNow
                };

                // Send to all conversation participants
                var conversation = await _conversationRepository.GetByIdAsync(conversationId);
                if (conversation != null)
                {
                    await _hubContext.Clients.Group($"user_{conversation.InitiatorId}")
                        .SendAsync("ConversationRead", readReceipt);
                    
                    await _hubContext.Clients.Group($"user_{conversation.ReceiverId}")
                        .SendAsync("ConversationRead", readReceipt);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending conversation read receipt notification for conversation {ConversationId}", conversationId);
            }
        }

        private async Task SendMessageDeletionNotification(Guid messageId, Guid userId)
        {
            try
            {
                var message = await _messageRepository.GetByIdAsync(messageId);
                if (message != null)
                {
                    var deletionEvent = new
                    {
                        MessageId = messageId.ToString(),
                        ConversationId = message.ConversationId.ToString(),
                        DeletedBy = userId.ToString(),
                        DeletedAt = DateTime.UtcNow
                    };

                    // Send to receiver
                    await _hubContext.Clients.Group($"user_{message.ReceiverId}")
                        .SendAsync("MessageDeleted", deletionEvent);

                    // Send to sender (for multi-device support)
                    await _hubContext.Clients.Group($"user_{message.SenderId}")
                        .SendAsync("MessageDeleted", deletionEvent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message deletion notification for message {MessageId}", messageId);
            }
        }
    }
} 