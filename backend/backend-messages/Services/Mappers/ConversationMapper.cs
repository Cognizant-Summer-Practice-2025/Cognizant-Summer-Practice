using BackendMessages.Models;
using BackendMessages.DTO.Conversation.Request;
using BackendMessages.DTO.Conversation.Response;
using BackendMessages.DTO.Message.Response;

namespace BackendMessages.Services.Mappers
{
    public static class ConversationMapper
    {
        public static ConversationResponse ToResponse(Conversation conversation, Guid currentUserId, string? otherUserName = null, string? otherUserAvatar = null, string? otherUserProfessionalTitle = null, int unreadCount = 0, bool isOnline = false)
        {
            var otherUserId = conversation.InitiatorId == currentUserId ? conversation.ReceiverId : conversation.InitiatorId;
            
            return new ConversationResponse
            {
                Id = conversation.Id,
                InitiatorId = conversation.InitiatorId,
                ReceiverId = conversation.ReceiverId,
                OtherUserId = otherUserId,
                OtherUserName = otherUserName ?? "Unknown User",
                OtherUserAvatar = otherUserAvatar,
                OtherUserProfessionalTitle = otherUserProfessionalTitle,
                LastMessageTimestamp = conversation.LastMessageTimestamp,
                LastMessage = conversation.LastMessage != null ? MessageMapper.ToSummaryResponse(conversation.LastMessage) : null,
                UnreadCount = unreadCount,
                CreatedAt = conversation.CreatedAt,
                UpdatedAt = conversation.UpdatedAt,
                IsOnline = isOnline
            };
        }

        public static ConversationDetailResponse ToDetailResponse(Conversation conversation, Guid currentUserId, string? otherUserName = null, string? otherUserAvatar = null, string? otherUserProfessionalTitle = null, int unreadCount = 0, bool isOnline = false)
        {
            var otherUserId = conversation.InitiatorId == currentUserId ? conversation.ReceiverId : conversation.InitiatorId;
            
            return new ConversationDetailResponse
            {
                Id = conversation.Id,
                InitiatorId = conversation.InitiatorId,
                ReceiverId = conversation.ReceiverId,
                OtherUserId = otherUserId,
                OtherUserName = otherUserName ?? "Unknown User",
                OtherUserAvatar = otherUserAvatar,
                OtherUserProfessionalTitle = otherUserProfessionalTitle,
                LastMessageTimestamp = conversation.LastMessageTimestamp,
                LastMessage = conversation.LastMessage != null ? MessageMapper.ToResponse(conversation.LastMessage) : null,
                UnreadCount = unreadCount,
                CreatedAt = conversation.CreatedAt,
                UpdatedAt = conversation.UpdatedAt,
                IsOnline = isOnline,
                RecentMessages = conversation.Messages?.Select(MessageMapper.ToResponse) ?? new List<MessageResponse>()
            };
        }

        public static Conversation ToEntity(CreateConversationRequest request)
        {
            return new Conversation
            {
                InitiatorId = request.InitiatorId,
                ReceiverId = request.ReceiverId
            };
        }

        public static IEnumerable<ConversationResponse> ToResponseList(IEnumerable<Conversation> conversations, Guid currentUserId)
        {
            return conversations.Select(c => ToResponse(c, currentUserId));
        }

        public static ConversationsPagedResponse ToPagedResponse(
            IEnumerable<Conversation> conversations, 
            Guid currentUserId,
            int totalCount, 
            int pageNumber, 
            int pageSize)
        {
            return new ConversationsPagedResponse
            {
                Conversations = ToResponseList(conversations, currentUserId),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                HasNextPage = (pageNumber * pageSize) < totalCount,
                HasPreviousPage = pageNumber > 1
            };
        }

        public static CreateConversationResponse ToCreateConversationResponse(Conversation conversation, Guid currentUserId, Message? initialMessage = null, bool success = true, string? error = null)
        {
            return new CreateConversationResponse
            {
                Conversation = ToResponse(conversation, currentUserId),
                InitialMessage = initialMessage != null ? MessageMapper.ToResponse(initialMessage) : null,
                Success = success,
                Error = error
            };
        }

        public static DeleteConversationResponse ToDeleteConversationResponse(bool success = true, string? error = null)
        {
            return new DeleteConversationResponse
            {
                Success = success,
                Error = error
            };
        }

        public static ConversationStatsResponse ToStatsResponse(
            int totalConversations, 
            int unreadConversations, 
            int totalMessages, 
            int unreadMessages, 
            DateTime? lastActivity = null)
        {
            return new ConversationStatsResponse
            {
                TotalConversations = totalConversations,
                UnreadConversations = unreadConversations,
                TotalMessages = totalMessages,
                UnreadMessages = unreadMessages,
                LastActivity = lastActivity
            };
        }
    }
} 