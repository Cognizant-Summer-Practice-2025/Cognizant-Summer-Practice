namespace backend_messages.DTO
{
    // MessageRead Response DTO
    public class MessageReadResponseDto
    {
        public Guid Id { get; set; }
        public Guid MessageId { get; set; }
        public Guid UserId { get; set; }
        public DateTime ReadAt { get; set; }
        public UserCacheDto? User { get; set; }
    }

    // MessageRead Request DTO
    public class MessageReadRequestDto
    {
        public Guid MessageId { get; set; }
        public Guid UserId { get; set; }
    }

    // MessageRead Summary DTO
    public class MessageReadSummaryDto
    {
        public Guid UserId { get; set; }
        public DateTime ReadAt { get; set; }
        public UserCacheDto? User { get; set; }
    }
} 