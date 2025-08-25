using Microsoft.EntityFrameworkCore;
using BackendMessages.Data;
using BackendMessages.Models;
using BackendMessages.Services.Abstractions;

namespace BackendMessages.Services
{
    /// <summary>
    /// Service for creating and persisting messages
    /// </summary>
    public class MessageCreationService : IMessageCreationService
    {
        private readonly MessagesDbContext _context;
        private readonly ILogger<MessageCreationService> _logger;

        public MessageCreationService(
            MessagesDbContext context,
            ILogger<MessageCreationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(Message message, Conversation conversation)> CreateMessageAsync(
            Guid conversationId, 
            Guid senderId, 
            string content, 
            MessageType? messageType = null, 
            Guid? replyToMessageId = null)
        {
            // Get the conversation
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null)
            {
                throw new ArgumentException($"Conversation with ID {conversationId} not found", nameof(conversationId));
            }

            // Verify user is part of the conversation
            if (conversation.InitiatorId != senderId && conversation.ReceiverId != senderId)
            {
                throw new UnauthorizedAccessException("User is not part of this conversation");
            }

            // Determine receiver ID
            var receiverId = conversation.InitiatorId == senderId 
                ? conversation.ReceiverId 
                : conversation.InitiatorId;

            // Create message
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content,
                MessageType = messageType ?? MessageType.Text,
                ReplyToMessageId = replyToMessageId,
                IsRead = false,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            _context.Messages.Add(message);

            // Update conversation's last message
            conversation.LastMessageId = message.Id;
            conversation.LastMessageTimestamp = message.CreatedAt;
            conversation.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Message {MessageId} created successfully in conversation {ConversationId} from {SenderId} to {ReceiverId}", 
                message.Id, conversationId, senderId, receiverId);

            return (message, conversation);
        }
    }
} 