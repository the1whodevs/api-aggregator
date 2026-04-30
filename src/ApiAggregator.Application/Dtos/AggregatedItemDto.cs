namespace ApiAggregator.Application.Dtos;

public sealed class AggregatedItemDto
{
    public string Source { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Category { get; init; }
    public string? Url { get; init; }
    public DateTimeOffset? PublishedAt { get; init; }
    public double? RelevanceScore { get; init; }
}