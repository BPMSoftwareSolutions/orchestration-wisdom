using OrchestrationWisdom.Models;

namespace OrchestrationWisdom.Services;

public interface IPatternService
{
    Task<List<Pattern>> GetAllPatternsAsync();
    Task<Pattern?> GetPatternBySlugAsync(string slug);
    Task<List<Pattern>> GetFeaturedPatternsAsync(int count = 6);
    Task<List<Pattern>> FilterPatternsAsync(FilterOptions filter);
}
