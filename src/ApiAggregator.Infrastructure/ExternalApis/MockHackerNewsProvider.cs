using ApiAggregator.Application.Dtos;
using ApiAggregator.Application.ExternalApis;

namespace ApiAggregator.Infrastructure.ExternalApis;

public class MockHackerNewsProvider : IExternalApiProvider {
    public string Name => "Hacker News";
    
    public Task<ExternalApiResult> GetItemsAsync(AggregationQuery query, CancellationToken cancellationToken) {
        var items = new List<AggregatedItemDto> {
            new() {
                Source = Name,
                Title = "An Unreal C# Engineering Achievement",
                Description = "Example Hacker News result.",
                Category = "technology",
                Url = "https://news.ycombinator.com",
                PublishedAt = DateTimeOffset.UtcNow.AddHours(-3),
                RelevanceScore = 0.88
            }
        };

        return Task.FromResult(ExternalApiResult.Success(Name, items));
    }
}