using backend_messages.DTOs.Requests;
using backend_messages.DTOs.Responses;

namespace backend_messages.Services
{
    public interface IConversationService
    {
        Task<ConversationResponse> CreateConversationAsync(CreateConversationRequest request);
        Task<List<MessageResponse>> GetConversationHistoryAsync(Guid conversationId);
        Task<List<ConversationResponse>> GetUserConversationsAsync(Guid userId);
    }
}