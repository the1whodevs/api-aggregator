using ApiAggregator.Application.Statistics;
using Microsoft.Extensions.Options;

namespace ApiAggregator.Api.Performance;

public sealed class PerformanceAnomalyBackgroundService : BackgroundService
{
    private readonly IRequestStatisticsStore _statisticsStore;
    private readonly ILogger<PerformanceAnomalyBackgroundService> _logger;
    private readonly PerformanceAnomalyOptions _options;

    public PerformanceAnomalyBackgroundService(
        IRequestStatisticsStore statisticsStore,
        ILogger<PerformanceAnomalyBackgroundService> logger,
        IOptions<PerformanceAnomalyOptions> options)
    {
        _statisticsStore = statisticsStore;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled) {
            return;
        }

        while (!stoppingToken.IsCancellationRequested) {
            try {
                await Task.Delay(_options.CheckInterval, stoppingToken);
                LogAnomalies();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
                return;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Performance anomaly analysis failed.");
            }
        }
    }

    private void LogAnomalies()
    {
        var analyses = _statisticsStore.GetPerformanceAnalysis(
            _options.RecentWindow,
            DateTimeOffset.UtcNow,
            _options.ThresholdPercentage,
            _options.MinimumRecentSamples);

        foreach (var analysis in analyses.Where(analysis => analysis.IsAnomaly)) {
            _logger.LogWarning(
                "Performance anomaly detected for {ApiName}. Recent average response time over the last {RecentWindowMinutes} minutes is {RecentAverageResponseTimeMs}ms, which is {PercentageIncrease}% higher than the baseline average of {BaselineAverageResponseTimeMs}ms.",
                analysis.ApiName,
                _options.RecentWindowMinutes,
                Math.Round(analysis.RecentAverageResponseTimeMs, 2),
                Math.Round(analysis.PercentageIncrease, 2),
                Math.Round(analysis.TotalAverageResponseTimeMs, 2));
        }
    }
}
