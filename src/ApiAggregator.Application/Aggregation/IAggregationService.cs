using ApiAggregator.Application.Dtos;

namespace ApiAggregator.Application.Aggregation;

public interface IAggregationService
{
    Task<AggregatedResponseDto> GetAggregatedDataAsync(
        AggregationQuery query, 
        CancellationToken cancellationToken);
}