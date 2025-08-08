using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendMessages.Data;
using BackendMessages.Models;
using BackendMessages.Services;
using BackendMessages.Services.Abstractions;
using BackendMessages.DTO.Conversation.Request;
using System.Security.Claims;

namespace BackendMessages.Controllers
{
    [ApiController]
    [Route("api/conversations")]
    public class ConversationsController : ControllerBase
    {
        private readonly MessagesDbContext _context;
        private readonly ILogger<ConversationsController> _logger;
        private readonly IConversationService _conversationService;

        public ConversationsController(MessagesDbContext context, ILogger<ConversationsController> logger, IConversationService conversationService)
        {
            _context = context;
            _logger = logger;
            _conversationService = conversationService;
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
        /// Get all conversations for a user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>List of conversations with last message info</returns>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserConversations(Guid userId)
        {
            try
            {
                // Validate authenticated user matches the requested userId
                var authenticatedUserId = GetAuthenticatedUserId();
                if (authenticatedUserId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                if (authenticatedUserId != userId)
                {
                    return Forbid("You can only access your own conversations");
                }
                var conversations = await _context.Conversations
                    .Where(c => (c.InitiatorId == userId || c.ReceiverId == userId) &&
                               // Filter out conversations deleted by this user
                               ((c.InitiatorId == userId && !c.InitiatorDeletedAt.HasValue) ||
                                (c.ReceiverId == userId && !c.ReceiverDeletedAt.HasValue)))
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
                // Validate authenticated user matches the initiator
                var authenticatedUserId = GetAuthenticatedUserId();
                if (authenticatedUserId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                if (authenticatedUserId != request.InitiatorId)
                {
                    return Forbid("You can only create conversations as yourself");
                }

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
                    LastMessageTimestamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
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
                // Validate authenticated user
                var authenticatedUserId = GetAuthenticatedUserId();
                if (authenticatedUserId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                var conversation = await _context.Conversations
                    .Include(c => c.LastMessage)
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

        /// <summary>
        /// Delete a conversation and all its messages
        /// </summary>
        /// <param name="conversationId">The conversation ID to delete</param>
        /// <param name="userId">The ID of the user requesting the deletion</param>
        /// <returns>Success or error response</returns>
        [HttpDelete("{conversationId}")]
        public async Task<IActionResult> DeleteConversation(Guid conversationId, [FromQuery] Guid userId)
        {
            _logger.LogInformation("DELETE request received for conversation {ConversationId} by user {UserId}", 
                conversationId, userId);

            try
            {
                // Validate authenticated user matches the userId parameter
                var authenticatedUserId = GetAuthenticatedUserId();
                if (authenticatedUserId == null)
                {
                    return Unauthorized("User not authenticated");
                }

                if (authenticatedUserId != userId)
                {
                    return Forbid("You can only delete conversations as yourself");
                }

                if (userId == Guid.Empty)
                {
                    _logger.LogWarning("Delete conversation failed: User ID is empty");
                    return BadRequest("User ID is required");
                }

                _logger.LogInformation("Calling DeleteConversationAsync service method...");
                var deleteRequest = new DeleteConversationRequest
                {
                    ConversationId = conversationId,
                    UserId = userId
                };
                var result = await _conversationService.DeleteConversationAsync(deleteRequest);
                var success = result.Success;
                _logger.LogInformation("DeleteConversationAsync returned: {Success}", success);

                if (!success)
                {
                    _logger.LogWarning("Delete conversation service returned false, checking reasons...");
                    
                    // Check if conversation exists
                    _logger.LogInformation("Checking if conversation exists...");
                    var conversationExists = await _context.Conversations
                        .AnyAsync(c => c.Id == conversationId);
                    _logger.LogInformation("Conversation exists: {Exists}", conversationExists);

                    if (!conversationExists)
                    {
                        _logger.LogWarning("Conversation {ConversationId} not found", conversationId);
                        return NotFound("Conversation not found");
                    }

                    // Check if user has permission
                    _logger.LogInformation("Checking user permissions...");
                    var hasPermission = await _conversationService.UserCanAccessConversationAsync(conversationId, userId);
                    _logger.LogInformation("User has permission: {HasPermission}", hasPermission);
                    
                    if (!hasPermission)
                    {
                        _logger.LogWarning("User {UserId} does not have permission to delete conversation {ConversationId}", 
                            userId, conversationId);
                        return Forbid("You don't have permission to delete this conversation");
                    }

                    _logger.LogError("Delete conversation failed for unknown reason. Conversation exists: {Exists}, User has permission: {HasPermission}", 
                        conversationExists, hasPermission);
                    return StatusCode(500, "An error occurred while deleting the conversation");
                }

                _logger.LogInformation("Conversation {ConversationId} deleted successfully by user {UserId}", 
                    conversationId, userId);
                return Ok(new { message = "Conversation removed from your chat list" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while deleting conversation {ConversationId} for user {UserId}. " +
                    "Exception type: {ExceptionType}, Message: {ExceptionMessage}, StackTrace: {StackTrace}", 
                    conversationId, userId, ex.GetType().Name, ex.Message, ex.StackTrace);
                return StatusCode(500, $"An error occurred while deleting the conversation: {ex.Message}");
            }
        }
    }
} 