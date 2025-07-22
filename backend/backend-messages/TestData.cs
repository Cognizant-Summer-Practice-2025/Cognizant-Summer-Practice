using Microsoft.EntityFrameworkCore;
using backend_messages.Data;
using backend_messages.Models;

namespace backend_messages
{
    public static class TestData
    {
        public static async Task SeedTestDataAsync(MessageDbContext context)
        {
            if (await context.Conversations.AnyAsync())
            {
                return; 
            }

            var conv1 = new Conversation
            {
                Id = Guid.NewGuid(),
                User1Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                User2Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), 
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-2)
            };

            var conv2 = new Conversation
            {
                Id = Guid.NewGuid(),
                User1Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), 
                User2Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), 
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddHours(-1)
            };

            await context.Conversations.AddRangeAsync(conv1, conv2);

            var messages1 = new[]
            {
                new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conv1.Id,
                    SenderId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Content = "Hi! I saw your portfolio and I'm really impressed with your projects!",
                    MessageType = "Text",
                    CreatedAt = DateTime.UtcNow.AddHours(-4),
                    UpdatedAt = DateTime.UtcNow.AddHours(-4)
                },
                new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conv1.Id,
                    SenderId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Content = "Thank you! I really appreciate the feedback.",
                    MessageType = "Text",
                    CreatedAt = DateTime.UtcNow.AddHours(-3),
                    UpdatedAt = DateTime.UtcNow.AddHours(-3)
                },
                new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conv1.Id,
                    SenderId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Content = "Would you be interested in collaborating on a project?",
                    MessageType = "Text",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-2),
                    UpdatedAt = DateTime.UtcNow.AddMinutes(-2)
                }
            };

            var messages2 = new[]
            {
                new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conv2.Id,
                    SenderId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Content = "Hey! Just saw your latest ML project on GitHub. Really clean implementation!",
                    MessageType = "Text",
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    UpdatedAt = DateTime.UtcNow.AddHours(-2)
                },
                new Message
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conv2.Id,
                    SenderId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Content = "Thanks Mike! I spent a lot of time optimizing the training pipeline.",
                    MessageType = "Text",
                    CreatedAt = DateTime.UtcNow.AddHours(-1),
                    UpdatedAt = DateTime.UtcNow.AddHours(-1)
                }
            };

            await context.Messages.AddRangeAsync(messages1);
            await context.Messages.AddRangeAsync(messages2);

            await context.SaveChangesAsync();
        }
    }
} 