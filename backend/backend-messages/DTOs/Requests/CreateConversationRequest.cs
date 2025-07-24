using System;

namespace backend_messages.DTOs.Requests
{
    public class CreateConversationRequest
    {
        public Guid User1Id { get; set; }
        public Guid User2Id { get; set; }
    }
}