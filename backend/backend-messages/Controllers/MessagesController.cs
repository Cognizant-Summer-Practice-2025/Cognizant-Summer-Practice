using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using BackendMessages.Data;
using BackendMessages.Models;
using BackendMessages.Hubs;
using BackendMessages.Repositories;
using BackendMessages.DTO.Message.Response;
using BackendMessages.Services.Abstractions;
using System.Security.Claims;

namespace BackendMessages.Controllers
{
    [ApiController]
    [Route("api/messages")]
    public class MessagesController : ControllerBase
    {
        private readonly MessagesDbContext _context;
        private readonly ILogger<MessagesController> _logger;
        private readonly IHubContext<MessageHub> _hubContext;
        private readonly IMessageReportRepository _messageReportRepository;
        private readonly IEmailService _emailService;
        private readonly IUserSearchService _userSearchService;
        private readonly IConfiguration _configuration;

        public MessagesController(
            MessagesDbContext context, 
            ILogger<MessagesController> logger, 
            IHubContext<MessageHub> hubContext, 
            IMessageReportRepository messageReportRepository,
            IEmailService emailService,
            IUserSearchService userSearchService,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _hubContext = hubContext;
            _messageReportRepository = messageReportRepository;
            _emailService = emailService;
            _userSearchService = userSearchService;
            _configuration = configuration;
        }

