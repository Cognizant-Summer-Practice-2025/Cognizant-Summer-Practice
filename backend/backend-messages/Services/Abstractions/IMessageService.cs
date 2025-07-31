using BackendMessages.DTO.Message.Request;
using BackendMessages.DTO.Message.Response;

namespace BackendMessages.Services.Abstractions
{
    public interface IMessageService
    {
        Task<SendMessageResponse> SendMessageAsync(SendMessageRequest request);
        Task<MessageResponse?> GetMessageByIdAsync(Guid messageId, Guid userId);
        Task<MessagesPagedResponse> GetConversationMessagesAsync(Guid conversationId, Guid userId, int page = 1, int pageSize = 50, DateTime? before = null, DateTime? after = null);
        Task<MarkAsReadResponse> MarkMessageAsReadAsync(MarkMessageAsReadRequest request);
        Task<MarkAsReadResponse> MarkConversationAsReadAsync(MarkMessagesAsReadRequest request);
        Task<DeleteMessageResponse> DeleteMessageAsync(DeleteMessageRequest request);
        Task<DeleteMessageResponse> ReportMessageAsync(ReportMessageRequest request);
        Task<IEnumerable<MessageResponse>> GetUnreadMessagesAsync(Guid userId);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<bool> UserCanAccessMessageAsync(Guid messageId, Guid userId);
        Task<bool> UserOwnsMessageAsync(Guid messageId, Guid userId);
    }
} 