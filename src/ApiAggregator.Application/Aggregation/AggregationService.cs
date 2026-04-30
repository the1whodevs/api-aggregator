using ApiAggregator.Application.Dtos;
using ApiAggregator.Application.ExternalApis;

namespace ApiAggregator.Application.Aggregation;

public sealed class AggregationService : IAggregationService
{
    public Task<AggregatedResponseDto> GetAggregatedDataAsync(AggregationQuery query, CancellationToken cancellationToken) {
        
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