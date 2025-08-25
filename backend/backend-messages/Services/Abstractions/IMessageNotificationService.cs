using BackendMessages.Models;

namespace BackendMessages.Services.Abstractions
{
    /// <summary>
    /// Service for handling message-related notifications (email, push, etc.)
    /// </summary>
    public interface IMessageNotificationService
    {
        /// <summary>
        /// Sends a contact request notification email when a user initiates their first message in a conversation
        /// </summary>
        /// <param name="conversationId">The conversation ID</param>
        /// <param name="senderId">The sender's user ID</param>
        /// <param name="receiverId">The receiver's user ID</param>
        /// <param name="isFirstMessageFromInitiator">Whether this is the first message from the conversation initiator</param>
        /// <returns>Task representing the async operation</returns>
        Task SendContactRequestNotificationAsync(Guid conversationId, Guid senderId, Guid receiverId, bool isFirstMessageFromInitiator);
    }
} 