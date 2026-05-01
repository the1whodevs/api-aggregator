namespace ApiAggregator.Application.Dtos;

/// <summary>
/// Query options accepted by the aggregation endpoint.
/// </summary>
public sealed class AggregationQuery
{
    public string? Query { get; init; }
    public string? Category { get; init; }
    public string SortBy { get; init; } = "date";
    public string SortDirection { get; init; } = "desc";
}
