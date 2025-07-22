using Microsoft.AspNetCore.Mvc;
using backend_messages.DTOs.Requests;
using backend_messages.DTOs.Responses;
using backend_messages.Services;

namespace backend_messages.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationService _conversationService;

        public ConversationController(IConversationService conversationService)
        {
            _conversationService = conversationService;
        }

        [HttpPost]
        public async Task<ActionResult<ConversationResponse>> CreateConversation([FromBody] CreateConversationRequest request)
        {
            var conversation = await _conversationService.CreateConversationAsync(request);
            if (conversation == null)
            {
                return BadRequest("Unable to create conversation.");
            }
            return CreatedAtAction(nameof(GetConversationHistory), new { id = conversation.Id }, conversation);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<List<MessageResponse>>> GetConversationHistory(Guid id)
        {
            var messages = await _conversationService.GetConversationHistoryAsync(id);
            if (messages == null)
            {
                return NotFound("Conversation not found.");
            }
            return Ok(messages);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<ConversationResponse>>> GetUserConversations(Guid userId)
        {
            var conversations = await _conversationService.GetUserConversationsAsync(userId);
            return Ok(conversations);
        }
    }
}