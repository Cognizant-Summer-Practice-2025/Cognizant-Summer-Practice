using backend_messages.Data;
using backend_messages.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_messages.Data.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly MessageDbContext _context;

        public ConversationRepository(MessageDbContext context)
        {
            _context = context;
        }

        public async Task<Conversation> CreateConversationAsync(Conversation conversation)
        {
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync();
            return conversation;
        }

        public async Task<Conversation?> GetConversationByIdAsync(Guid id)
        {
            return await _context.Conversations
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Conversation>> GetConversationsByUserIdAsync(Guid userId)
        {
            return await _context.Conversations
                .Where(c => c.User1Id == userId || c.User2Id == userId)
                .Include(c => c.Messages)
                .ToListAsync();
        }

        public async Task<List<Conversation>> GetAllConversationsAsync()
        {
            return await _context.Conversations
                .Include(c => c.Messages)
                .ToListAsync();
        }
    }
}