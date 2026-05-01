namespace ApiAggregator.Application.Statistics;

/// <summary>
/// Stores runtime metrics for each external provider call.
/// </summary>
public interface IRequestStatisticsStore {
    void RecordRequest(string apiName, TimeSpan responseTime);

    void RecordRequest(string apiName, TimeSpan responseTime, DateTimeOffset timestampUtc);

    IReadOnlyList<ApiStatisticsDto> GetStatistics();

    IReadOnlyList<ApiPerformanceAnalysisDto> GetPerformanceAnalysis(
        TimeSpan recentWindow,
        DateTimeOffset nowUtc,
        double thresholdPercentage = 50,
        int minimumRecentSamples = 3);
}
