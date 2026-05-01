namespace ApiAggregator.Api.Performance;

public sealed class PerformanceAnomalyOptions
{
    public const string SectionName = "PerformanceAnomaly";

    public bool Enabled { get; init; } = true;
    public int CheckIntervalSeconds { get; init; } = 60;
    public int RecentWindowMinutes { get; init; } = 5;
    public double ThresholdPercentage { get; init; } = 50;
    public int MinimumRecentSamples { get; init; } = 3;

    public TimeSpan CheckInterval => TimeSpan.FromSeconds(CheckIntervalSeconds);
    public TimeSpan RecentWindow => TimeSpan.FromMinutes(RecentWindowMinutes);
}
