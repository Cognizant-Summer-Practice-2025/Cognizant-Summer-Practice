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
        }

        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
}