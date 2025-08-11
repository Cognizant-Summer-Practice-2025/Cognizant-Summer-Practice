namespace backend_AI.Services.Abstractions
{
    public record PortfolioCandidate(string Id, string UserId, int ExperienceScore, int SkillsScore, int BlogScore, int BioScore, int ProjectQualityScore, int TotalScore);

    public interface IPortfolioRankingService
    {
        List<PortfolioCandidate> SelectTopCandidates(string portfoliosJson, int topN = 10);
    }
}


