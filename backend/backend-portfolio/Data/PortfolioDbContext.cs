using Microsoft.EntityFrameworkCore;
using backend_portfolio.Models;

namespace backend_portfolio.Data
{
    public class PortfolioDbContext : DbContext
    {
        public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<PortfolioTemplate> PortfolioTemplates { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Experience> Experience { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Portfolio configuration
            modelBuilder.Entity<Portfolio>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.Property(e => e.ViewCount).HasDefaultValue(0);
                entity.Property(e => e.LikeCount).HasDefaultValue(0);
                entity.Property(e => e.Visibility).HasDefaultValue(Visibility.Public);
                entity.Property(e => e.IsPublished).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.Template)
                    .WithMany(t => t.Portfolios)
                    .HasForeignKey(e => e.TemplateId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // PortfolioTemplate configuration
            modelBuilder.Entity<PortfolioTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Project configuration
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PortfolioId);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Featured).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(e => e.Portfolio)
                    .WithMany(p => p.Projects)
                    .HasForeignKey(e => e.PortfolioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Experience configuration
            modelBuilder.Entity<Experience>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PortfolioId);
                entity.Property(e => e.JobTitle).IsRequired();
                entity.Property(e => e.CompanyName).IsRequired();
                entity.Property(e => e.IsCurrent).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(e => e.Portfolio)
                    .WithMany(p => p.Experience)
                    .HasForeignKey(e => e.PortfolioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Skill configuration
            modelBuilder.Entity<Skill>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PortfolioId);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(e => e.Portfolio)
                    .WithMany(p => p.Skills)
                    .HasForeignKey(e => e.PortfolioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // BlogPost configuration
            modelBuilder.Entity<BlogPost>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PortfolioId);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.IsPublished).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(e => e.Portfolio)
                    .WithMany(p => p.BlogPosts)
                    .HasForeignKey(e => e.PortfolioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Bookmark configuration
            modelBuilder.Entity<Bookmark>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.PortfolioId);
                // Unique constraint: one user can only bookmark a portfolio once
                entity.HasIndex(e => new { e.UserId, e.PortfolioId }).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                entity.HasOne(e => e.Portfolio)
                    .WithMany(p => p.Bookmarks)
                    .HasForeignKey(e => e.PortfolioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}