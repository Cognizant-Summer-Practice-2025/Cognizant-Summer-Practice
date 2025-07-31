using BackendMessages.DTO.Conversation.Request;
using BackendMessages.DTO.Conversation.Response;

namespace BackendMessages.Services.Abstractions
{
    public interface IConversationService
    {
        Task<CreateConversationResponse> CreateConversationAsync(CreateConversationRequest request);
        Task<ConversationResponse?> GetConversationByIdAsync(Guid conversationId, Guid userId);
        Task<ConversationDetailResponse?> GetConversationDetailAsync(Guid conversationId, Guid userId);
        Task<ConversationsPagedResponse> GetUserConversationsAsync(GetConversationsRequest request);
        Task<DeleteConversationResponse> DeleteConversationAsync(DeleteConversationRequest request);
        Task<ConversationResponse?> GetOrCreateConversationAsync(Guid user1Id, Guid user2Id);
        Task<ConversationStatsResponse> GetConversationStatsAsync(Guid userId);
        Task<bool> UserCanAccessConversationAsync(Guid conversationId, Guid userId);
        Task<bool> IsConversationDeletedByUserAsync(Guid conversationId, Guid userId);
        Task<int> GetUnreadConversationCountAsync(Guid userId);
        Task UpdateLastMessageAsync(Guid conversationId, Guid messageId);
    }
} 