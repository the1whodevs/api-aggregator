namespace ApiAggregator.Application.Statistics;

public sealed class ApiStatisticsDto {
    public string ApiName { get; init; } = string.Empty;
    public long TotalRequests { get; init; }
    public double AverageResponseTimeMs { get; init; }
    public PerformanceBucketsDto Buckets { get; init; } = new();
}