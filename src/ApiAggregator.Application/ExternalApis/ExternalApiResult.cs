using ApiAggregator.Application.Dtos;

namespace ApiAggregator.Application.ExternalApis;

public sealed class ExternalApiResult
{
    public string Source { get; init; } = string.Empty;
    public IReadOnlyList<AggregatedItemDto> Items { get; init; } = [];
    public string? Warning { get; init; }

    public static ExternalApiResult Success(string source, IReadOnlyList<AggregatedItemDto> items) {
        return new ExternalApiResult {
            Source = source, 
            Items = items
        };   
    }
    
    public static ExternalApiResult Failure(string source, string warning) {
        return new ExternalApiResult {
            Source = source, 
            Items = [],
            Warning = warning
        };   
    }
}