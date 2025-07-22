using backend_messages.Models;

namespace backend_messages.Data.Repositories
{
    public interface IMessageRepository
    {
        Task<Message> SendMessageAsync(Message message);
        Task<List<Message>> GetMessagesByConversationIdAsync(Guid conversationId);
        Task<Message?> GetMessageByIdAsync(Guid messageId);
        Task<bool> DeleteMessageAsync(Guid messageId);
    }
}