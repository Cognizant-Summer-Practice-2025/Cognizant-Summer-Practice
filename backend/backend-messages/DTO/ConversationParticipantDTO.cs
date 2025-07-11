using backend_messages.Models;

namespace backend_messages.DTO
{
    // ConversationParticipant Response DTO
    public class ConversationParticipantResponseDto
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid UserId { get; set; }
        public Guid? LastReadMessageId { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public bool IsMuted { get; set; }
        public ParticipantRole Role { get; set; }
        public UserCacheDto? UserInfo { get; set; }
    }

    // ConversationParticipant Request DTO
    public class ConversationParticipantRequestDto
    {
        public Guid ConversationId { get; set; }
        public Guid UserId { get; set; }
        public ParticipantRole Role { get; set; } = ParticipantRole.Member;
    }

    // ConversationParticipant Summary DTO
    public class ConversationParticipantSummaryDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public ParticipantRole Role { get; set; }
        public bool IsMuted { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public UserCacheDto? UserInfo { get; set; }
    }

    // ConversationParticipant Update DTO
    public class ConversationParticipantUpdateDto
    {
        public bool? IsMuted { get; set; }
        public ParticipantRole? Role { get; set; }
        public Guid? LastReadMessageId { get; set; }
    }
} 