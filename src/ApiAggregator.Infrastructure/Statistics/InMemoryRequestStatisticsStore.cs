using System.Collections.Concurrent;
using ApiAggregator.Application.Statistics;

namespace ApiAggregator.Infrastructure.Statistics;

public sealed class InMemoryRequestStatisticsStore : IRequestStatisticsStore {
    private static readonly TimeSpan SampleRetention = TimeSpan.FromMinutes(60);

    // The dictionary protects provider lookup; each accumulator protects its own counters.
    private readonly ConcurrentDictionary<string, ApiStatisticsAccumulator> _statistics = new();
    
    public void RecordRequest(string apiName, TimeSpan responseTime) {
        RecordRequest(apiName, responseTime, DateTimeOffset.UtcNow);
    }

    public void RecordRequest(string apiName, TimeSpan responseTime, DateTimeOffset timestampUtc) {
        var accumulator = _statistics.GetOrAdd(
            apiName,
            _ => new ApiStatisticsAccumulator());

        accumulator.Record(apiName, responseTime, timestampUtc, SampleRetention);
    }
    
    public IReadOnlyList<ApiStatisticsDto> GetStatistics() {
        return _statistics.Select(pair => {
            var snapshot = pair.Value.GetSnapshot();

            return new ApiStatisticsDto {
                ApiName = pair.Key,
                TotalRequests = snapshot.TotalRequests,
                AverageResponseTimeMs = snapshot.AverageResponseTimeMs,
                Buckets = new PerformanceBucketsDto {
                    Fast = snapshot.Fast,
                    Average = snapshot.Average,
                    Slow = snapshot.Slow
                }
            };
        }).OrderBy(stat => stat.ApiName).ToList();
    }

    public IReadOnlyList<ApiPerformanceAnalysisDto> GetPerformanceAnalysis(
        TimeSpan recentWindow,
        DateTimeOffset nowUtc,
        double thresholdPercentage = 50,
        int minimumRecentSamples = 3) {
        if (recentWindow <= TimeSpan.Zero) {
            throw new ArgumentOutOfRangeException(nameof(recentWindow), "Recent window must be greater than zero.");
        }

        if (thresholdPercentage < 0) {
            throw new ArgumentOutOfRangeException(nameof(thresholdPercentage), "Threshold percentage cannot be negative.");
        }

        if (minimumRecentSamples < 1) {
            throw new ArgumentOutOfRangeException(nameof(minimumRecentSamples), "Minimum recent samples must be at least one.");
        }

        return _statistics.Select(pair => {
            var snapshot = pair.Value.GetSnapshot();
            var recentCutoffUtc = nowUtc - recentWindow;
            var recentSamples = snapshot.Samples
                .Where(sample => sample.TimestampUtc >= recentCutoffUtc && sample.TimestampUtc <= nowUtc)
                .ToList();

            var baselineSamples = snapshot.Samples
                .Where(sample => sample.TimestampUtc < recentCutoffUtc)
                .ToList();

            var recentAverageMs = recentSamples.Count == 0
                ? 0
                : recentSamples.Average(sample => sample.ResponseTime.TotalMilliseconds);

            // Prefer older retained samples as the baseline. Fall back to the total
            // average so APIs with only recent retained samples still produce a snapshot.
            var baselineAverageMs = baselineSamples.Count == 0
                ? snapshot.AverageResponseTimeMs
                : baselineSamples.Average(sample => sample.ResponseTime.TotalMilliseconds);

            var percentageIncrease = baselineAverageMs <= 0 || recentSamples.Count == 0
                ? 0
                : ((recentAverageMs - baselineAverageMs) / baselineAverageMs) * 100;

            return new ApiPerformanceAnalysisDto {
                ApiName = pair.Key,
                TotalAverageResponseTimeMs = baselineAverageMs,
                RecentAverageResponseTimeMs = recentAverageMs,
                RecentRequestCount = recentSamples.Count,
                IsAnomaly = recentSamples.Count >= minimumRecentSamples &&
                            baselineAverageMs > 0 &&
                            percentageIncrease > thresholdPercentage,
                PercentageIncrease = percentageIncrease
            };
        }).OrderBy(analysis => analysis.ApiName).ToList();
    }

    private sealed class ApiStatisticsAccumulator {
        private readonly object _lock = new();

        private long _totalRequests;
        private double _totalResponseTimeMs;

        private long _fast;
        private long _average;
        private long _slow;

        private readonly List<RequestPerformanceSample> _samples = [];

        public void Record(
            string apiName,
            TimeSpan responseTime,
            DateTimeOffset timestampUtc,
            TimeSpan sampleRetention) {
            var responseTimeMs = responseTime.TotalMilliseconds;

            lock (_lock) {
                _totalRequests++;
                _totalResponseTimeMs += responseTimeMs;

                if (responseTimeMs < 100) {
                    _fast++;
                }
                else if (responseTimeMs <= 200) {
                    _average++;
                }
                else {
                    _slow++;
                }

                _samples.Add(new RequestPerformanceSample {
                    ApiName = apiName,
                    ResponseTime = responseTime,
                    TimestampUtc = timestampUtc
                });

                var oldestSampleToKeepUtc = timestampUtc - sampleRetention;
                _samples.RemoveAll(sample => sample.TimestampUtc < oldestSampleToKeepUtc);
            }
        }

        public ApiStatisticsSnapshot GetSnapshot() {
            // Return a consistent point-in-time view without exposing mutable counters.
            lock (_lock) {
                return new ApiStatisticsSnapshot {
                    TotalRequests = _totalRequests,
                    AverageResponseTimeMs = _totalRequests == 0 ? 0 : _totalResponseTimeMs / _totalRequests,
                    Fast = _fast,
                    Average = _average,
                    Slow = _slow,
                    Samples = _samples.ToList()
                };
            }
        }
    }

    private sealed class ApiStatisticsSnapshot 
    {
        public long TotalRequests { get; init; }
        public double AverageResponseTimeMs { get; init; }
        public long Fast { get; init; }
        public long Average { get; init; }
        public long Slow { get; init; }
        public IReadOnlyList<RequestPerformanceSample> Samples { get; init; } = [];
    }

    private sealed class RequestPerformanceSample
    {
        public string ApiName { get; init; } = string.Empty;
        public TimeSpan ResponseTime { get; init; }
        public DateTimeOffset TimestampUtc { get; init; }
    }
}
