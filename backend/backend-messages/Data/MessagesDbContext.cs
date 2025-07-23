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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("messages");
                
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                
                entity.Property(e => e.SenderId).HasColumnName("sender_id");
                entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
                entity.Property(e => e.ReplyToMessageId).HasColumnName("reply_to_message_id");
                entity.Property(e => e.Content).HasColumnName("content");
                entity.Property(e => e.MessageType).HasColumnName("message_type");
                entity.Property(e => e.IsRead).HasColumnName("is_read");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
                entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            });
        }
    }
}