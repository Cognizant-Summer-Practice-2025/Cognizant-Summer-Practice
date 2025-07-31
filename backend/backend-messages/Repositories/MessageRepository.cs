using Microsoft.EntityFrameworkCore;
using BackendMessages.Data;
using BackendMessages.Models;

namespace BackendMessages.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly MessagesDbContext _context;
        private readonly ILogger<MessageRepository> _logger;

        public MessageRepository(MessagesDbContext context, ILogger<MessageRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Message?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Messages
                    .Include(m => m.ReplyToMessage)
                    .FirstOrDefaultAsync(m => m.Id == id && m.DeletedAt == null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting message by id {MessageId}", id);
                throw;
            }
        }

        public async Task<Message> CreateAsync(Message message)
        {
            try
            {
                message.CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                message.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();
                
                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating message");
                throw;
            }
        }

        public async Task<Message> UpdateAsync(Message message)
        {
            try
            {
                message.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                
                _context.Messages.Update(message);
                await _context.SaveChangesAsync();
                
                return message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating message {MessageId}", message.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var message = await _context.Messages.FindAsync(id);
                if (message == null) return false;

                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hard deleting message {MessageId}", id);
                throw;
            }
        }

        public async Task<bool> SoftDeleteAsync(Guid id, Guid userId)
        {
            try
            {
                var message = await _context.Messages
                    .FirstOrDefaultAsync(m => m.Id == id && m.SenderId == userId && m.DeletedAt == null);
                
                if (message == null) return false;

                message.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                message.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting message {MessageId} by user {UserId}", id, userId);
                throw;
            }
        }

        public async Task<IEnumerable<Message>> GetByConversationIdAsync(Guid conversationId, int page = 1, int pageSize = 50)
        {
            try
            {
                return await _context.Messages
                    .Include(m => m.ReplyToMessage)
                    .Where(m => m.ConversationId == conversationId && m.DeletedAt == null)
                    .OrderByDescending(m => m.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for conversation {ConversationId}", conversationId);
                throw;
            }
        }

        public async Task<IEnumerable<Message>> GetByConversationIdAsync(Guid conversationId, DateTime? before = null, DateTime? after = null, int page = 1, int pageSize = 50)
        {
            try
            {
                var query = _context.Messages
                    .Include(m => m.ReplyToMessage)
                    .Where(m => m.ConversationId == conversationId && m.DeletedAt == null);

                if (before.HasValue)
                    query = query.Where(m => m.CreatedAt < before.Value);

                if (after.HasValue)
                    query = query.Where(m => m.CreatedAt > after.Value);

                return await query
                    .OrderByDescending(m => m.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for conversation {ConversationId} with date filters", conversationId);
                throw;
            }
        }

        public async Task<int> GetTotalCountByConversationIdAsync(Guid conversationId)
        {
            try
            {
                return await _context.Messages
                    .CountAsync(m => m.ConversationId == conversationId && m.DeletedAt == null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total count for conversation {ConversationId}", conversationId);
                throw;
            }
        }

        public async Task<int> GetUnreadCountByConversationIdAsync(Guid conversationId, Guid userId)
        {
            try
            {
                return await _context.Messages
                    .CountAsync(m => m.ConversationId == conversationId 
                                  && m.ReceiverId == userId 
                                  && !m.IsRead 
                                  && m.DeletedAt == null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for conversation {ConversationId} and user {UserId}", conversationId, userId);
                throw;
            }
        }

        public async Task<int> GetUnreadCountByUserIdAsync(Guid userId)
        {
            try
            {
                return await _context.Messages
                    .CountAsync(m => m.ReceiverId == userId && !m.IsRead && m.DeletedAt == null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> MarkAsReadAsync(Guid messageId, Guid userId)
        {
            try
            {
                var message = await _context.Messages
                    .FirstOrDefaultAsync(m => m.Id == messageId && m.ReceiverId == userId && m.DeletedAt == null);

                if (message == null || message.IsRead) return false;

                message.IsRead = true;
                message.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message {MessageId} as read by user {UserId}", messageId, userId);
                throw;
            }
        }

        public async Task<int> MarkConversationAsReadAsync(Guid conversationId, Guid userId)
        {
            try
            {
                var unreadMessages = await _context.Messages
                    .Where(m => m.ConversationId == conversationId 
                             && m.ReceiverId == userId 
                             && !m.IsRead 
                             && m.DeletedAt == null)
                    .ToListAsync();

                var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                    message.UpdatedAt = now;
                }

                await _context.SaveChangesAsync();
                return unreadMessages.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking conversation {ConversationId} as read by user {UserId}", conversationId, userId);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            try
            {
                return await _context.Messages
                    .AnyAsync(m => m.Id == id && m.DeletedAt == null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if message {MessageId} exists", id);
                throw;
            }
        }

        public async Task<bool> UserCanAccessMessageAsync(Guid messageId, Guid userId)
        {
            try
            {
                return await _context.Messages
                    .AnyAsync(m => m.Id == messageId 
                                && (m.SenderId == userId || m.ReceiverId == userId) 
                                && m.DeletedAt == null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} can access message {MessageId}", userId, messageId);
                throw;
            }
        }

        public async Task<IEnumerable<Message>> GetUnreadMessagesByUserIdAsync(Guid userId)
        {
            try
            {
                return await _context.Messages
                    .Include(m => m.ReplyToMessage)
                    .Where(m => m.ReceiverId == userId && !m.IsRead && m.DeletedAt == null)
                    .OrderByDescending(m => m.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread messages for user {UserId}", userId);
                throw;
            }
        }

        public async Task<Message?> GetLatestMessageInConversationAsync(Guid conversationId)
        {
            try
            {
                return await _context.Messages
                    .Where(m => m.ConversationId == conversationId && m.DeletedAt == null)
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest message for conversation {ConversationId}", conversationId);
                throw;
            }
        }

        public async Task<bool> UserOwnsMessageAsync(Guid messageId, Guid userId)
        {
            try
            {
                return await _context.Messages
                    .AnyAsync(m => m.Id == messageId && m.SenderId == userId && m.DeletedAt == null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} owns message {MessageId}", userId, messageId);
                throw;
            }
        }
    }
} 