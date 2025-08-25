using BackendMessages.Models;

namespace BackendMessages.Services.Abstractions
{
    /// <summary>
    /// Service for creating and persisting messages
    /// </summary>
    public interface IMessageCreationService
    {
        /// <summary>
        /// Creates a new message and updates the conversation
        /// </summary>
        /// <param name="conversationId">The conversation ID</param>
        /// <param name="senderId">The sender's user ID</param>
        /// <param name="content">The message content</param>
        /// <param name="messageType">The message type (optional, defaults to Text)</param>
        /// <param name="replyToMessageId">The ID of the message being replied to (optional)</param>
        /// <returns>The created message and updated conversation</returns>
        Task<(Message message, Conversation conversation)> CreateMessageAsync(
            Guid conversationId, 
            Guid senderId, 
            string content, 
            MessageType? messageType = null, 
            Guid? replyToMessageId = null);
    }
} 