using System.ComponentModel.DataAnnotations;

namespace BackendMessages.DTO.Conversation.Request
{
    public class CreateConversationRequest
    {
        [Required]
        public Guid InitiatorId { get; set; }

        [Required]
        public Guid ReceiverId { get; set; }

        [StringLength(5000)]
        public string? InitialMessage { get; set; }
    }

    public class DeleteConversationRequest
    {
        [Required]
        public Guid ConversationId { get; set; }

        [Required]
        public Guid UserId { get; set; }
    }

    public class GetConversationsRequest
    {
        [Required]
        public Guid UserId { get; set; }

        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;

        public bool IncludeDeleted { get; set; } = false;
    }

    public class GetConversationMessagesRequest
    {
        [Required]
        public Guid ConversationId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 50;

        public DateTime? Before { get; set; }
        public DateTime? After { get; set; }
    }
} 