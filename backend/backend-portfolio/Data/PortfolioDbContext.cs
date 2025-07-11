using Microsoft.EntityFrameworkCore;
using backend_portfolio.Models;

namespace backend_portfolio.Data
{
    public class PortfolioDbContext : DbContext
    {
        public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Portfolio configurations
            modelBuilder.Entity<Portfolio>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired();
                entity.HasOne(e => e.Template)
                      .WithMany(t => t.Portfolios)
                      .HasForeignKey(e => e.TemplateId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // PortfolioTemplate configurations
            modelBuilder.Entity<PortfolioTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
            });

            // Project configurations
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired();
                entity.HasOne(e => e.Portfolio)
                      .WithMany(p => p.Projects)
                      .HasForeignKey(e => e.PortfolioId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Experience configurations
            modelBuilder.Entity<Experience>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.JobTitle).IsRequired();
                entity.Property(e => e.CompanyName).IsRequired();
                entity.HasOne(e => e.Portfolio)
                      .WithMany(p => p.Experiences)
                      .HasForeignKey(e => e.PortfolioId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Skill configurations
            modelBuilder.Entity<Skill>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // PortfolioSkill configurations (many-to-many relationship table)
            modelBuilder.Entity<PortfolioSkill>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Portfolio)
                      .WithMany(p => p.PortfolioSkills)
                      .HasForeignKey(e => e.PortfolioId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Skill)
                      .WithMany(s => s.PortfolioSkills)
                      .HasForeignKey(e => e.SkillId)
                      .OnDelete(DeleteBehavior.Cascade);
                // Create unique index on PortfolioId and SkillId
                entity.HasIndex(e => new { e.PortfolioId, e.SkillId }).IsUnique();
            });

            // BlogPost configurations
            modelBuilder.Entity<BlogPost>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Slug).IsRequired();
                entity.Property(e => e.Content).IsRequired();
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.HasOne(e => e.Portfolio)
                      .WithMany(p => p.BlogPosts)
                      .HasForeignKey(e => e.PortfolioId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // BlogPostTag configurations
            modelBuilder.Entity<BlogPostTag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TagName).IsRequired();
                entity.HasOne(e => e.BlogPost)
                      .WithMany(b => b.Tags)
                      .HasForeignKey(e => e.BlogPostId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // PortfolioView configurations
            modelBuilder.Entity<PortfolioView>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Portfolio)
                      .WithMany(p => p.Views)
                      .HasForeignKey(e => e.PortfolioId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // PortfolioLike configurations
            modelBuilder.Entity<PortfolioLike>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Portfolio)
                      .WithMany(p => p.Likes)
                      .HasForeignKey(e => e.PortfolioId)
                      .OnDelete(DeleteBehavior.Cascade);
                // Create unique index on PortfolioId and UserId
                entity.HasIndex(e => new { e.PortfolioId, e.UserId }).IsUnique();
            });

            // Bookmark configurations
            modelBuilder.Entity<Bookmark>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Portfolio)
                      .WithMany(p => p.Bookmarks)
                      .HasForeignKey(e => e.PortfolioId)
                      .OnDelete(DeleteBehavior.Cascade);
                // Create unique index on UserId and PortfolioId
                entity.HasIndex(e => new { e.UserId, e.PortfolioId }).IsUnique();
            });

            // SearchQuery configurations
            modelBuilder.Entity<SearchQuery>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.ClickedPortfolio)
                      .WithMany(p => p.SearchQueries)
                      .HasForeignKey(e => e.ClickedPortfolioId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }

        // DbSet properties for all models
        public DbSet<Portfolio> Portfolios { get; set; }
        public DbSet<PortfolioTemplate> PortfolioTemplates { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Experience> Experiences { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<PortfolioSkill> PortfolioSkills { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<BlogPostTag> BlogPostTags { get; set; }
        public DbSet<PortfolioView> PortfolioViews { get; set; }
        public DbSet<PortfolioLike> PortfolioLikes { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<SearchQuery> SearchQueries { get; set; }
    }
}