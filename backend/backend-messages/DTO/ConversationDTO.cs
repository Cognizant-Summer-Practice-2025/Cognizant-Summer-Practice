using backend_messages.Models;

namespace backend_messages.DTO
{
    // Conversation Response DTO
    public class ConversationResponseDto
    {
        public Guid Id { get; set; }
        public Guid? LastMessageId { get; set; }
        public ConversationType Type { get; set; }
        public string? Title { get; set; }
        public DateTime LastActivityAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public MessageSummaryDto? LastMessage { get; set; }
        public List<ConversationParticipantSummaryDto> Participants { get; set; } = new();
        public int UnreadCount { get; set; }
    }

    // Conversation Request DTO
    public class ConversationRequestDto
    {
        public ConversationType Type { get; set; }
        public string? Title { get; set; }
        public List<Guid> ParticipantUserIds { get; set; } = new();
    }

    // Conversation Summary DTO (for list views)
    public class ConversationSummaryDto
    {
        public Guid Id { get; set; }
        public ConversationType Type { get; set; }
        public string? Title { get; set; }
        public DateTime LastActivityAt { get; set; }
        public MessageSummaryDto? LastMessage { get; set; }
        public int ParticipantCount { get; set; }
        public int UnreadCount { get; set; }
    }

    // Conversation Update DTO
    public class ConversationUpdateDto
    {
        public string? Title { get; set; }
    }
} 