using Microsoft.EntityFrameworkCore;
using backend_user.Models;

namespace backend_user.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configurations
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.Username).IsRequired();
                entity.Property(e => e.FirstName).IsRequired();
                entity.Property(e => e.LastName).IsRequired();
            });

            // OAuthProvider configurations
            modelBuilder.Entity<OAuthProvider>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.OAuthProviders)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // UserSettings configurations
            modelBuilder.Entity<UserSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithOne(u => u.UserSettings)
                      .HasForeignKey<UserSettings>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Newsletter configurations
            modelBuilder.Entity<Newsletter>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Newsletters)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // UserAnalytics configurations
            modelBuilder.Entity<UserAnalytics>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.UserAnalytics)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // AdminAction configurations
            modelBuilder.Entity<AdminAction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Admin)
                      .WithMany(u => u.AdminActions)
                      .HasForeignKey(e => e.AdminId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // UserReport configurations
            modelBuilder.Entity<UserReport>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Reporter)
                      .WithMany(u => u.ReportsCreated)
                      .HasForeignKey(e => e.ReporterId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Resolver)
                      .WithMany(u => u.ReportsResolved)
                      .HasForeignKey(e => e.ResolvedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // DbSet properties for all models
        public DbSet<User> Users { get; set; }
        public DbSet<OAuthProvider> OAuthProviders { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<Newsletter> Newsletters { get; set; }
        public DbSet<UserAnalytics> UserAnalytics { get; set; }
        public DbSet<AdminAction> AdminActions { get; set; }
        public DbSet<UserReport> UserReports { get; set; }
    }
}