using BackendMessages.Models;

namespace BackendMessages.Services
{
    public interface IConversationService
    {
        Task<bool> DeleteConversationAsync(Guid conversationId, Guid userId);
        Task<bool> IsUserPartOfConversationAsync(Guid conversationId, Guid userId);
        Task<int> GetMessageCountInConversationAsync(Guid conversationId);
        Task<bool> RestoreConversationForUserAsync(Guid conversationId, Guid userId);
    }
} 