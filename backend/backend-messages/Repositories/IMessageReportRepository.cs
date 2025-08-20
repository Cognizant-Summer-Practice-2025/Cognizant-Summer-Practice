using BackendMessages.Models;

namespace BackendMessages.Repositories
{
    public interface IMessageReportRepository
    {
        Task<MessageReport> CreateReportAsync(Guid messageId, Guid reportedByUserId, string reason);
        Task<bool> HasUserReportedMessageAsync(Guid messageId, Guid userId);
        Task<IEnumerable<MessageReport>> GetReportsByMessageIdAsync(Guid messageId);
        Task<IEnumerable<MessageReport>> GetReportsByUserIdAsync(Guid userId);
        Task<MessageReport?> GetReportByIdAsync(Guid reportId);
        Task<IEnumerable<MessageReport>> GetAllMessageReportsAsync();
    }
} 