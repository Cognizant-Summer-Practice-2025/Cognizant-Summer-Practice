using backend_messages.Data;
using backend_messages.Models;
using backend_messages.DTOs.Requests;
using Microsoft.EntityFrameworkCore;

namespace backend_messages.Data.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly MessageDbContext _context;

        public MessageRepository(MessageDbContext context)
        {
            _context = context;
        }

        public async Task<Message> SendMessageAsync(Message message)
        {
            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<List<Message>> GetMessagesByConversationIdAsync(Guid conversationId)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<Message?> GetMessageByIdAsync(Guid id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<bool> DeleteMessageAsync(Guid id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message == null) return false;

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}