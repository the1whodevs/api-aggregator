using ApiAggregator.Application.Dtos;
using ApiAggregator.Application.ExternalApis;

namespace ApiAggregator.Infrastructure.ExternalApis;

public class MockWeatherProvider : IExternalApiProvider {
    public string Name => "Open-Meteo";
    
    public Task<ExternalApiResult> GetItemsAsync(
        AggregationQuery query, 
        CancellationToken cancellationToken) {
        var items = new List<AggregatedItemDto> {
            new() {
                Source = Name,
                Title = "Weather in Athens",
                Description = "Current weather data from Open-Meteo",
                Category = "weather",
                Url = "https://open-meteo.com",
                PublishedAt = DateTimeOffset.UtcNow,
                RelevanceScore = 0.80
            }
        };

        return Task.FromResult(ExternalApiResult.Success(Name, items));
    }
}