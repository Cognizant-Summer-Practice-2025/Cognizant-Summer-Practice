using AutoFixture;
using backend_portfolio.Models;

namespace backend_portfolio.tests.Helpers;

public static class TestDataFactory
{
    private static readonly Fixture _fixture = new();

    static TestDataFactory()
    {
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        
        _fixture.Register(() => DateOnly.FromDateTime(DateTime.Now.AddDays(-new Random().Next(0, 3650))));
    }

    public static Portfolio CreatePortfolio(Guid? userId = null)
    {
        return _fixture.Build<Portfolio>()
            .With(p => p.UserId, userId ?? Guid.NewGuid())
            .With(p => p.TemplateId, Guid.NewGuid())
            .With(p => p.Visibility, Visibility.Public)
            .With(p => p.IsPublished, true)
            .With(p => p.CreatedAt, DateTime.UtcNow)
            .With(p => p.UpdatedAt, DateTime.UtcNow)
            .Without(p => p.Template)
            .Create();
    }

    public static Project CreateProject(Guid? portfolioId = null)
    {
        return _fixture.Build<Project>()
            .With(p => p.PortfolioId, portfolioId ?? Guid.NewGuid())
            .With(p => p.CreatedAt, DateTime.UtcNow)
            .With(p => p.UpdatedAt, DateTime.UtcNow)
            .Create();
    }

    public static BlogPost CreateBlogPost(Guid? portfolioId = null)
    {
        return _fixture.Build<BlogPost>()
            .With(b => b.PortfolioId, portfolioId ?? Guid.NewGuid())
            .With(b => b.CreatedAt, DateTime.UtcNow)
            .With(b => b.UpdatedAt, DateTime.UtcNow)
            .Create();
    }

    public static Experience CreateExperience(Guid? portfolioId = null)
    {
        return _fixture.Build<Experience>()
            .With(e => e.PortfolioId, portfolioId ?? Guid.NewGuid())
            .With(e => e.StartDate, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)))
            .With(e => e.EndDate, DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)))
            .Create();
    }

    public static Skill CreateSkill(Guid? portfolioId = null)
    {
        return _fixture.Build<Skill>()
            .With(s => s.PortfolioId, portfolioId ?? Guid.NewGuid())
            .Create();
    }

    public static Bookmark CreateBookmark(Guid? userId = null, Guid? portfolioId = null)
    {
        return _fixture.Build<Bookmark>()
            .With(b => b.UserId, userId ?? Guid.NewGuid())
            .With(b => b.PortfolioId, portfolioId ?? Guid.NewGuid())
            .Create();
    }

    public static PortfolioTemplate CreatePortfolioTemplate()
    {
        return _fixture.Build<PortfolioTemplate>()
            .With(pt => pt.CreatedAt, DateTime.UtcNow)
            .With(pt => pt.UpdatedAt, DateTime.UtcNow)
            .Create();
    }

    public static List<T> CreateList<T>(int count = 3) where T : class
    {
        return _fixture.CreateMany<T>(count).ToList();
    }
} 