using ApiAggregator.Application.Dtos;
using ApiAggregator.Application.ExternalApis;

namespace ApiAggregator.Application.Aggregation;

public sealed class AggregationService : IAggregationService
{
    private readonly IEnumerable<IExternalApiProvider> _providers;
    
    public AggregationService(IEnumerable<IExternalApiProvider> providers) {
        _providers = providers;
    }
    
    /// <summary>
    /// Aggregates data from all external APIs using a single query.
    /// </summary>
    /// <param name="query">The query to use when fetching data.</param>
    /// <param name="cancellationToken">Token used in case cancellation takes place.</param>
    /// <returns>Task to await with the custom AggregatedResponsDto, containing the filtered,
    /// and sorted items result list.</returns>
    public async Task<AggregatedResponseDto> GetAggregatedDataAsync(AggregationQuery query, CancellationToken cancellationToken) {
        // Get tasks from all providers...
        var providerTasks = _providers.Select(provider => provider.GetItemsAsync(query, cancellationToken));

        // Wait for the results from all providers...
        var providerResults = await Task.WhenAll(providerTasks);
        
        // Get the items list, filter it and sort it...
        var items = providerResults.SelectMany(
                result => result.Items).ToList();

        items = ApplyFiltering(items, query);
        items = ApplySorting(items, query);
 
        // Get the warnings...
        var warnings = providerResults
            .Where(result => !string.IsNullOrWhiteSpace(result.Warning))
            .Select(result => result.Warning!)
            .ToList();

        // Create and return the AggregatedResponseDto!
        return new AggregatedResponseDto() {
            Items = items,
            Warnings = warnings
        };
    }

    /// <summary>
    /// Filters items based on Category and Query.
    /// </summary>
    /// <param name="items">The items to filter.</param>
    /// <param name="query">The query to filter with.</param>
    /// <returns>The filtered item list.</returns>
    public static List<AggregatedItemDto> ApplyFiltering(IEnumerable<AggregatedItemDto> items, AggregationQuery query) {
        // If query.Category is valid...
        if (!string.IsNullOrWhiteSpace(query.Category)) {
            // ...filter items where category matches query.Category
            items = items.Where(item => string.Equals(
                item.Category,
                query.Category,
                StringComparison.OrdinalIgnoreCase));
        }

        // AND if query.Query is valid...
        if (!string.IsNullOrWhiteSpace(query.Query)) {
            // ...filter items where title or description contains query.Query
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
                items.OrderByDescending(item => item.PublishedAt).ToList() : items.OrderBy(item => item.PublishedAt).ToList()
        };
    }
}