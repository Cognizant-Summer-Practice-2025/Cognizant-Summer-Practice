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
            modelBuilder.Entity<Conversation>()
                .ToTable("conversations");

            modelBuilder.Entity<Message>()
                .ToTable("messages");

            modelBuilder.Entity<Notification>()
                .ToTable("notifications");
        }

        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
}