using ApiAggregator.Infrastructure.Statistics;
using FluentAssertions;

namespace ApiAggregator.Tests.Statistics;

public sealed class InMemoryRequestStatisticsStoreTests
{
    [Fact]
    public void GetStatistics_WhenRequestsAreRecorded_ReturnsTotalRequestsAndAverageResponseTime() {
        var store = new InMemoryRequestStatisticsStore();

        store.RecordRequest("GitHub", TimeSpan.FromMilliseconds(100));
        store.RecordRequest("GitHub", TimeSpan.FromMilliseconds(200));

        var statistics = store.GetStatistics();

        statistics.Should().ContainSingle();

        var githubStats = statistics[0];

        githubStats.ApiName.Should().Be("GitHub");
        githubStats.TotalRequests.Should().Be(2);
        githubStats.AverageResponseTimeMs.Should().Be(150);
    }

    [Fact]
    public void GetStatistics_WhenRequestsAreRecorded_GroupsRequestsIntoPerformanceBuckets() {
        var store = new InMemoryRequestStatisticsStore();

        store.RecordRequest("GitHub", TimeSpan.FromMilliseconds(50));   // fast
        store.RecordRequest("GitHub", TimeSpan.FromMilliseconds(150));  // average
        store.RecordRequest("GitHub", TimeSpan.FromMilliseconds(300));  // slow

        var statistics = store.GetStatistics();

        var githubStats = statistics.Single(stat => stat.ApiName == "GitHub");

        githubStats.Buckets.Fast.Should().Be(1);
        githubStats.Buckets.Average.Should().Be(1);
        githubStats.Buckets.Slow.Should().Be(1);
    }

    [Fact]
    public void GetStatistics_WhenMultipleApisAreRecorded_KeepsStatisticsSeparate() {
        var store = new InMemoryRequestStatisticsStore();

        store.RecordRequest("GitHub", TimeSpan.FromMilliseconds(50));
        store.RecordRequest("Open-Meteo", TimeSpan.FromMilliseconds(250));

        var statistics = store.GetStatistics();

        statistics.Should().HaveCount(2);

        statistics.Single(stat => stat.ApiName == "GitHub")
            .TotalRequests.Should().Be(1);

        statistics.Single(stat => stat.ApiName == "Open-Meteo")
            .TotalRequests.Should().Be(1);
    }
    
    [Fact]
    public async Task GetStatistics_WhenRequestsAreRecordedConcurrently_RecordsAllRequests() {
        var store = new InMemoryRequestStatisticsStore();

        var tasks = Enumerable.Range(0, 1_000)
            .Select(_ => Task.Run(() =>
                store.RecordRequest("GitHub", TimeSpan.FromMilliseconds(50))));

        await Task.WhenAll(tasks);

        var statistics = store.GetStatistics();

        var githubStats = statistics.Single(stat => stat.ApiName == "GitHub");

        githubStats.TotalRequests.Should().Be(1_000);
        githubStats.Buckets.Fast.Should().Be(1_000);
    }

    [Fact]
    public void GetPerformanceAnalysis_WhenRecentAverageIsNormal_DoesNotReturnAnomaly() {
        var nowUtc = new DateTimeOffset(2026, 5, 1, 12, 0, 0, TimeSpan.Zero);
        var store = new InMemoryRequestStatisticsStore();

        RecordSamples(store, "GitHub", TimeSpan.FromMilliseconds(100), nowUtc.AddMinutes(-10), 5);
        RecordSamples(store, "GitHub", TimeSpan.FromMilliseconds(120), nowUtc.AddMinutes(-2), 3);

        var analysis = store.GetPerformanceAnalysis(
            TimeSpan.FromMinutes(5),
            nowUtc,
            thresholdPercentage: 50,
            minimumRecentSamples: 3);

        var githubAnalysis = analysis.Should().ContainSingle().Subject;
        githubAnalysis.IsAnomaly.Should().BeFalse();
        githubAnalysis.RecentRequestCount.Should().Be(3);
        githubAnalysis.TotalAverageResponseTimeMs.Should().Be(100);
        githubAnalysis.RecentAverageResponseTimeMs.Should().Be(120);
    }

    [Fact]
    public void GetPerformanceAnalysis_WhenRecentAverageExceedsThreshold_ReturnsAnomaly() {
        var nowUtc = new DateTimeOffset(2026, 5, 1, 12, 0, 0, TimeSpan.Zero);
        var store = new InMemoryRequestStatisticsStore();

        RecordSamples(store, "GitHub", TimeSpan.FromMilliseconds(100), nowUtc.AddMinutes(-10), 5);
        RecordSamples(store, "GitHub", TimeSpan.FromMilliseconds(200), nowUtc.AddMinutes(-2), 3);

        var analysis = store.GetPerformanceAnalysis(
            TimeSpan.FromMinutes(5),
            nowUtc,
            thresholdPercentage: 50,
            minimumRecentSamples: 3);

        var githubAnalysis = analysis.Should().ContainSingle().Subject;
        githubAnalysis.IsAnomaly.Should().BeTrue();
        githubAnalysis.RecentRequestCount.Should().Be(3);
        githubAnalysis.TotalAverageResponseTimeMs.Should().Be(100);
        githubAnalysis.RecentAverageResponseTimeMs.Should().Be(200);
        githubAnalysis.PercentageIncrease.Should().Be(100);
    }

    [Fact]
    public void GetPerformanceAnalysis_WhenRecentSampleCountIsBelowMinimum_DoesNotReturnAnomaly() {
        var nowUtc = new DateTimeOffset(2026, 5, 1, 12, 0, 0, TimeSpan.Zero);
        var store = new InMemoryRequestStatisticsStore();

        RecordSamples(store, "GitHub", TimeSpan.FromMilliseconds(100), nowUtc.AddMinutes(-10), 5);
        RecordSamples(store, "GitHub", TimeSpan.FromMilliseconds(300), nowUtc.AddMinutes(-2), 2);

        var analysis = store.GetPerformanceAnalysis(
            TimeSpan.FromMinutes(5),
            nowUtc,
            thresholdPercentage: 50,
            minimumRecentSamples: 3);

        var githubAnalysis = analysis.Should().ContainSingle().Subject;
        githubAnalysis.IsAnomaly.Should().BeFalse();
        githubAnalysis.RecentRequestCount.Should().Be(2);
        githubAnalysis.PercentageIncrease.Should().Be(200);
    }

    private static void RecordSamples(
        InMemoryRequestStatisticsStore store,
        string apiName,
        TimeSpan responseTime,
        DateTimeOffset firstTimestampUtc,
        int count) {
        for (var index = 0; index < count; index++) {
            store.RecordRequest(
                apiName,
                responseTime,
                firstTimestampUtc.AddSeconds(index));
        }
    }
}
