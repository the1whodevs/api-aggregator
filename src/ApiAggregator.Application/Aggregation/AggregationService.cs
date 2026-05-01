using System.Diagnostics;
using ApiAggregator.Application.Dtos;
using ApiAggregator.Application.ExternalApis;
using ApiAggregator.Application.Statistics;

namespace ApiAggregator.Application.Aggregation;

public sealed class AggregationService : IAggregationService
{
    private readonly IEnumerable<IExternalApiProvider> _providers;
    private readonly IRequestStatisticsStore _statisticsStore;
    
    public AggregationService(IEnumerable<IExternalApiProvider> providers, IRequestStatisticsStore statisticsStore) {
        _providers = providers;
        _statisticsStore = statisticsStore;
    }
    
    /// <summary>
    /// Aggregates data from all external APIs using a single query.
    /// </summary>
    /// <param name="query">The query to use when fetching data.</param>
    /// <param name="cancellationToken">Token used in case cancellation takes place.</param>
    /// <returns>Task to await with the custom AggregatedResponseDto, containing the filtered,
    /// and sorted items result list.</returns>
    public async Task<AggregatedResponseDto> GetAggregatedDataAsync(AggregationQuery query, CancellationToken cancellationToken) {
        // Start every provider call before awaiting so slow APIs do not block fast ones.
        var providerTasks = _providers.Select(provider => ExecuteProviderAsync(provider, query, cancellationToken));
        
        var providerResults = await Task.WhenAll(providerTasks);
        
        var items = providerResults.SelectMany(
                result => result.Items).ToList();

        items = ApplyFiltering(items, query);
        items = ApplySorting(items, query);
 
        // Get the warnings...
        var warnings = providerResults
            .Where(result => !string.IsNullOrWhiteSpace(result.Warning))
            .Select(result => result.Warning!)
            .ToList();

        return new AggregatedResponseDto() {
            Items = items,
            Warnings = warnings
        };
    }

    private async Task<ExternalApiResult> ExecuteProviderAsync(
        IExternalApiProvider provider, 
        AggregationQuery query,
        CancellationToken cancellationToken) {
        
        var timer = Stopwatch.StartNew();

        try {
            return await provider.GetItemsAsync(query, cancellationToken);
        }
        catch (OperationCanceledException) {
            // Request cancellation should remain visible to ASP.NET Core instead of
            // being converted into a partial-result warning.
            throw;
        }
        catch (Exception) {
            return ExternalApiResult.Failure(
                provider.Name,
                $"{provider.Name} API failed. Partial aggregated data returned.");
        }
        finally {
            // Track timing for both successful and failed provider calls.
            timer.Stop();
            _statisticsStore.RecordRequest(provider.Name, timer.Elapsed);
        }
    }

    /// <summary>
    /// Filters items based on Category and Query.
    /// </summary>
    /// <param name="items">The items to filter.</param>
    /// <param name="query">The query to filter with.</param>
    /// <returns>The filtered item list.</returns>
    public static List<AggregatedItemDto> ApplyFiltering(IEnumerable<AggregatedItemDto> items, AggregationQuery query) {
        if (!string.IsNullOrWhiteSpace(query.Category)) {
            items = items.Where(item => string.Equals(
                item.Category,
                query.Category,
                StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query.Query)) {
            items = items.Where(item => 
                item.Title.Contains(query.Query, StringComparison.OrdinalIgnoreCase) ||
                (item.Description?.Contains(query.Query, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return items.ToList();
    }

    /// <summary>
    /// Sorts items based on query.SortBy and query.SortDirection.
    /// </summary>
    /// <param name="items">The items to sort.</param>
    /// <param name="query">The query to sort with.</param>
    /// <returns>The sorted item list.</returns>
    public static List<AggregatedItemDto> ApplySorting(IEnumerable<AggregatedItemDto> items, AggregationQuery query) {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return query.SortBy.ToLowerInvariant() switch {
            "title" => descending ?
                items.OrderByDescending(item => item.Title).ToList() : items.OrderBy(item => item.Title).ToList(),
            
            "relevance" => descending ?
                items.OrderByDescending(item => item.RelevanceScore).ToList() : items.OrderBy(item => item.RelevanceScore).ToList(),
            
            "date" => descending ?
                items.OrderByDescending(item => item.PublishedAt).ToList() : items.OrderBy(item => item.PublishedAt).ToList(),

            _ => descending ?
                items.OrderByDescending(item => item.PublishedAt).ToList() : items.OrderBy(item => item.PublishedAt).ToList()
        };
    }
}
