using ApiAggregator.Application.Caching;
using ApiAggregator.Application.Dtos;
using ApiAggregator.Application.ExternalApis;

namespace ApiAggregator.Infrastructure.ExternalApis.Mock;

public class MockGitHubProvider : IExternalApiProvider {
    public string Name => "GitHub";

    private readonly IExternalApiCache _cache;

    public MockGitHubProvider(IExternalApiCache cache) {
        _cache = cache;
    }
    
    public async Task<ExternalApiResult> GetItemsAsync(AggregationQuery query, CancellationToken cancellationToken) {
        var cacheKey = $"fake-github:{query.Query}:{query.Category}";
        
        if (_cache.TryGetValue<ExternalApiResult>(cacheKey, out var cachedResult)
            && cachedResult is not null)
        {
            return cachedResult;
        }
        
        var items = new List<AggregatedItemDto> {
            new() {
                Source = Name,
                Title = "Amazing Unity C# Game",
                Description = "An awesome game made in Unity.",
                Category = "technology",
                Url = "https://github.com",
                PublishedAt = DateTimeOffset.UtcNow.AddDays(-1),
                RelevanceScore = 0.95
            }
        };

        var result = ExternalApiResult.Success(Name, items);
        
        _cache.Set(
            cacheKey,
            result,
            TimeSpan.FromMinutes(5));

        return result;
    }
}