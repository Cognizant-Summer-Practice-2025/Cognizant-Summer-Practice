using System;
using AutoFixture;
using BackendMessages.Models;
using System.Collections.Generic;
using System.Linq;

namespace BackendMessages.Tests.Helpers
{
    public static class TestDataFactory
    {
        private static readonly Fixture _fixture = new();

        static TestDataFactory()
        {
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        public static Message CreateMessage(
            Guid? conversationId = null,
            Guid? senderId = null,
            Guid? receiverId = null,
            string? content = null,
            MessageType messageType = MessageType.Text,
            bool isRead = false,
            DateTime? createdAt = null,
            DateTime? updatedAt = null)
        {
            return new Message
            {
                Id = Guid.NewGuid(),
                ConversationId = conversationId ?? Guid.NewGuid(),
                SenderId = senderId ?? Guid.NewGuid(),
                ReceiverId = receiverId ?? Guid.NewGuid(),
                Content = content ?? _fixture.Create<string>(),
                MessageType = messageType,
                IsRead = isRead,
                CreatedAt = createdAt ?? DateTime.UtcNow.AddDays(-1),
                UpdatedAt = updatedAt ?? DateTime.UtcNow
            };
        }

        public static Conversation CreateConversation(
            Guid? initiatorId = null,
            Guid? receiverId = null,
            Guid? lastMessageId = null,
            DateTime? lastMessageTimestamp = null,
            DateTime? createdAt = null,
            DateTime? updatedAt = null)
        {
            return new Conversation
            {
                Id = Guid.NewGuid(),
                InitiatorId = initiatorId ?? Guid.NewGuid(),
                ReceiverId = receiverId ?? Guid.NewGuid(),
                LastMessageId = lastMessageId,
                LastMessageTimestamp = lastMessageTimestamp ?? DateTime.UtcNow.AddDays(-1),
                CreatedAt = createdAt ?? DateTime.UtcNow.AddDays(-5),
                UpdatedAt = updatedAt ?? DateTime.UtcNow.AddDays(-1)
            };
        }

        public static MessageReport CreateMessageReport(
            Guid? messageId = null,
            Guid? reportedByUserId = null,
            string? reason = null,
            DateTime? createdAt = null)
        {
            return new MessageReport
            {
                Id = Guid.NewGuid(),
                MessageId = messageId ?? Guid.NewGuid(),
                ReportedByUserId = reportedByUserId ?? Guid.NewGuid(),
                Reason = reason ?? _fixture.Create<string>(),
                CreatedAt = createdAt ?? DateTime.UtcNow
            };
        }

        public static SearchUser CreateSearchUser(
            Guid? id = null,
            string? username = null,
            string? firstName = null,
            string? lastName = null,
            string? fullName = null,
            string? professionalTitle = null,
            string? avatarUrl = null,
            bool? isActive = null)
        {
            return new SearchUser
            {
                Id = id ?? Guid.NewGuid(),
                Username = username ?? _fixture.Create<string>(),
                FirstName = firstName,
                LastName = lastName,
                FullName = fullName ?? _fixture.Create<string>(),
                ProfessionalTitle = professionalTitle,
                AvatarUrl = avatarUrl,
                IsActive = isActive ?? _fixture.Create<bool>()
            };
        }

        public static List<Message> CreateMessages(int count, Guid conversationId)
        {
            var messages = new List<Message>();
            for (int i = 0; i < count; i++)
            {
                messages.Add(CreateMessage(conversationId: conversationId));
            }
            return messages;
        }

        public static List<Conversation> CreateConversations(int count)
        {
            var conversations = new List<Conversation>();
            for (int i = 0; i < count; i++)
            {
                conversations.Add(CreateConversation());
            }
            return conversations;
        }

        public static List<MessageReport> CreateMessageReports(int count)
        {
            var reports = new List<MessageReport>();
            for (int i = 0; i < count; i++)
            {
                reports.Add(CreateMessageReport());
            }
            return reports;
        }

        public static List<SearchUser> CreateSearchUsers(int count)
        {
            var users = new List<SearchUser>();
            for (int i = 0; i < count; i++)
            {
                users.Add(CreateSearchUser());
            }
            return users;
        }

        public static Message CreateTextMessage(string content = "Hello, world!")
        {
            return CreateMessage(content: content, messageType: MessageType.Text);
        }

        public static Message CreateImageMessage(string content = "image.jpg")
        {
            return CreateMessage(content: content, messageType: MessageType.Image);
        }

        public static Message CreateFileMessage(string content = "document.pdf")
        {
            return CreateMessage(content: content, messageType: MessageType.File);
        }

        public static Message CreateAudioMessage(string content = "audio.mp3")
        {
            return CreateMessage(content: content, messageType: MessageType.Audio);
        }

        public static Message CreateVideoMessage(string content = "video.mp4")
        {
            return CreateMessage(content: content, messageType: MessageType.Video);
        }

        public static Message CreateSystemMessage(string content = "System notification")
        {
            return CreateMessage(content: content, messageType: MessageType.System);
        }

        public static Conversation CreateActiveConversation()
        {
            return CreateConversation();
        }

        public static Conversation CreateDeletedConversation(Guid userId)
        {
            var conversation = CreateConversation(initiatorId: userId);
            conversation.InitiatorDeletedAt = DateTime.UtcNow;
            return conversation;
        }

        public static Conversation CreateFullyDeletedConversation()
        {
            var conversation = CreateConversation();
            conversation.InitiatorDeletedAt = DateTime.UtcNow.AddDays(-2);
            conversation.ReceiverDeletedAt = DateTime.UtcNow.AddDays(-1);
            return conversation;
        }

        public static SearchUser CreateActiveUser()
        {
            return CreateSearchUser(isActive: true);
        }

        public static SearchUser CreateInactiveUser()
        {
            return CreateSearchUser(isActive: false);
        }

        public static SearchUser CreateUserWithFullName(string firstName, string lastName)
        {
            return CreateSearchUser(
                firstName: firstName,
                lastName: lastName,
                fullName: $"{firstName} {lastName}"
            );
        }

        public static SearchUser CreateUserWithProfessionalTitle(string title)
        {
            return CreateSearchUser(professionalTitle: title);
        }

        public static SearchUser CreateUserWithAvatar(string avatarUrl)
        {
            return CreateSearchUser(avatarUrl: avatarUrl);
        }
    }
}
