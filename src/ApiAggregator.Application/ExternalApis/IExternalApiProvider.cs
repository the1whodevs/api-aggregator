using ApiAggregator.Application.Dtos;

namespace ApiAggregator.Application.ExternalApis;

/// <summary>
/// Implement this interface to add another external source to the aggregation pipeline.
/// </summary>
public interface IExternalApiProvider
{
    string Name { get; }
    
    Task<ExternalApiResult> GetItemsAsync(AggregationQuery query,
        CancellationToken cancellationToken);
}
