using Microsoft.EntityFrameworkCore;
using backend_messages.Models;

namespace backend_messages.Data
{
    public class MessageDbContext : DbContext
    {
        public MessageDbContext(DbContextOptions<MessageDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Conversation configurations
            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.LastMessage)
                      .WithMany()
                      .HasForeignKey(e => e.LastMessageId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ConversationParticipant configurations
            modelBuilder.Entity<ConversationParticipant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Conversation)
                      .WithMany(c => c.Participants)
                      .HasForeignKey(e => e.ConversationId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.LastReadMessage)
                      .WithMany()
                      .HasForeignKey(e => e.LastReadMessageId)
                      .OnDelete(DeleteBehavior.SetNull);
                // Create unique index on ConversationId and UserId
                entity.HasIndex(e => new { e.ConversationId, e.UserId }).IsUnique();
            });

            // Message configurations
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Conversation)
                      .WithMany(c => c.Messages)
                      .HasForeignKey(e => e.ConversationId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.ReplyToMessage)
                      .WithMany(m => m.Replies)
                      .HasForeignKey(e => e.ReplyToMessageId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.Property(e => e.Content).IsRequired();
            });

            // MessageRead configurations
            modelBuilder.Entity<MessageRead>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Message)
                      .WithMany(m => m.MessageReads)
                      .HasForeignKey(e => e.MessageId)
                      .OnDelete(DeleteBehavior.Cascade);
                // Create unique index on MessageId and UserId
                entity.HasIndex(e => new { e.MessageId, e.UserId }).IsUnique();
            });

            // UserCache configurations
            modelBuilder.Entity<UserCache>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.FirstName).IsRequired();
                entity.Property(e => e.LastName).IsRequired();
            });

            // Notification configurations
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Content).IsRequired();
                entity.HasOne(e => e.RelatedConversation)
                      .WithMany()
                      .HasForeignKey(e => e.RelatedConversationId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.RelatedMessage)
                      .WithMany()
                      .HasForeignKey(e => e.RelatedMessageId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }

        // DbSet properties for all models
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageRead> MessageReads { get; set; }
        public DbSet<UserCache> UserCache { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
}