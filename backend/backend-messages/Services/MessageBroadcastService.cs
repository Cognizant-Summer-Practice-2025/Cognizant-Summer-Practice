using Microsoft.AspNetCore.SignalR;
using BackendMessages.Hubs;
using BackendMessages.Models;
using BackendMessages.Services.Abstractions;

namespace BackendMessages.Services
{
    /// <summary>
    /// Service for broadcasting message-related events via SignalR
    /// </summary>
    public class MessageBroadcastService : IMessageBroadcastService
    {
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly ILogger<MessageBroadcastService> _logger;

        public MessageBroadcastService(
            IHubContext<MessageHub> hubContext,
            ILogger<MessageBroadcastService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task BroadcastNewMessageAsync(Message message, Conversation conversation)
        {
            try
            {
                // Create response object
                var messageResponse = new
                {
                    message.Id,
                    message.ConversationId,
                    message.SenderId,
                    message.ReceiverId,
                    message.Content,
                    message.MessageType,
                    message.ReplyToMessageId,
                    message.IsRead,
                    message.CreatedAt,
                    message.UpdatedAt
                };

                // Send to receiver
                await _hubContext.Clients.Group($"user_{message.ReceiverId}")
                    .SendAsync("ReceiveMessage", messageResponse);

                // Send to sender (for multi-device support)
                await _hubContext.Clients.Group($"user_{message.SenderId}")
                    .SendAsync("ReceiveMessage", messageResponse);

                _logger.LogInformation("Message {MessageId} broadcasted via SignalR to sender {SenderId} and receiver {ReceiverId}", 
                    message.Id, message.SenderId, message.ReceiverId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast message {MessageId} via SignalR", message.Id);
            }
        }

        public async Task BroadcastConversationUpdateAsync(Conversation conversation, Message lastMessage)
        {
            try
            {
                var lastMessageResponse = new
                {
                    lastMessage.Id,
                    lastMessage.ConversationId,
                    lastMessage.SenderId,
                    lastMessage.ReceiverId,
                    lastMessage.Content,
                    lastMessage.MessageType,
                    lastMessage.ReplyToMessageId,
                    lastMessage.IsRead,
                    lastMessage.CreatedAt,
                    lastMessage.UpdatedAt
                };

                var conversationUpdate = new
                {
                    conversation.Id,
                    conversation.LastMessageTimestamp,
                    LastMessage = lastMessageResponse,
                    conversation.UpdatedAt
                };

                // Send to both participants
                await _hubContext.Clients.Group($"user_{conversation.InitiatorId}")
                    .SendAsync("ConversationUpdated", conversationUpdate);
                
                await _hubContext.Clients.Group($"user_{conversation.ReceiverId}")
                    .SendAsync("ConversationUpdated", conversationUpdate);

                _logger.LogInformation("Conversation {ConversationId} update broadcasted via SignalR to participants {InitiatorId} and {ReceiverId}", 
                    conversation.Id, conversation.InitiatorId, conversation.ReceiverId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast conversation update {ConversationId} via SignalR", conversation.Id);
            }
        }
    }
} 