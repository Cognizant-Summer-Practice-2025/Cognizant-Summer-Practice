using Microsoft.EntityFrameworkCore;
using BackendMessages.Models;

namespace BackendMessages.Data
{
    public class MessagesDbContext : DbContext
    {
        public MessagesDbContext(DbContextOptions<MessagesDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<MessageReport> MessageReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure all DateTime properties to use timestamp without time zone to match existing database
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        property.SetColumnType("timestamp without time zone");
                    }
                }
            }

            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("messages");
                
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                
                entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
                entity.Property(e => e.SenderId).HasColumnName("sender_id");
                entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
                entity.Property(e => e.ReplyToMessageId).HasColumnName("reply_to_message_id");
                entity.Property(e => e.Content).HasColumnName("content");
                entity.Property(e => e.MessageType).HasColumnName("message_type");
                entity.Property(e => e.IsRead).HasColumnName("is_read");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
                entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
                
                // Foreign key relationships
                entity.HasOne<Conversation>()
                    .WithMany(c => c.Messages)
                    .HasForeignKey(m => m.ConversationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.ToTable("conversations");
                
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                
                entity.Property(e => e.InitiatorId).HasColumnName("initiator_id");
                entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
                entity.Property(e => e.LastMessageTimestamp).HasColumnName("last_message_timestamp");
                entity.Property(e => e.LastMessageId).HasColumnName("last_message_id");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
                entity.Property(e => e.InitiatorDeletedAt).HasColumnName("initiator_deleted_at");
                entity.Property(e => e.ReceiverDeletedAt).HasColumnName("receiver_deleted_at");
                
                // Foreign key relationship for last message
                entity.HasOne(c => c.LastMessage)
                    .WithMany()
                    .HasForeignKey(c => c.LastMessageId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<MessageReport>(entity =>
            {
                entity.ToTable("message_reports");
                
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                
                entity.Property(e => e.MessageId).HasColumnName("message_id");
                entity.Property(e => e.ReportedByUserId).HasColumnName("reported_by_user_id");
                entity.Property(e => e.Reason).HasColumnName("reason");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                
                // Foreign key relationship
                entity.HasOne(mr => mr.Message)
                    .WithMany()
                    .HasForeignKey(mr => mr.MessageId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                // Unique constraint to prevent duplicate reports
                entity.HasIndex(mr => new { mr.MessageId, mr.ReportedByUserId })
                    .IsUnique()
                    .HasDatabaseName("uk_message_reports_user_message");
            });
        }
    }
}