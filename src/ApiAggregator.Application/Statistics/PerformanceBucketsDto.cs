namespace ApiAggregator.Application.Statistics;

public sealed class PerformanceBucketsDto {
    public long Fast { get; init; }
    public long Average { get; init; }
    public long Slow { get; init; }
}