using ApiAggregator.Application.Dtos;
using ApiAggregator.Application.ExternalApis;

namespace ApiAggregator.Tests.Mocks;

public sealed class MockExternalApiProvider : IExternalApiProvider
{
    private readonly IReadOnlyList<AggregatedItemDto> _items;
    private readonly Exception? _exception;

    public string Name { get; }

    public MockExternalApiProvider(
        string name,
        IReadOnlyList<AggregatedItemDto> items) {
        Name = name;
        _items = items;
    }

    public MockExternalApiProvider(
        string name,
        Exception exception) {
        Name = name;
        _items = [];
        _exception = exception;
    }

    public Task<ExternalApiResult> GetItemsAsync(
        AggregationQuery query,
        CancellationToken cancellationToken) {
        if (_exception is not null) {
            throw _exception;
        }

        return Task.FromResult(ExternalApiResult.Success(Name, _items));
    }
}