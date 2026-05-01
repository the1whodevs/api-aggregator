namespace ApiAggregator.Application.Dtos;

/// <summary>
/// Response returned by the aggregation endpoint.
/// </summary>
public sealed class AggregatedResponseDto
{
    public IReadOnlyList<AggregatedItemDto> Items { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];
}
