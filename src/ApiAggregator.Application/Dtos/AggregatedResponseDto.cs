namespace ApiAggregator.Application.Dtos;

public sealed class AggregatedResponseDto
{
    public IReadOnlyList<AggregatedItemDto> Items { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];
}
