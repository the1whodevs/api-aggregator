using System.Collections.Concurrent;
using ApiAggregator.Application.Statistics;

namespace ApiAggregator.Infrastructure.Statistics;

public sealed class InMemoryRequestStatisticsStore : IRequestStatisticsStore {
    // The dictionary protects provider lookup; each accumulator protects its own counters.
    private readonly ConcurrentDictionary<string, ApiStatisticsAccumulator> _statistics = new();
    
    public void RecordRequest(string apiName, TimeSpan responseTime) {
        var accumulator = _statistics.GetOrAdd(
            apiName,
            _ => new ApiStatisticsAccumulator());

        accumulator.Record(responseTime);
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

    private sealed class ApiStatisticsAccumulator {
        private readonly object _lock = new();

        private long _totalRequests;
        private double _totalResponseTimeMs;

        private long _fast;
        private long _average;
        private long _slow;

        public void Record(TimeSpan responseTime) {
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
                    Slow = _slow
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
    }
}
