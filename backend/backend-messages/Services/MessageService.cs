using backend_messages.Data;
using backend_messages.DTOs.Requests;
using backend_messages.DTOs.Responses;
using backend_messages.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_messages.Services
{
    public class MessageService : IMessageService
    {
        private readonly MessageDbContext _context;

        public MessageService(MessageDbContext context)
        {
            _context = context;
        }

        public async Task<MessageResponse> SendMessageAsync(SendMessageRequest request)
        {
            Guid conversationId = request.ConversationId ?? Guid.NewGuid();
            
            var message = new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId,
                SenderId = request.SenderId,
                Content = request.Content,
                MessageType = "Text",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            return new MessageResponse
            {
                Id = message.Id,
                SenderId = message.SenderId,
                ReceiverId = request.ReceiverId,
                Content = message.Content ?? string.Empty,
                CreatedAt = message.CreatedAt,
                ConversationId = message.ConversationId,
                IsRead = false
            };
        }

        public async Task<List<MessageResponse>> GetMessagesByConversationIdAsync(Guid conversationId)
        {
            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId && m.DeletedAt == null)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();

            return messages.Select(m => new MessageResponse
            {
                Id = m.Id,
                SenderId = m.SenderId,
                ReceiverId = Guid.Empty,
                Content = m.Content ?? string.Empty,
                CreatedAt = m.CreatedAt,
                ConversationId = m.ConversationId,
                IsRead = false
            }).ToList();
        }
    }
}