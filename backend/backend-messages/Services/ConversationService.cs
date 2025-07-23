using backend_messages.Data.Repositories;
using backend_messages.DTOs.Requests;
using backend_messages.DTOs.Responses;
using backend_messages.Models;
using Microsoft.EntityFrameworkCore;

namespace backend_messages.Services
{
    public class ConversationService : IConversationService
    {
        private readonly IConversationRepository _conversationRepository;

        public ConversationService(IConversationRepository conversationRepository)
        {
            _conversationRepository = conversationRepository;
        }

        public async Task<ConversationResponse> CreateConversationAsync(CreateConversationRequest request)
        {
            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                User1Id = request.User1Id,
                User2Id = request.User2Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _conversationRepository.CreateConversationAsync(conversation);

            return new ConversationResponse
            {
                Id = conversation.Id,
                OtherUserId = conversation.User2Id,
                OtherUserName = "User", // TODO: Get from user service
                OtherUserAvatar = null,
                LastMessage = null,
                UnreadCount = 0,
                UpdatedAt = conversation.UpdatedAt
            };
        }

        public async Task<List<ConversationResponse>> GetUserConversationsAsync(Guid userId)
        {
            var conversations = await _conversationRepository.GetConversationsByUserIdAsync(userId);

            return conversations.Select(c => new ConversationResponse
            {
                Id = c.Id,
                OtherUserId = c.User1Id == userId ? c.User2Id : c.User1Id,
                OtherUserName = "User", // TODO: Get from user service
                OtherUserAvatar = null,
                LastMessage = null, // TODO: Get last message
                UnreadCount = 0, // TODO: Calculate unread count
                UpdatedAt = c.UpdatedAt
            }).ToList();
        }

        public async Task<List<MessageResponse>> GetConversationHistoryAsync(Guid conversationId)
        {
            var conversation = await _conversationRepository.GetConversationByIdAsync(conversationId);
            if (conversation == null)
            {
                return new List<MessageResponse>();
            }

            return conversation.Messages.Select(m => new MessageResponse
            {
                Id = m.Id,
                SenderId = m.SenderId,
                ReceiverId = Guid.Empty, // TODO: Determine from conversation participants
                Content = m.Content ?? string.Empty,
                CreatedAt = m.CreatedAt,
                ConversationId = m.ConversationId,
                IsRead = false // TODO: Determine from message reads
            }).ToList();
        }
    }
}