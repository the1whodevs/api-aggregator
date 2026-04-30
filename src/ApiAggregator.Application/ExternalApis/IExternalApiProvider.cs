using ApiAggregator.Application.Dtos;

namespace ApiAggregator.Application.ExternalApis;

public interface IExternalApiProvider
{
    string Name { get; }
    
    Task<ExternalApiResult> GetItemsAsync(AggregationQuery query,
        CancellationToken cancellationToken);
}