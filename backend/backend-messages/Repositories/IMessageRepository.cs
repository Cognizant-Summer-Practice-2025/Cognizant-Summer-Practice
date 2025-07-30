using BackendMessages.Models;

namespace BackendMessages.Repositories
{
    public interface IMessageRepository
    {
        Task<Message?> GetByIdAsync(Guid id);
        Task<Message> CreateAsync(Message message);
        Task<Message> UpdateAsync(Message message);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> SoftDeleteAsync(Guid id, Guid userId);
        Task<IEnumerable<Message>> GetByConversationIdAsync(Guid conversationId, int page = 1, int pageSize = 50);
        Task<IEnumerable<Message>> GetByConversationIdAsync(Guid conversationId, DateTime? before = null, DateTime? after = null, int page = 1, int pageSize = 50);
        Task<int> GetTotalCountByConversationIdAsync(Guid conversationId);
        Task<int> GetUnreadCountByConversationIdAsync(Guid conversationId, Guid userId);
        Task<int> GetUnreadCountByUserIdAsync(Guid userId);
        Task<bool> MarkAsReadAsync(Guid messageId, Guid userId);
        Task<int> MarkConversationAsReadAsync(Guid conversationId, Guid userId);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> UserCanAccessMessageAsync(Guid messageId, Guid userId);
        Task<IEnumerable<Message>> GetUnreadMessagesByUserIdAsync(Guid userId);
        Task<Message?> GetLatestMessageInConversationAsync(Guid conversationId);
        Task<bool> UserOwnsMessageAsync(Guid messageId, Guid userId);
    }
} 