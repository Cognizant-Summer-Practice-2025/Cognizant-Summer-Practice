using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using BackendMessages.Data;
using BackendMessages.Models;
using BackendMessages.Hubs;

namespace BackendMessages.Controllers
{
    [ApiController]
    [Route("api/messages")]
    public class MessagesController : ControllerBase
    {
        private readonly MessagesDbContext _context;
        private readonly ILogger<MessagesController> _logger;
        private readonly IHubContext<MessageHub> _hubContext;

        public MessagesController(MessagesDbContext context, ILogger<MessagesController> logger, IHubContext<MessageHub> hubContext)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Send a message in a conversation
        /// </summary>
        /// <param name="request">Message details</param>
        /// <returns>Created message</returns>
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Content))
                {
                    return BadRequest("Message content cannot be empty");
                }

                // Check if conversation exists
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.Id == request.ConversationId);

                if (conversation == null)
                {
                    return NotFound("Conversation not found");
                }

                // Verify user is part of the conversation
                if (conversation.InitiatorId != request.SenderId && conversation.ReceiverId != request.SenderId)
                {
                    return StatusCode(403, "User is not part of this conversation");
                }

                // Determine receiver ID
                var receiverId = conversation.InitiatorId == request.SenderId 
                    ? conversation.ReceiverId 
                    : conversation.InitiatorId;

                // Create message
                var message = new Message
                {
                    ConversationId = request.ConversationId,
                    SenderId = request.SenderId,
                    ReceiverId = receiverId,
                    Content = request.Content,
                    MessageType = request.MessageType ?? MessageType.Text,
                    ReplyToMessageId = request.ReplyToMessageId,
                    IsRead = false,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                };

                _context.Messages.Add(message);

                // Update conversation's last message
                conversation.LastMessageId = message.Id;
                conversation.LastMessageTimestamp = message.CreatedAt;
                conversation.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

                await _context.SaveChangesAsync();

                // Create response object
                var messageResponse = new
                {
                    message.Id,
                    message.ConversationId,
                    message.SenderId,
                    message.ReceiverId,
                    message.Content,
                    message.MessageType,
                    message.ReplyToMessageId,
                    message.IsRead,
                    message.CreatedAt,
                    message.UpdatedAt
                };

                // Broadcast the new message to both sender and receiver
                try
                {
                    // Send to receiver
                    await _hubContext.Clients.Group($"user_{receiverId}")
                        .SendAsync("ReceiveMessage", messageResponse);

                    // Send to sender (for multi-device support)
                    await _hubContext.Clients.Group($"user_{request.SenderId}")
                        .SendAsync("ReceiveMessage", messageResponse);

                    // Also broadcast conversation update
                    var conversationUpdate = new
                    {
                        conversation.Id,
                        conversation.LastMessageTimestamp,
                        LastMessage = messageResponse,
                        conversation.UpdatedAt
                    };

                    await _hubContext.Clients.Group($"user_{receiverId}")
                        .SendAsync("ConversationUpdated", conversationUpdate);
                    
                    await _hubContext.Clients.Group($"user_{request.SenderId}")
                        .SendAsync("ConversationUpdated", conversationUpdate);

                    _logger.LogInformation("Message {MessageId} broadcasted via SignalR to sender {SenderId} and receiver {ReceiverId}", 
                        message.Id, request.SenderId, receiverId);
                }
                catch (Exception hubEx)
                {
                    _logger.LogError(hubEx, "Failed to broadcast message {MessageId} via SignalR", message.Id);
                    // Don't fail the request if SignalR fails
                }

                return Ok(messageResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message in conversation {ConversationId}", request.ConversationId);
                return StatusCode(500, "An error occurred while sending the message");
            }
        }

        /// <summary>
        /// Get messages for a conversation with pagination
        /// </summary>
        /// <param name="conversationId">The conversation ID</param>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 50)</param>
        /// <returns>Paginated list of messages</returns>
        [HttpGet("conversation/{conversationId}")]
        public async Task<IActionResult> GetConversationMessages(
            Guid conversationId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;

                // Check if conversation exists
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.Id == conversationId);

                if (conversation == null)
                {
                    return NotFound("Conversation not found");
                }

                var skip = (page - 1) * pageSize;

                var messages = await _context.Messages
                    .Where(m => m.ConversationId == conversationId && m.DeletedAt == null)
                    .OrderByDescending(m => m.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(m => new
                    {
                        m.Id,
                        m.ConversationId,
                        m.SenderId,
                        m.ReceiverId,
                        m.Content,
                        m.MessageType,
                        m.ReplyToMessageId,
                        m.IsRead,
                        m.CreatedAt,
                        m.UpdatedAt
                    })
                    .ToListAsync();

                var totalMessages = await _context.Messages
                    .Where(m => m.ConversationId == conversationId && m.DeletedAt == null)
                    .CountAsync();

                return Ok(new
                {
                    Messages = messages.OrderBy(m => m.CreatedAt).ToList(), // Return in chronological order
                    Pagination = new
                    {
                        Page = page,
                        PageSize = pageSize,
                        TotalItems = totalMessages,
                        TotalPages = (int)Math.Ceiling((double)totalMessages / pageSize),
                        HasNext = page * pageSize < totalMessages,
                        HasPrevious = page > 1
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting messages for conversation {ConversationId}", conversationId);
                return StatusCode(500, "An error occurred while retrieving messages");
            }
        }

        /// <summary>
        /// Mark a single message as read
        /// </summary>
        /// <param name="messageId">The message ID to mark as read</param>
        /// <param name="request">Mark single message as read request</param>
        /// <returns>Success response</returns>
        [HttpPut("{messageId}/mark-read")]
        public async Task<IActionResult> MarkSingleMessageAsRead(Guid messageId, [FromBody] MarkSingleMessageReadRequest request)
        {
            try
            {
                var message = await _context.Messages
                    .FirstOrDefaultAsync(m => m.Id == messageId && 
                                         m.ReceiverId == request.UserId && 
                                         m.DeletedAt == null);

                if (message == null)
                {
                    return NotFound("Message not found or user is not the receiver");
                }

                // Only update if not already read
                if (!message.IsRead)
                {
                    message.IsRead = true;
                    message.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                    await _context.SaveChangesAsync();

                    // Broadcast the read receipt via SignalR
                    try
                    {
                        var readReceipt = new
                        {
                            MessageId = message.Id.ToString(),
                            ConversationId = message.ConversationId.ToString(),
                            ReadByUserId = request.UserId.ToString(),
                            ReadAt = message.UpdatedAt
                        };

                        // Send read receipt to the sender
                        await _hubContext.Clients.Group($"user_{message.SenderId}")
                            .SendAsync("MessageRead", readReceipt);

                        // Also send to the reader (for multi-device support)
                        await _hubContext.Clients.Group($"user_{request.UserId}")
                            .SendAsync("MessageRead", readReceipt);

                        _logger.LogInformation("Message {MessageId} marked as read by user {UserId}, receipt sent to sender {SenderId}", 
                            messageId, request.UserId, message.SenderId);
                    }
                    catch (Exception hubEx)
                    {
                        _logger.LogError(hubEx, "Failed to broadcast read receipt for message {MessageId}", messageId);
                        // Don't fail the request if SignalR fails
                    }
                }

                return Ok(new { 
                    MessageId = messageId, 
                    IsRead = message.IsRead,
                    ReadAt = message.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message {MessageId} as read", messageId);
                return StatusCode(500, "An error occurred while marking the message as read");
            }
        }

        /// <summary>
        /// Mark messages as read
        /// </summary>
        /// <param name="request">Mark as read request</param>
        /// <returns>Success response</returns>
        [HttpPut("mark-read")]
        public async Task<IActionResult> MarkMessagesAsRead([FromBody] MarkMessagesReadRequest request)
        {
            try
            {
                var messages = await _context.Messages
                    .Where(m => m.ConversationId == request.ConversationId && 
                               m.ReceiverId == request.UserId && 
                               !m.IsRead && 
                               m.DeletedAt == null)
                    .ToListAsync();

                foreach (var message in messages)
                {
                    message.IsRead = true;
                    message.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                }

                await _context.SaveChangesAsync();

                return Ok(new { MarkedCount = messages.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read for conversation {ConversationId}", 
                    request.ConversationId);
                return StatusCode(500, "An error occurred while marking messages as read");
            }
        }

        /// <summary>
        /// Delete a message (soft delete)
        /// </summary>
        /// <param name="messageId">The message ID</param>
        /// <param name="userId">The user requesting deletion</param>
        /// <returns>Success response</returns>
        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage(Guid messageId, [FromQuery] Guid userId)
        {
            try
            {
                var message = await _context.Messages
                    .FirstOrDefaultAsync(m => m.Id == messageId && m.DeletedAt == null);

                if (message == null)
                {
                    return NotFound("Message not found");
                }

                // Only sender can delete message
                if (message.SenderId != userId)
                {
                    return StatusCode(403, "Only the sender can delete this message");
                }

                message.DeletedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                message.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

                await _context.SaveChangesAsync();

                // Broadcast the message deletion to both users via SignalR
                try
                {
                    var messageDeletedEvent = new
                    {
                        MessageId = messageId.ToString(),
                        ConversationId = message.ConversationId.ToString(),
                        DeletedBy = userId.ToString(),
                        DeletedAt = message.DeletedAt
                    };

                    // Send to receiver
                    await _hubContext.Clients.Group($"user_{message.ReceiverId}")
                        .SendAsync("MessageDeleted", messageDeletedEvent);

                    // Send to sender (for multi-device support)
                    await _hubContext.Clients.Group($"user_{message.SenderId}")
                        .SendAsync("MessageDeleted", messageDeletedEvent);

                    _logger.LogInformation("Message {MessageId} deletion broadcasted via SignalR to sender {SenderId} and receiver {ReceiverId}", 
                        messageId, message.SenderId, message.ReceiverId);
                }
                catch (Exception hubEx)
                {
                    _logger.LogError(hubEx, "Failed to broadcast message deletion {MessageId} via SignalR", messageId);
                    // Don't fail the request if SignalR fails
                }

                return Ok(new { Message = "Message deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message {MessageId}", messageId);
                return StatusCode(500, "An error occurred while deleting the message");
            }
        }
    }

    public class SendMessageRequest
    {
        public Guid ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public MessageType? MessageType { get; set; }
        public Guid? ReplyToMessageId { get; set; }
    }

    public class MarkMessagesReadRequest
    {
        public Guid ConversationId { get; set; }
        public Guid UserId { get; set; }
    }

    public class MarkSingleMessageReadRequest
    {
        public Guid UserId { get; set; }
    }
}