using ApiAggregator.Application.Dtos;

namespace ApiAggregator.Application.Aggregation;

public interface IAggregationService
{
    /// <summary>
    /// Fetches data from all registered providers and returns one filtered, sorted response.
    /// </summary>
    Task<AggregatedResponseDto> GetAggregatedDataAsync(
        AggregationQuery query, 
        CancellationToken cancellationToken);
}
