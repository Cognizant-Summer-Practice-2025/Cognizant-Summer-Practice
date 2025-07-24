using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendMessages.Data;
using BackendMessages.Models;

namespace BackendMessages.Controllers
{
    [ApiController]
    [Route("api/conversations")]
    public class ConversationsController : ControllerBase
    {
        private readonly MessagesDbContext _context;
        private readonly ILogger<ConversationsController> _logger;

        public ConversationsController(MessagesDbContext context, ILogger<ConversationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all conversations for a user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>List of conversations with last message info</returns>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserConversations(Guid userId)
        {
            try
            {
                var conversations = await _context.Conversations
                    .Where(c => c.InitiatorId == userId || c.ReceiverId == userId)
                    .Include(c => c.LastMessage)
                    .OrderByDescending(c => c.LastMessageTimestamp)
                    .Select(c => new
                    {
                        c.Id,
                        OtherUserId = c.InitiatorId == userId ? c.ReceiverId : c.InitiatorId,
                        IsInitiator = c.InitiatorId == userId,
                        c.LastMessageTimestamp,
                        LastMessage = c.LastMessage != null ? new
                        {
                            c.LastMessage.Id,
                            c.LastMessage.SenderId,
                            c.LastMessage.ReceiverId,
                            c.LastMessage.Content,
                            c.LastMessage.MessageType,
                            c.LastMessage.IsRead,
                            c.LastMessage.CreatedAt,
                            c.LastMessage.UpdatedAt
                        } : null,
                        UnreadCount = _context.Messages
                            .Where(m => m.ConversationId == c.Id && m.ReceiverId == userId && !m.IsRead && m.DeletedAt == null)
                            .Count(),
                        c.CreatedAt,
                        c.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversations for user {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving conversations");
            }
        }

        /// <summary>
        /// Create or get existing conversation between two users
        /// </summary>
        /// <param name="initiatorId">The ID of the user initiating the conversation</param>
        /// <param name="receiverId">The ID of the other user</param>
        /// <returns>Conversation details</returns>
        [HttpPost("create")]
        public async Task<IActionResult> CreateOrGetConversation([FromBody] CreateConversationRequest request)
        {
            try
            {
                if (request.InitiatorId == request.ReceiverId)
                {
                    return BadRequest("Cannot create conversation with yourself");
                }

                // Check if conversation already exists
                var existingConversation = await _context.Conversations
                    .Where(c => (c.InitiatorId == request.InitiatorId && c.ReceiverId == request.ReceiverId) ||
                               (c.InitiatorId == request.ReceiverId && c.ReceiverId == request.InitiatorId))
                    .Include(c => c.LastMessage)
                    .FirstOrDefaultAsync();

                if (existingConversation != null)
                {
                    return Ok(new
                    {
                        existingConversation.Id,
                        OtherUserId = existingConversation.InitiatorId == request.InitiatorId 
                            ? existingConversation.ReceiverId 
                            : existingConversation.InitiatorId,
                        IsInitiator = existingConversation.InitiatorId == request.InitiatorId,
                        existingConversation.LastMessageTimestamp,
                        LastMessage = existingConversation.LastMessage != null ? new
                        {
                            existingConversation.LastMessage.Id,
                            existingConversation.LastMessage.SenderId,
                            existingConversation.LastMessage.ReceiverId,
                            existingConversation.LastMessage.Content,
                            existingConversation.LastMessage.MessageType,
                            existingConversation.LastMessage.IsRead,
                            existingConversation.LastMessage.CreatedAt,
                            existingConversation.LastMessage.UpdatedAt
                        } : null,
                        UnreadCount = await _context.Messages
                            .Where(m => m.ConversationId == existingConversation.Id && 
                                       m.ReceiverId == request.InitiatorId && 
                                       !m.IsRead && 
                                       m.DeletedAt == null)
                            .CountAsync(),
                        existingConversation.CreatedAt,
                        existingConversation.UpdatedAt,
                        IsExisting = true
                    });
                }

                // Create new conversation
                var newConversation = new Conversation
                {
                    InitiatorId = request.InitiatorId,
                    ReceiverId = request.ReceiverId,
                    LastMessageTimestamp = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Conversations.Add(newConversation);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    newConversation.Id,
                    OtherUserId = request.ReceiverId,
                    IsInitiator = true,
                    newConversation.LastMessageTimestamp,
                    LastMessage = (object?)null,
                    UnreadCount = 0,
                    newConversation.CreatedAt,
                    newConversation.UpdatedAt,
                    IsExisting = false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation between {InitiatorId} and {ReceiverId}", 
                    request.InitiatorId, request.ReceiverId);
                return StatusCode(500, "An error occurred while creating the conversation");
            }
        }

        /// <summary>
        /// Get conversation by ID
        /// </summary>
        /// <param name="conversationId">The conversation ID</param>
        /// <returns>Conversation details</returns>
        [HttpGet("{conversationId}")]
        public async Task<IActionResult> GetConversation(Guid conversationId)
        {
            try
            {
                var conversation = await _context.Conversations
                    .Include(c => c.LastMessage)
                    .FirstOrDefaultAsync(c => c.Id == conversationId);

                if (conversation == null)
                {
                    return NotFound("Conversation not found");
                }

                return Ok(new
                {
                    conversation.Id,
                    conversation.InitiatorId,
                    conversation.ReceiverId,
                    conversation.LastMessageTimestamp,
                    LastMessage = conversation.LastMessage != null ? new
                    {
                        conversation.LastMessage.Id,
                        conversation.LastMessage.SenderId,
                        conversation.LastMessage.ReceiverId,
                        conversation.LastMessage.Content,
                        conversation.LastMessage.MessageType,
                        conversation.LastMessage.IsRead,
                        conversation.LastMessage.CreatedAt,
                        conversation.LastMessage.UpdatedAt
                    } : null,
                    conversation.CreatedAt,
                    conversation.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation {ConversationId}", conversationId);
                return StatusCode(500, "An error occurred while retrieving the conversation");
            }
        }
    }

    public class CreateConversationRequest
    {
        public Guid InitiatorId { get; set; }
        public Guid ReceiverId { get; set; }
    }
} 