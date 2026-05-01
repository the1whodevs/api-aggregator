using ApiAggregator.Application.Dtos;
using ApiAggregator.Application.ExternalApis;

namespace ApiAggregator.Infrastructure.ExternalApis;

public class MockGitHubProvider : IExternalApiProvider {
    public string Name => "GitHub";
    
    public Task<ExternalApiResult> GetItemsAsync(
        AggregationQuery query, 
        CancellationToken cancellationToken) {
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

        return Task.FromResult(ExternalApiResult.Success(Name, items));
    }
}