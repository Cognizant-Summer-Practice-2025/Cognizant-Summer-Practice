using backend_messages.Models;

namespace backend_messages.Data.Repositories
{
    public interface IConversationRepository
    {
        Task<Conversation> CreateConversationAsync(Conversation conversation);
        Task<Conversation?> GetConversationByIdAsync(Guid id);
        Task<List<Conversation>> GetAllConversationsAsync();
        Task<List<Conversation>> GetConversationsByUserIdAsync(Guid userId);
    }
}