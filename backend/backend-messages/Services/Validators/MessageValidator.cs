using BackendMessages.DTO.Message.Request;

namespace BackendMessages.Services.Validators
{
    public interface IValidationService<T>
    {
        (bool IsValid, string? ErrorMessage) Validate(T request);
    }

    public class SendMessageValidator : IValidationService<SendMessageRequest>
    {
        public (bool IsValid, string? ErrorMessage) Validate(SendMessageRequest request)
        {
            if (request == null)
                return (false, "Request cannot be null");

            if (request.ConversationId == Guid.Empty)
                return (false, "ConversationId is required");

            if (request.SenderId == Guid.Empty)
                return (false, "SenderId is required");

            if (request.ReceiverId == Guid.Empty)
                return (false, "ReceiverId is required");

            if (string.IsNullOrWhiteSpace(request.Content))
                return (false, "Message content cannot be empty");

            if (request.Content.Length > 5000)
                return (false, "Message content cannot exceed 5000 characters");

            if (request.SenderId == request.ReceiverId)
                return (false, "Cannot send message to yourself");

            return (true, null);
        }
    }

    public class MarkMessageAsReadValidator : IValidationService<MarkMessageAsReadRequest>
    {
        public (bool IsValid, string? ErrorMessage) Validate(MarkMessageAsReadRequest request)
        {
            if (request == null)
                return (false, "Request cannot be null");

            if (request.MessageId == Guid.Empty)
                return (false, "MessageId is required");

            if (request.UserId == Guid.Empty)
                return (false, "UserId is required");

            return (true, null);
        }
    }

    public class MarkMessagesAsReadValidator : IValidationService<MarkMessagesAsReadRequest>
    {
        public (bool IsValid, string? ErrorMessage) Validate(MarkMessagesAsReadRequest request)
        {
            if (request == null)
                return (false, "Request cannot be null");

            if (request.ConversationId == Guid.Empty)
                return (false, "ConversationId is required");

            if (request.UserId == Guid.Empty)
                return (false, "UserId is required");

            return (true, null);
        }
    }

    public class DeleteMessageValidator : IValidationService<DeleteMessageRequest>
    {
        public (bool IsValid, string? ErrorMessage) Validate(DeleteMessageRequest request)
        {
            if (request == null)
                return (false, "Request cannot be null");

            if (request.MessageId == Guid.Empty)
                return (false, "MessageId is required");

            if (request.UserId == Guid.Empty)
                return (false, "UserId is required");

            return (true, null);
        }
    }

    public class ReportMessageValidator : IValidationService<ReportMessageRequest>
    {
        public (bool IsValid, string? ErrorMessage) Validate(ReportMessageRequest request)
        {
            if (request == null)
                return (false, "Request cannot be null");

            if (request.MessageId == Guid.Empty)
                return (false, "MessageId is required");

            if (request.ReportedById == Guid.Empty)
                return (false, "ReportedById is required");

            if (string.IsNullOrWhiteSpace(request.Reason))
                return (false, "Reason is required");

            if (request.Reason.Length < 10)
                return (false, "Reason must be at least 10 characters");

            if (request.Reason.Length > 500)
                return (false, "Reason cannot exceed 500 characters");

            return (true, null);
        }
    }
} 