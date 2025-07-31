using BackendMessages.Models;

namespace BackendMessages.Repositories
{
    public interface IConversationRepository
    {
        Task<Conversation?> GetByIdAsync(Guid id);
        Task<Conversation> CreateAsync(Conversation conversation);
        Task<Conversation> UpdateAsync(Conversation conversation);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> SoftDeleteAsync(Guid conversationId, Guid userId);
        Task<IEnumerable<Conversation>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20, bool includeDeleted = false);
        Task<int> GetTotalCountByUserIdAsync(Guid userId, bool includeDeleted = false);
        Task<Conversation?> GetConversationBetweenUsersAsync(Guid user1Id, Guid user2Id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> UserCanAccessConversationAsync(Guid conversationId, Guid userId);
        Task<bool> UpdateLastMessageAsync(Guid conversationId, Guid messageId);
        Task<IEnumerable<Conversation>> GetActiveConversationsByUserIdAsync(Guid userId);
        Task<int> GetUnreadConversationCountAsync(Guid userId);
        Task<bool> IsConversationDeletedByUserAsync(Guid conversationId, Guid userId);
        Task<Conversation?> GetByIdWithMessagesAsync(Guid id, int messageCount = 20);
    }
} 