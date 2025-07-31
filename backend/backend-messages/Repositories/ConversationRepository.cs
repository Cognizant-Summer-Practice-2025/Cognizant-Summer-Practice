using Microsoft.EntityFrameworkCore;
using BackendMessages.Data;
using BackendMessages.Models;

namespace BackendMessages.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly MessagesDbContext _context;
        private readonly ILogger<ConversationRepository> _logger;

        public ConversationRepository(MessagesDbContext context, ILogger<ConversationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Conversation?> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.Conversations
                    .Include(c => c.LastMessage)
                    .FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation by id {ConversationId}", id);
                throw;
            }
        }

        public async Task<Conversation> CreateAsync(Conversation conversation)
        {
            try
            {
                conversation.CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                conversation.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                conversation.LastMessageTimestamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
                
                return conversation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation");
                throw;
            }
        }

        public async Task<Conversation> UpdateAsync(Conversation conversation)
        {
            try
            {
                conversation.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                
                _context.Conversations.Update(conversation);
                await _context.SaveChangesAsync();
                
                return conversation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating conversation {ConversationId}", conversation.Id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var conversation = await _context.Conversations.FindAsync(id);
                if (conversation == null) return false;

                _context.Conversations.Remove(conversation);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hard deleting conversation {ConversationId}", id);
                throw;
            }
        }

        public async Task<bool> SoftDeleteAsync(Guid conversationId, Guid userId)
        {
            try
            {
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.Id == conversationId);
                
                if (conversation == null) return false;

                var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                
                if (userId == conversation.InitiatorId)
                {
                    conversation.InitiatorDeletedAt = now;
                }
                else if (userId == conversation.ReceiverId)
                {
                    conversation.ReceiverDeletedAt = now;
                }
                else
                {
                    return false; // User is not part of this conversation
                }

                conversation.UpdatedAt = now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting conversation {ConversationId} by user {UserId}", conversationId, userId);
                throw;
            }
        }

        public async Task<IEnumerable<Conversation>> GetByUserIdAsync(Guid userId, int page = 1, int pageSize = 20, bool includeDeleted = false)
        {
            try
            {
                var query = _context.Conversations
                    .Include(c => c.LastMessage)
                    .Where(c => c.InitiatorId == userId || c.ReceiverId == userId);

                if (!includeDeleted)
                {
                    query = query.Where(c => !c.IsDeletedByUser(userId));
                }

                return await query
                    .OrderByDescending(c => c.LastMessageTimestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversations for user {UserId}", userId);
                throw;
            }
        }

        public async Task<int> GetTotalCountByUserIdAsync(Guid userId, bool includeDeleted = false)
        {
            try
            {
                var query = _context.Conversations
                    .Where(c => c.InitiatorId == userId || c.ReceiverId == userId);

                if (!includeDeleted)
                {
                    query = query.Where(c => !c.IsDeletedByUser(userId));
                }

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total conversation count for user {UserId}", userId);
                throw;
            }
        }

        public async Task<Conversation?> GetConversationBetweenUsersAsync(Guid user1Id, Guid user2Id)
        {
            try
            {
                return await _context.Conversations
                    .Include(c => c.LastMessage)
                    .FirstOrDefaultAsync(c => 
                        (c.InitiatorId == user1Id && c.ReceiverId == user2Id) ||
                        (c.InitiatorId == user2Id && c.ReceiverId == user1Id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation between users {User1Id} and {User2Id}", user1Id, user2Id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            try
            {
                return await _context.Conversations.AnyAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if conversation {ConversationId} exists", id);
                throw;
            }
        }

        public async Task<bool> UserCanAccessConversationAsync(Guid conversationId, Guid userId)
        {
            try
            {
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.Id == conversationId);

                if (conversation == null) return false;

                return (conversation.InitiatorId == userId || conversation.ReceiverId == userId) 
                       && !conversation.IsDeletedByUser(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} can access conversation {ConversationId}", userId, conversationId);
                throw;
            }
        }

        public async Task<bool> UpdateLastMessageAsync(Guid conversationId, Guid messageId)
        {
            try
            {
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.Id == conversationId);

                if (conversation == null) return false;

                var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                conversation.LastMessageId = messageId;
                conversation.LastMessageTimestamp = now;
                conversation.UpdatedAt = now;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating last message for conversation {ConversationId}", conversationId);
                throw;
            }
        }

        public async Task<IEnumerable<Conversation>> GetActiveConversationsByUserIdAsync(Guid userId)
        {
            try
            {
                return await _context.Conversations
                    .Include(c => c.LastMessage)
                    .Where(c => (c.InitiatorId == userId || c.ReceiverId == userId) 
                             && !c.IsDeletedByUser(userId))
                    .OrderByDescending(c => c.LastMessageTimestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active conversations for user {UserId}", userId);
                throw;
            }
        }

        public async Task<int> GetUnreadConversationCountAsync(Guid userId)
        {
            try
            {
                return await _context.Conversations
                    .Where(c => (c.InitiatorId == userId || c.ReceiverId == userId) 
                             && !c.IsDeletedByUser(userId))
                    .Where(c => c.Messages.Any(m => m.ReceiverId == userId && !m.IsRead && m.DeletedAt == null))
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread conversation count for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> IsConversationDeletedByUserAsync(Guid conversationId, Guid userId)
        {
            try
            {
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.Id == conversationId);

                return conversation?.IsDeletedByUser(userId) ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if conversation {ConversationId} is deleted by user {UserId}", conversationId, userId);
                throw;
            }
        }

        public async Task<Conversation?> GetByIdWithMessagesAsync(Guid id, int messageCount = 20)
        {
            try
            {
                return await _context.Conversations
                    .Include(c => c.LastMessage)
                    .Include(c => c.Messages.Where(m => m.DeletedAt == null)
                        .OrderByDescending(m => m.CreatedAt)
                        .Take(messageCount))
                    .ThenInclude(m => m.ReplyToMessage)
                    .FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation {ConversationId} with messages", id);
                throw;
            }
        }
    }
} 