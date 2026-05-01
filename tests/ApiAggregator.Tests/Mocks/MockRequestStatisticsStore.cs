using ApiAggregator.Application.Statistics;

namespace ApiAggregator.Tests.Mocks;

public sealed class MockRequestStatisticsStore : IRequestStatisticsStore
{
    private readonly List<(string ApiName, TimeSpan ResponseTime)> _records = [];

    public IReadOnlyList<(string ApiName, TimeSpan ResponseTime)> Records => _records;

    public void RecordRequest(string apiName, TimeSpan responseTime) {
        _records.Add((apiName, responseTime));
    }

    public IReadOnlyList<ApiStatisticsDto> GetStatistics() {
        return [];
    }
}