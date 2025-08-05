using BackendMessages.Data;
using BackendMessages.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendMessages.Repositories
{
    public class MessageReportRepository : IMessageReportRepository
    {
        private readonly MessagesDbContext _context;
        private readonly ILogger<MessageReportRepository> _logger;

        public MessageReportRepository(MessagesDbContext context, ILogger<MessageReportRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<MessageReport> CreateReportAsync(Guid messageId, Guid reportedByUserId, string reason)
        {
            try
            {
                // Check if user has already reported this message
                var existingReport = await _context.MessageReports
                    .FirstOrDefaultAsync(mr => mr.MessageId == messageId && mr.ReportedByUserId == reportedByUserId);

                if (existingReport != null)
                {
                    throw new InvalidOperationException("User has already reported this message");
                }

                var report = new MessageReport
                {
                    MessageId = messageId,
                    ReportedByUserId = reportedByUserId,
                    Reason = reason,
                    CreatedAt = DateTime.UtcNow
                };

                _context.MessageReports.Add(report);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Message report created: {ReportId} for message {MessageId} by user {UserId}", 
                    report.Id, messageId, reportedByUserId);

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating message report for message {MessageId} by user {UserId}", 
                    messageId, reportedByUserId);
                throw;
            }
        }

        public async Task<bool> HasUserReportedMessageAsync(Guid messageId, Guid userId)
        {
            return await _context.MessageReports
                .AnyAsync(mr => mr.MessageId == messageId && mr.ReportedByUserId == userId);
        }

        public async Task<IEnumerable<MessageReport>> GetReportsByMessageIdAsync(Guid messageId)
        {
            return await _context.MessageReports
                .Where(mr => mr.MessageId == messageId)
                .Include(mr => mr.Message)
                .OrderByDescending(mr => mr.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<MessageReport>> GetReportsByUserIdAsync(Guid userId)
        {
            return await _context.MessageReports
                .Where(mr => mr.ReportedByUserId == userId)
                .Include(mr => mr.Message)
                .OrderByDescending(mr => mr.CreatedAt)
                .ToListAsync();
        }

        public async Task<MessageReport?> GetReportByIdAsync(Guid reportId)
        {
            return await _context.MessageReports
                .Include(mr => mr.Message)
                .FirstOrDefaultAsync(mr => mr.Id == reportId);
        }
    }
} 