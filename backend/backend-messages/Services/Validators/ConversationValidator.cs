using BackendMessages.DTO.Conversation.Request;

namespace BackendMessages.Services.Validators
{
    public class CreateConversationValidator : IValidationService<CreateConversationRequest>
    {
        public (bool IsValid, string? ErrorMessage) Validate(CreateConversationRequest request)
        {
            if (request == null)
                return (false, "Request cannot be null");

            if (request.InitiatorId == Guid.Empty)
                return (false, "InitiatorId is required");

            if (request.ReceiverId == Guid.Empty)
                return (false, "ReceiverId is required");

            if (request.InitiatorId == request.ReceiverId)
                return (false, "Cannot create conversation with yourself");

            if (!string.IsNullOrEmpty(request.InitialMessage))
            {
                if (request.InitialMessage.Length > 5000)
                    return (false, "Initial message cannot exceed 5000 characters");
            }

            return (true, null);
        }
    }

    public class DeleteConversationValidator : IValidationService<DeleteConversationRequest>
    {
        public (bool IsValid, string? ErrorMessage) Validate(DeleteConversationRequest request)
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

    public class GetConversationsValidator : IValidationService<GetConversationsRequest>
    {
        public (bool IsValid, string? ErrorMessage) Validate(GetConversationsRequest request)
        {
            if (request == null)
                return (false, "Request cannot be null");

            if (request.UserId == Guid.Empty)
                return (false, "UserId is required");

            if (request.Page < 1)
                return (false, "Page must be greater than 0");

            if (request.PageSize < 1 || request.PageSize > 100)
                return (false, "PageSize must be between 1 and 100");

            return (true, null);
        }
    }

    public class GetConversationMessagesValidator : IValidationService<GetConversationMessagesRequest>
    {
        public (bool IsValid, string? ErrorMessage) Validate(GetConversationMessagesRequest request)
        {
            if (request == null)
                return (false, "Request cannot be null");

            if (request.ConversationId == Guid.Empty)
                return (false, "ConversationId is required");

            if (request.UserId == Guid.Empty)
                return (false, "UserId is required");

            if (request.Page < 1)
                return (false, "Page must be greater than 0");

            if (request.PageSize < 1 || request.PageSize > 100)
                return (false, "PageSize must be between 1 and 100");

            if (request.Before.HasValue && request.After.HasValue && request.Before <= request.After)
                return (false, "Before date must be after the After date");

            return (true, null);
        }
    }
} 