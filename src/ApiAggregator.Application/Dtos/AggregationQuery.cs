namespace ApiAggregator.Application.Dtos;

public sealed class AggregationQuery
{
    public string? Query { get; init; }
    public string? Category { get; init; }
    public string SortBy { get; init; } = "date";
    public string SortDirection { get; init; } = "desc";
}