using BackendMessages.Models;

namespace BackendMessages.DTO.Message.Response
{
    public class MessageReportResponseDto
    {
        public Guid Id { get; set; }
        public Guid MessageId { get; set; }
        public Guid ReportedByUserId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public MessageReportDetailsDto? Message { get; set; }
    }

    public class MessageReportDetailsDto
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Content { get; set; } = string.Empty;
        public MessageType MessageType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 