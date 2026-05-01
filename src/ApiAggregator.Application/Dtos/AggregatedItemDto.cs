namespace ApiAggregator.Application.Dtos;

/// <summary>
/// Normalized item returned by any external provider.
/// </summary>
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
