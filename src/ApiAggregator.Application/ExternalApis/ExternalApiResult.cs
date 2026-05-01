using ApiAggregator.Application.Dtos;

namespace ApiAggregator.Application.ExternalApis;

/// <summary>
/// Provider-level result. A provider can return data, a warning, or both.
/// </summary>
public sealed class ExternalApiResult
{
    public string Source { get; init; } = string.Empty;
    public IReadOnlyList<AggregatedItemDto> Items { get; init; } = [];
    public string? Warning { get; init; }

    // Factory methods keep provider success/failure handling consistent.
    public static ExternalApiResult Success(string source, IReadOnlyList<AggregatedItemDto> items) {
        return new ExternalApiResult {
            Source = source, 
            Items = items
        };   
    }
    
    public static ExternalApiResult SuccessWithWarning(
        string source,
        IReadOnlyList<AggregatedItemDto> items,
        string warning) {
        return new ExternalApiResult {
            Source = source,
            Items = items,
            Warning = warning
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
