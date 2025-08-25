using BackendMessages.Models;

namespace BackendMessages.Services.Abstractions
{
    /// <summary>
    /// Service for broadcasting message-related events via SignalR
    /// </summary>
    public interface IMessageBroadcastService
    {
        /// <summary>
        /// Broadcasts a new message to both sender and receiver via SignalR
        /// </summary>
        /// <param name="message">The message to broadcast</param>
        /// <param name="conversation">The conversation the message belongs to</param>
        /// <returns>Task representing the async operation</returns>
        Task BroadcastNewMessageAsync(Message message, Conversation conversation);

        /// <summary>
        /// Broadcasts a conversation update to both participants
        /// </summary>
        /// <param name="conversation">The updated conversation</param>
        /// <param name="lastMessage">The last message in the conversation</param>
        /// <returns>Task representing the async operation</returns>
        Task BroadcastConversationUpdateAsync(Conversation conversation, Message lastMessage);
    }
} 