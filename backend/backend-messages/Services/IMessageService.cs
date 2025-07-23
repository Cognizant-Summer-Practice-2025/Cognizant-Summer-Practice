using backend_messages.DTOs.Requests;
using backend_messages.DTOs.Responses;

namespace backend_messages.Services
{
    public interface IMessageService
    {
        Task<MessageResponse> SendMessageAsync(SendMessageRequest request);
        Task<List<MessageResponse>> GetMessagesByConversationIdAsync(Guid conversationId);
    }
}