        /// <summary>
        /// Gets the authenticated user ID from the current context.
        /// </summary>
        /// <returns>The authenticated user ID, or null if not authenticated.</returns>
        private Guid? GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
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
                // Validate authenticated user matches sender
                var authenticatedUserId = GetAuthenticatedUserId();
                if (authenticatedUserId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                if (authenticatedUserId != request.SenderId)
                {
                    return Forbid("You can only send messages as yourself");
                }

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

                // Send email notification to receiver (fire and forget)
                var enableRealTimeNotifications = bool.Parse(_configuration["Email:EnableRealTimeNotifications"] ?? "true");
                if (enableRealTimeNotifications)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var recipient = await _userSearchService.GetUserByIdAsync(receiverId);
                            var sender = await _userSearchService.GetUserByIdAsync(request.SenderId);
                            
                            if (recipient != null && sender != null && !string.IsNullOrEmpty(recipient.Email))
                            {
                                await _emailService.SendMessageReceivedNotificationAsync(message, recipient, sender);
                                _logger.LogInformation("Email notification sent for message {MessageId} to {RecipientEmail}", 
                                    message.Id, recipient.Email);
                            }
                            else
                            {
                                _logger.LogWarning("Could not send email notification for message {MessageId}. Recipient: {RecipientFound}, Sender: {SenderFound}, Email: {HasEmail}", 
                                    message.Id, recipient != null, sender != null, recipient?.Email != null);
                            }
                        }
                        catch (Exception emailEx)
                        {
                            _logger.LogError(emailEx, "Failed to send email notification for message {MessageId}", message.Id);
                            // Don't fail the request if email fails
                        }
                    });
                }
                else
                {
                    _logger.LogDebug("Real-time email notifications are disabled for message {MessageId}", message.Id);
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
                // Validate authenticated user
                var authenticatedUserId = GetAuthenticatedUserId();
                if (authenticatedUserId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 50;

                // Check if conversation exists
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.Id == conversationId);

                if (conversation == null)
                {
                    return NotFound("Conversation not found");
                }

                // Verify user is part of the conversation
                if (conversation.InitiatorId != authenticatedUserId && conversation.ReceiverId != authenticatedUserId)
                {
                    return Forbid("You are not part of this conversation");
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
                            messageId = message.Id.ToString(),
                            conversationId = message.ConversationId.ToString(),
                            readByUserId = request.UserId.ToString(),
                            readAt = message.UpdatedAt
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

                foreach (var message in messages)
                {
                    try
                    {
                        var readReceipt = new
                        {
                            messageId = message.Id.ToString(),
                            conversationId = message.ConversationId.ToString(),
                            readByUserId = request.UserId.ToString(),
                            readAt = message.UpdatedAt
                        };

                        await _hubContext.Clients.Group($"user_{message.SenderId}")
                            .SendAsync("MessageRead", readReceipt);

                        await _hubContext.Clients.Group($"user_{request.UserId}")
                            .SendAsync("MessageRead", readReceipt);

                        _logger.LogInformation("Message {MessageId} marked as read by user {UserId}, receipt sent to sender {SenderId}", 
                            message.Id, request.UserId, message.SenderId);
                    }
                    catch (Exception hubEx)
                    {
                        _logger.LogError(hubEx, "Failed to broadcast read receipt for message {MessageId}", message.Id);
                    }
                }

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

        /// <summary>
        /// Report a message
        /// </summary>
        /// <param name="messageId">The message ID to report</param>
        /// <param name="request">Report request details</param>
        /// <returns>Success response</returns>
        [HttpPost("{messageId}/report")]
        public async Task<IActionResult> ReportMessage(Guid messageId, [FromBody] ReportMessageRequest request)
        {
            try
            {
                // Verify the message exists and user has access
                var message = await _context.Messages
                    .FirstOrDefaultAsync(m => m.Id == messageId && m.DeletedAt == null);

                if (message == null)
                {
                    return NotFound("Message not found");
                }

                // Verify user is part of the conversation (can see the message)
                var conversation = await _context.Conversations
                    .FirstOrDefaultAsync(c => c.Id == message.ConversationId &&
                                              (c.InitiatorId == request.ReportedByUserId || c.ReceiverId == request.ReportedByUserId));

                if (conversation == null)
                {
                    return StatusCode(403, "You don't have access to this message");
                }

                // Check if user has already reported this message
                var existingReport = await _context.MessageReports
                    .FirstOrDefaultAsync(mr => mr.MessageId == messageId && mr.ReportedByUserId == request.ReportedByUserId);

                if (existingReport != null)
                {
                    return BadRequest("You have already reported this message");
                }

                // Create the report
                var report = new BackendMessages.Models.MessageReport
                {
                    MessageId = messageId,
                    ReportedByUserId = request.ReportedByUserId,
                    Reason = request.Reason,
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                };

                _context.MessageReports.Add(report);
                await _context.SaveChangesAsync();

                _logger.LogWarning("Message {MessageId} reported by user {UserId} for reason: {Reason}", 
                    messageId, request.ReportedByUserId, request.Reason);

                return Ok(new { 
                    Message = "Message reported successfully",
                    ReportId = report.Id,
                    ReportedAt = report.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting message {MessageId}", messageId);
                return StatusCode(500, "An error occurred while reporting the message");
            }
        }

        /// <summary>
        /// Get all message reports (Admin only)
        /// </summary>
        /// <returns>List of all message reports with message details</returns>
        [HttpGet("admin/reports")]
        public async Task<IActionResult> GetAllMessageReports()
        {
            try
            {
                var reports = await _messageReportRepository.GetAllMessageReportsAsync();
                var reportDtos = reports.Select(r => new MessageReportResponseDto
                {
                    Id = r.Id,
                    MessageId = r.MessageId,
                    ReportedByUserId = r.ReportedByUserId,
                    Reason = r.Reason,
                    CreatedAt = r.CreatedAt,
                    Message = r.Message != null ? new MessageReportDetailsDto
                    {
                        Id = r.Message.Id,
                        ConversationId = r.Message.ConversationId,
                        SenderId = r.Message.SenderId,
                        ReceiverId = r.Message.ReceiverId,
                        Content = r.Message.Content ?? string.Empty,
                        MessageType = r.Message.MessageType,
                        CreatedAt = r.Message.CreatedAt
                    } : null
                }).ToList();
                
                return Ok(reportDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all message reports");
                return StatusCode(500, "An error occurred while retrieving message reports");
            }
        }

        /// <summary>
        /// Delete all message data for a user (Admin only - called during user deletion)
        /// </summary>
        /// <param name="userId">The user ID whose message data should be deleted</param>
        /// <returns>Success response</returns>
        [HttpDelete("admin/user/{userId}")]
        public async Task<IActionResult> DeleteUserMessageData(Guid userId)
        {
            try
            {
                _logger.LogInformation("Starting cascade deletion of all message data for user {UserId}", userId);

                // Delete all conversations where user is initiator or receiver
                var conversations = await _context.Conversations
                    .Where(c => c.InitiatorId == userId || c.ReceiverId == userId)
                    .ToListAsync();

                var allMessageReports = new List<MessageReport>();

                foreach (var conversation in conversations)
                {
                    // Delete all messages in the conversation
                    var messages = await _context.Messages
                        .Where(m => m.ConversationId == conversation.Id)
                        .ToListAsync();

                    // Delete message reports for these messages
                    var messageIds = messages.Select(m => m.Id).ToList();
                    var messageReports = await _context.MessageReports
                        .Where(mr => messageIds.Contains(mr.MessageId))
                        .ToListAsync();

                    allMessageReports.AddRange(messageReports);
                    _context.MessageReports.RemoveRange(messageReports);
                    _context.Messages.RemoveRange(messages);
                }

                // Delete conversations
                _context.Conversations.RemoveRange(conversations);

                // Delete message reports created by this user (for messages they reported)
                var userReports = await _context.MessageReports
                    .Where(mr => mr.ReportedByUserId == userId)
                    .ToListAsync();

                _context.MessageReports.RemoveRange(userReports);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted all message data for user {UserId}. " +
                    "Deleted {ConversationCount} conversations, {MessageCount} messages, {ReportCount} reports",
                    userId, conversations.Count, conversations.Sum(c => c.Messages.Count), 
                    allMessageReports.Count + userReports.Count);

                return Ok(new { 
                    message = "User message data deleted successfully",
                    deletedConversations = conversations.Count,
                    deletedReports = allMessageReports.Count + userReports.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting message data for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while deleting user message data" });
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

    public class ReportMessageRequest
    {
        public Guid ReportedByUserId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}