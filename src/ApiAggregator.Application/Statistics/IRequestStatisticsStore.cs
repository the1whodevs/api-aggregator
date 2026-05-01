namespace ApiAggregator.Application.Statistics;

/// <summary>
/// Stores runtime metrics for each external provider call.
/// </summary>
public interface IRequestStatisticsStore {
    void RecordRequest(string apiName, TimeSpan responseTime);

    IReadOnlyList<ApiStatisticsDto> GetStatistics();
}
