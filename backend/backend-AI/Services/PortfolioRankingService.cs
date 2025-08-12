using System.Text.Json;
using backend_AI.Services.Abstractions;

namespace backend_AI.Services
{
    public class PortfolioRankingService : IPortfolioRankingService
    {
        private readonly ILogger<PortfolioRankingService> _logger;

        public PortfolioRankingService(ILogger<PortfolioRankingService> logger)
        {
            _logger = logger;
        }

        public List<PortfolioCandidate> SelectTopCandidates(string portfoliosJson, int topN = 10)
        {
            try
            {
                using var doc = JsonDocument.Parse(portfoliosJson);
                if (doc.RootElement.ValueKind != JsonValueKind.Array)
                {
                    _logger.LogWarning("Ranking: root element is not an array");
                    return new List<PortfolioCandidate>();
                }
                var totalInput = doc.RootElement.GetArrayLength();
                _logger.LogInformation("Ranking: received {Count} portfolios, payload length {Len} chars", totalInput, portfoliosJson.Length);
                var now = DateOnly.FromDateTime(DateTime.UtcNow);
                var candidates = new List<PortfolioCandidate>();
                var bestPerUser = new Dictionary<string, PortfolioCandidate>();
                foreach (var p in doc.RootElement.EnumerateArray())
                {
                    var id = p.TryGetProperty("id", out var idEl) ? idEl.GetString() ?? string.Empty : string.Empty;
                    if (string.IsNullOrEmpty(id)) continue;
                    var userId = p.TryGetProperty("userId", out var uidEl) ? uidEl.GetString() ?? string.Empty : string.Empty;
                    if (string.IsNullOrEmpty(userId)) userId = id; // fallback prevent grouping collapse
                    _logger.LogDebug("Ranking: processing portfolio {Id} for user {UserId}", id, userId);

                    // Experience score
                    int experienceScore = 0;
                    if (p.TryGetProperty("experience", out var expArr) && expArr.ValueKind == JsonValueKind.Array)
                    {
                        var expCount = expArr.GetArrayLength();
                        int totalMonths = 0;
                        foreach (var e in expArr.EnumerateArray())
                        {
                            var startStr = e.TryGetProperty("startDate", out var sd) ? sd.GetString() : null;
                            var endStr = e.TryGetProperty("endDate", out var ed) ? ed.GetString() : null;
                            var startOk = DateOnly.TryParse(startStr, out var start);
                            var endOk = DateOnly.TryParse(endStr, out var end);
                            var endDate = endOk ? end : now;
                            if (startOk)
                            {
                                var months = (endDate.Year - start.Year) * 12 + (endDate.Month - start.Month);
                                if (months < 0) months = 0;
                                totalMonths += months;
                                _logger.LogDebug("Ranking: {Id} experience item start={Start} end={End} months={Months}", id, startStr, endOk ? endStr : "present", months);
                            }
                        }
                        experienceScore = expCount * 10 + totalMonths;
                        _logger.LogDebug("Ranking: {Id} experience expCount={ExpCount} totalMonths={TotalMonths} experienceScore={Score}", id, expCount, totalMonths, experienceScore);
                    }

                    // Skills score
                    int skillsScore = 0;
                    if (p.TryGetProperty("skills", out var skillsArr) && skillsArr.ValueKind == JsonValueKind.Array)
                    {
                        int sumProficiency = 0;
                        var categories = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                        foreach (var s in skillsArr.EnumerateArray())
                        {
                            if (s.TryGetProperty("proficiencyLevel", out var prof) && prof.ValueKind == JsonValueKind.Number)
                            {
                                sumProficiency += prof.GetInt32();
                                _logger.LogDebug("Ranking: {Id} skill proficiency={Prof}", id, prof.GetInt32());
                            }
                            var cat = s.TryGetProperty("category", out var c) ? (c.GetString() ?? string.Empty) : string.Empty;
                            if (!string.IsNullOrEmpty(cat))
                            {
                                categories[cat] = categories.TryGetValue(cat, out var cnt) ? cnt + 1 : 1;
                                _logger.LogDebug("Ranking: {Id} skill category counted {Category} -> {Count}", id, cat, categories[cat]);
                            }
                        }
                        var distinctCats = categories.Keys.Count;
                        var catBonus = categories.Values.Sum(v => Math.Min(5, v));
                        skillsScore = sumProficiency + distinctCats * 5 + catBonus;
                         var categoriesSummary = string.Join(", ", categories.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
                         _logger.LogDebug("Ranking: {Id} skills sumProficiency={Sum} distinctCats={Distinct} catBonus={Bonus} categories=[{Cats}] skillsScore={Score}", id, sumProficiency, distinctCats, catBonus, categoriesSummary, skillsScore);
                    }

                    // Blog score
                    int blogScore = 0;
                    if (p.TryGetProperty("blogPosts", out var blogArr) && blogArr.ValueKind == JsonValueKind.Array)
                    {
                        int relevant = 0;
                        foreach (var b in blogArr.EnumerateArray())
                        {
                            var published = b.TryGetProperty("isPublished", out var ip) && ip.ValueKind == JsonValueKind.True;
                            var excerptLen = b.TryGetProperty("excerpt", out var ex) && ex.ValueKind == JsonValueKind.String ? (ex.GetString()?.Length ?? 0) : 0;
                            var contentLen = b.TryGetProperty("content", out var co) && co.ValueKind == JsonValueKind.String ? (co.GetString()?.Length ?? 0) : 0;
                            var tagsOk = b.TryGetProperty("tags", out var tg) && tg.ValueKind == JsonValueKind.Array && tg.GetArrayLength() > 0;
                            if (published && (excerptLen >= 100 || contentLen >= 100) && tagsOk)
                            {
                                relevant++;
                                _logger.LogDebug("Ranking: {Id} blog considered relevant (published={Published}, excerptLen={ExLen}, contentLen={CoLen}, tags={Tags})", id, published, excerptLen, contentLen, tagsOk);
                            }
                            else
                            {
                                _logger.LogDebug("Ranking: {Id} blog NOT relevant (published={Published}, excerptLen={ExLen}, contentLen={CoLen}, tags={Tags})", id, published, excerptLen, contentLen, tagsOk);
                            }
                        }
                        blogScore = relevant * 5;
                        _logger.LogDebug("Ranking: {Id} blog relevantCount={Relevant} blogScore={Score}", id, relevant, blogScore);
                    }

                    // Bio score
                    int bioScore = 0;
                    var bio = p.TryGetProperty("bio", out var bioEl) && bioEl.ValueKind == JsonValueKind.String ? bioEl.GetString() ?? string.Empty : string.Empty;
                    if (!string.IsNullOrWhiteSpace(bio))
                    {
                        var len = bio.Length;
                        var sentences = bio.Split('.', '!', '?').Count(s => s.Trim().Length > 0);
                        var placeholder = bio.Contains("lorem", StringComparison.OrdinalIgnoreCase) || bio.Distinct().Count() <= 3;
                        var hasKeywords = bio.IndexOf("developer", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                          bio.IndexOf("engineer", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                          bio.IndexOf("designer", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                          bio.IndexOf("react", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                          bio.IndexOf("node", StringComparison.OrdinalIgnoreCase) >= 0;
                        if (len >= 80 && len <= 500) bioScore += 10;
                        if (sentences >= 2) bioScore += 5;
                        if (hasKeywords) bioScore += 5;
                        if (placeholder) bioScore -= 10;
                        _logger.LogDebug("Ranking: {Id} bio len={Len} sentences={Sentences} hasKeywords={HasKeywords} placeholder={Placeholder} bioScore={Score}", id, len, sentences, hasKeywords, placeholder, bioScore);
                    }

                    // Project quality score
                    int projectQuality = 0;
                    if (p.TryGetProperty("projects", out var projArr) && projArr.ValueKind == JsonValueKind.Array)
                    {
                        int projCount = 0;
                        foreach (var pr in projArr.EnumerateArray())
                        {
                            projCount++;
                            var descLen = pr.TryGetProperty("description", out var d) && d.ValueKind == JsonValueKind.String ? (d.GetString()?.Length ?? 0) : 0;
                            var descPts = descLen >= 80 ? 5 : 0;
                            var hasDemo = pr.TryGetProperty("demoUrl", out var du) && du.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(du.GetString());
                            var hasGit = pr.TryGetProperty("githubUrl", out var gu) && gu.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(gu.GetString());
                            var demoPts = hasDemo ? 3 : 0;
                            var gitPts = hasGit ? 3 : 0;
                            var techCount = pr.TryGetProperty("technologies", out var techArr) && techArr.ValueKind == JsonValueKind.Array ? techArr.GetArrayLength() : 0;
                            var techPts = Math.Min(5, techCount);
                            var projectPts = descPts + demoPts + gitPts + techPts;
                            projectQuality += projectPts;
                            _logger.LogDebug("Ranking: {Id} project#{Idx} descLen={DescLen} descPts={DescPts} demo={Demo} git={Git} techCount={TechCount} techPts={TechPts} projectPts={ProjectPts}", id, projCount, descLen, descPts, hasDemo, hasGit, techCount, techPts, projectPts);
                        }
                        // small bonus for more projects, capped
                        projectQuality += Math.Min(10, Math.Max(0, projCount - 1));
                        _logger.LogDebug("Ranking: {Id} projects count={Count} projectQuality(with bonus)={Score}", id, projCount, projectQuality);
                    }

                    var total = experienceScore + skillsScore + blogScore + bioScore + projectQuality;
                    var cand = new PortfolioCandidate(id, userId, experienceScore, skillsScore, blogScore, bioScore, projectQuality, total);
                    if (bestPerUser.TryGetValue(userId, out var existing))
                    {
                        if (cand.TotalScore > existing.TotalScore)
                        {
                            bestPerUser[userId] = cand;
                            _logger.LogDebug("Ranking: updated best for user {UserId}: {OldId}({OldScore}) -> {NewId}({NewScore})", userId, existing.Id, existing.TotalScore, cand.Id, cand.TotalScore);
                        }
                        else
                        {
                            _logger.LogDebug("Ranking: kept existing best for user {UserId}: {KeepId}({KeepScore}) vs {NewId}({NewScore})", userId, existing.Id, existing.TotalScore, cand.Id, cand.TotalScore);
                        }
                    }
                    else
                    {
                        bestPerUser[userId] = cand;
                        _logger.LogDebug("Ranking: set initial best for user {UserId}: {Id}({Score})", userId, cand.Id, cand.TotalScore);
                    }
                    candidates.Add(cand);
                    _logger.LogDebug("Ranking: cand {Id} exp={Exp} skills={Skills} blog={Blog} bio={Bio} proj={Proj} total={Total}",
                        cand.Id, cand.ExperienceScore, cand.SkillsScore, cand.BlogScore, cand.BioScore, cand.ProjectQualityScore, cand.TotalScore);
                }
                _logger.LogInformation("Ranking: computed {Count} candidates", candidates.Count);
                // Deduplicate by user: only best portfolio per user is considered further
                var perUser = bestPerUser.Values.ToList();
                _logger.LogInformation("Ranking: users={Users} uniquePortfolios={Ports}", bestPerUser.Count, perUser.Count);
                var ordered = perUser
                    .OrderByDescending(c => c.TotalScore)
                    .ThenByDescending(c => c.SkillsScore)
                    .ThenByDescending(c => c.ExperienceScore)
                    .ThenByDescending(c => c.ProjectQualityScore)
                    .ThenByDescending(c => c.BlogScore)
                    .Take(topN)
                    .ToList();
                var preview = string.Join(", ", ordered.Take(Math.Min(5, ordered.Count)).Select(c => $"{c.Id}:{c.TotalScore}"));
                _logger.LogInformation("Ranking: top {TopN} preview {Preview}", ordered.Count, preview);
                foreach (var (c, idx) in ordered.Select((c, i) => (c, i + 1)))
                {
                    _logger.LogDebug("Ranking: TOP#{Rank} {Id} user={UserId} total={Total} exp={Exp} skills={Skills} blog={Blog} bio={Bio} proj={Proj}", idx, c.Id, c.UserId, c.TotalScore, c.ExperienceScore, c.SkillsScore, c.BlogScore, c.BioScore, c.ProjectQualityScore);
                }
                return ordered;
            }
            catch
            {
                return new List<PortfolioCandidate>();
            }
        }
    }
}


