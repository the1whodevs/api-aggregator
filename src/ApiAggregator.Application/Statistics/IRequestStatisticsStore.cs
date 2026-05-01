namespace ApiAggregator.Application.Statistics;

public interface IRequestStatisticsStore {
    void RecordRequest(string apiName, TimeSpan responseTime);

    IReadOnlyList<ApiStatisticsDto> GetStatistics();
}