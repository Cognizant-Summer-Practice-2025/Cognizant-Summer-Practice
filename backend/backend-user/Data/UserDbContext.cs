using Microsoft.EntityFrameworkCore;
using backend_user.Models;
using System;

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

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.Username).IsUnique();
            });

            // Configure OAuthProvider entity
            modelBuilder.Entity<OAuthProvider>(entity =>
            {
                entity.HasOne(o => o.User)
                    .WithMany(u => u.OAuthProviders)
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(o => new { o.Provider, o.ProviderId }).IsUnique();
                
                entity.Property(o => o.Provider)
                    .HasConversion<int>();
            });

            // Configure Newsletter entity
            modelBuilder.Entity<Newsletter>(entity =>
            {
                entity.HasOne(n => n.User)
                    .WithMany(u => u.Newsletters)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(n => new { n.UserId, n.Type }).IsUnique();
            });

            // Configure UserAnalytics entity
            modelBuilder.Entity<UserAnalytics>(entity =>
            {
                entity.HasOne(ua => ua.User)
                    .WithMany(u => u.UserAnalytics)
                    .HasForeignKey(ua => ua.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(ua => ua.SessionId);
                entity.HasIndex(ua => ua.CreatedAt);
            });

            // Configure UserReport entity
            modelBuilder.Entity<UserReport>(entity =>
            {
                entity.HasOne(ur => ur.Reporter)
                    .WithMany(u => u.ReportsCreated)
                    .HasForeignKey(ur => ur.ReporterId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ur => ur.ResolvedByUser)
                    .WithMany(u => u.ReportsResolved)
                    .HasForeignKey(ur => ur.ResolvedBy)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(ur => new { ur.ReportedService, ur.ReportedId });
                entity.HasIndex(ur => ur.Status);
                
                entity.Property(ur => ur.ReportedType)
                    .HasConversion<int>();
                    
                entity.Property(ur => ur.ReportType)
                    .HasConversion<int>();
                    
                entity.Property(ur => ur.Status)
                    .HasConversion<int>();
            });

            // Configure Bookmark entity
            modelBuilder.Entity<Bookmark>(entity =>
            {
                entity.HasOne(b => b.User)
                    .WithMany(u => u.Bookmarks)
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(b => new { b.UserId, b.PortfolioId }).IsUnique();
                entity.HasIndex(b => b.CreatedAt);
            });
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<OAuthProvider> OAuthProviders { get; set; }
        public DbSet<Newsletter> Newsletters { get; set; }
        public DbSet<UserAnalytics> UserAnalytics { get; set; }
        public DbSet<UserReport> UserReports { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
    }
}