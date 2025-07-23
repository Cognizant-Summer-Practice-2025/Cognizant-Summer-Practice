using Microsoft.AspNetCore.Mvc;
using backend_messages.DTOs.Requests;
using backend_messages.DTOs.Responses;
using backend_messages.Services;

namespace backend_messages.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            var message = await _messageService.SendMessageAsync(request);
            if (message == null)
            {
                return BadRequest("Failed to send message.");
            }
            return Ok(new MessageResponse
            {
                Id = message.Id,
                SenderId = message.SenderId,
                ReceiverId = message.ReceiverId,
                Content = message.Content,
                CreatedAt = message.CreatedAt
            });
        }

        [HttpGet("{conversationId}")]
        public async Task<IActionResult> GetMessages(Guid conversationId)
        {
            var messages = await _messageService.GetMessagesByConversationIdAsync(conversationId);
            return Ok(messages.Select(m => new MessageResponse
            {
                Id = m.Id,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                Content = m.Content,
                CreatedAt = m.CreatedAt
            }));
        }
    }
}