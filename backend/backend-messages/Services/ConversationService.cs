using BackendMessages.Data;
using BackendMessages.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendMessages.Services
{
    public class ConversationService : IConversationService
    {
        private readonly MessagesDbContext _context;
        private readonly ILogger<ConversationService> _logger;

        public ConversationService(MessagesDbContext context, ILogger<ConversationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> DeleteConversationAsync(Guid conversationId, Guid userId)
        {
            _logger.LogInformation("ConversationService.DeleteConversationAsync called for conversation {ConversationId} by user {UserId} (soft delete)", 
                conversationId, userId);

            using var transaction = await _context.Database.BeginTransactionAsync();
            _logger.LogInformation("Database transaction started");
            
            try
            {
                // Verify conversation exists and user has permission
                _logger.LogInformation("Looking up conversation {ConversationId}...", conversationId);
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.Id == conversationId);

                if (conversation == null)
                {
                    _logger.LogWarning("Conversation {ConversationId} not found in database", conversationId);
                    await transaction.RollbackAsync();
                    return false;
                }
                _logger.LogInformation("Conversation found: InitiatorId={InitiatorId}, ReceiverId={ReceiverId}", 
                    conversation.InitiatorId, conversation.ReceiverId);

                _logger.LogInformation("Checking if user {UserId} is part of conversation {ConversationId}...", userId, conversationId);
                if (!await IsUserPartOfConversationAsync(conversationId, userId))
                {
                    _logger.LogWarning("User {UserId} attempted to delete conversation {ConversationId} without permission", 
                        userId, conversationId);
                    await transaction.RollbackAsync();
                    return false;
                }
                _logger.LogInformation("User permission verified");

                // Check if user has already deleted this conversation
                if (conversation.IsDeletedByUser(userId))
                {
                    _logger.LogInformation("Conversation {ConversationId} already deleted by user {UserId}", conversationId, userId);
                    await transaction.CommitAsync();
                    return true;
                }

                // SOFT DELETE: Mark conversation as deleted for this user only
                _logger.LogInformation("Marking conversation as deleted for user {UserId}...", userId);
                var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                
                if (conversation.InitiatorId == userId)
                {
                    _logger.LogInformation("User {UserId} is the initiator, setting InitiatorDeletedAt", userId);
                    conversation.InitiatorDeletedAt = now;
                    _logger.LogInformation("Set InitiatorDeletedAt to {Timestamp}", now);
                }
                else if (conversation.ReceiverId == userId)
                {
                    _logger.LogInformation("User {UserId} is the receiver, setting ReceiverDeletedAt", userId);
                    conversation.ReceiverDeletedAt = now;
                    _logger.LogInformation("Set ReceiverDeletedAt to {Timestamp}", now);
                }
                else
                {
                    _logger.LogError("User {UserId} is neither initiator ({InitiatorId}) nor receiver ({ReceiverId})", 
                        userId, conversation.InitiatorId, conversation.ReceiverId);
                    await transaction.RollbackAsync();
                    return false;
                }

                conversation.UpdatedAt = now;
                _context.Conversations.Update(conversation);

                // Check if both users have deleted the conversation
                if (conversation.IsDeletedByBothUsers())
                {
                    _logger.LogInformation("Both users have deleted conversation {ConversationId}, performing hard delete...", conversationId);
                    
                    // Hard delete - remove conversation and all messages permanently
                    var messages = await _context.Messages
                        .Where(m => m.ConversationId == conversationId)
                        .ToListAsync();
                    
                    if (messages.Any())
                    {
                        _logger.LogInformation("Removing {MessageCount} messages permanently...", messages.Count);
                        _context.Messages.RemoveRange(messages);
                    }
                    
                    _context.Conversations.Remove(conversation);
                    _logger.LogInformation("Conversation marked for permanent deletion");
                }

                // Save changes
                _logger.LogInformation("Saving changes to database...");
                await _context.SaveChangesAsync();
                _logger.LogInformation("Changes saved successfully");
                
                // Commit transaction
                _logger.LogInformation("Committing transaction...");
                await transaction.CommitAsync();
                _logger.LogInformation("Transaction committed successfully");

                _logger.LogInformation("Successfully soft-deleted conversation {ConversationId} for user {UserId}", 
                    conversationId, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception caught in DeleteConversationAsync, rolling back transaction...");
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting conversation {ConversationId} for user {UserId}. " +
                    "Exception type: {ExceptionType}, Message: {ExceptionMessage}, StackTrace: {StackTrace}", 
                    conversationId, userId, ex.GetType().Name, ex.Message, ex.StackTrace);
                return false;
            }
        }

        public async Task<bool> IsUserPartOfConversationAsync(Guid conversationId, Guid userId)
        {
            return await _context.Conversations
                .AnyAsync(c => c.Id == conversationId && 
                              (c.InitiatorId == userId || c.ReceiverId == userId));
        }

        public async Task<int> GetMessageCountInConversationAsync(Guid conversationId)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .CountAsync();
        }

        public async Task<bool> RestoreConversationForUserAsync(Guid conversationId, Guid userId)
        {
            _logger.LogInformation("Restoring conversation {ConversationId} for user {UserId}", conversationId, userId);
            
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null)
            {
                _logger.LogWarning("Conversation {ConversationId} not found for restore", conversationId);
                return false;
            }

            var wasRestored = false;
            
            if (conversation.InitiatorId == userId && conversation.InitiatorDeletedAt.HasValue)
            {
                conversation.InitiatorDeletedAt = null;
                wasRestored = true;
                _logger.LogInformation("Restored conversation for initiator");
            }
            else if (conversation.ReceiverId == userId && conversation.ReceiverDeletedAt.HasValue)
            {
                conversation.ReceiverDeletedAt = null;
                wasRestored = true;
                _logger.LogInformation("Restored conversation for receiver");
            }

            if (wasRestored)
            {
                conversation.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                _context.Conversations.Update(conversation);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Conversation {ConversationId} restored for user {UserId}", conversationId, userId);
            }

            return wasRestored;
        }
    }
} 