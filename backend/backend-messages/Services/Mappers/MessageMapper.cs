using BackendMessages.Models;
using BackendMessages.DTO.Message.Request;
using BackendMessages.DTO.Message.Response;

namespace BackendMessages.Services.Mappers
{
    public static class MessageMapper
    {
        public static MessageResponse ToResponse(Message message)
        {
            return new MessageResponse
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Content = message.Content ?? string.Empty,
                MessageType = message.MessageType,
                IsRead = message.IsRead,
                CreatedAt = message.CreatedAt,
                UpdatedAt = message.UpdatedAt,
                ReplyToMessageId = message.ReplyToMessageId,
                ReplyToMessage = message.ReplyToMessage != null ? ToResponse(message.ReplyToMessage) : null
            };
        }

        public static MessageSummaryResponse ToSummaryResponse(Message message)
        {
            return new MessageSummaryResponse
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderId = message.SenderId,
                Content = message.Content ?? string.Empty,
                IsRead = message.IsRead,
                CreatedAt = message.CreatedAt
            };
        }

        public static Message ToEntity(SendMessageRequest request)
        {
            return new Message
            {
                ConversationId = request.ConversationId,
                SenderId = request.SenderId,
                ReceiverId = request.ReceiverId,
                Content = request.Content,
                MessageType = MessageType.Text,
                IsRead = false,
                ReplyToMessageId = request.ReplyToMessageId
            };
        }

        public static IEnumerable<MessageResponse> ToResponseList(IEnumerable<Message> messages)
        {
            return messages.Select(ToResponse);
        }

        public static IEnumerable<MessageSummaryResponse> ToSummaryResponseList(IEnumerable<Message> messages)
        {
            return messages.Select(ToSummaryResponse);
        }

        public static MessagesPagedResponse ToPagedResponse(
            IEnumerable<Message> messages, 
            int totalCount, 
            int pageNumber, 
            int pageSize)
        {
            return new MessagesPagedResponse
            {
                Messages = ToResponseList(messages),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                HasNextPage = (pageNumber * pageSize) < totalCount,
                HasPreviousPage = pageNumber > 1
            };
        }

        public static SendMessageResponse ToSendMessageResponse(Message message, bool success = true, string? error = null)
        {
            return new SendMessageResponse
            {
                Message = ToResponse(message),
                Success = success,
                Error = error
            };
        }

        public static MarkAsReadResponse ToMarkAsReadResponse(int messagesMarked, bool success = true, string? error = null)
        {
            return new MarkAsReadResponse
            {
                Success = success,
                MessagesMarked = messagesMarked,
                Error = error
            };
        }

        public static DeleteMessageResponse ToDeleteMessageResponse(bool success = true, string? error = null)
        {
            return new DeleteMessageResponse
            {
                Success = success,
                Error = error
            };
        }
    }
} 