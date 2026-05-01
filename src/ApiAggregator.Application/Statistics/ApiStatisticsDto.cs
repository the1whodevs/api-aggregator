namespace ApiAggregator.Application.Statistics;

/// <summary>
/// Runtime metrics for one external API provider.
/// </summary>
public sealed class ApiStatisticsDto {
    public string ApiName { get; init; } = string.Empty;
    public long TotalRequests { get; init; }
    public double AverageResponseTimeMs { get; init; }
    public PerformanceBucketsDto Buckets { get; init; } = new();
}
