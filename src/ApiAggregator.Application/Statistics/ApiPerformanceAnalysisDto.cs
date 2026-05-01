namespace ApiAggregator.Application.Statistics;

/// <summary>
/// Performance comparison used by the background anomaly detector.
/// </summary>
public sealed class ApiPerformanceAnalysisDto
{
    public string ApiName { get; init; } = string.Empty;
    public double TotalAverageResponseTimeMs { get; init; }
    public double RecentAverageResponseTimeMs { get; init; }
    public int RecentRequestCount { get; init; }
    public bool IsAnomaly { get; init; }
    public double PercentageIncrease { get; init; }
}
