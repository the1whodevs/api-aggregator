using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ApiAggregator.Application.Caching;
using ApiAggregator.Application.Dtos;
using ApiAggregator.Application.ExternalApis;

namespace ApiAggregator.Infrastructure.ExternalApis;

public sealed class GitHubApiProvider : IExternalApiProvider {
    public string Name => "Github";

    private readonly HttpClient _httpClient;
    private readonly IExternalApiCache _cache;
    
    public GitHubApiProvider(HttpClient httpClient, IExternalApiCache cache) {
        _httpClient = httpClient;
        _cache = cache;

        _httpClient.BaseAddress = new Uri("https://api.github.com/");
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ApiAggregatorAssignment/1.0");
        _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
    }
    
    public async Task<ExternalApiResult> GetItemsAsync(AggregationQuery query, CancellationToken cancellationToken) {
        var searchTerm = string.IsNullOrWhiteSpace(query.Query)
            ? "csharp"
            : query.Query;
        
        // Used for performance
        var cacheKey = $"github:repositories:{searchTerm}";
        
        // Used in case of API failure
        var fallbackCacheKey = $"github:repositories:{searchTerm}:fallback";
        
        // If there's a performance cache available on this search term, return that instead!
        if (_cache.TryGetValue<ExternalApiResult>(cacheKey, out var cachedResult) &&
            cachedResult is not null) {
            return cachedResult;
        }
        
        var url = $"search/repositories?q={Uri.EscapeDataString(searchTerm)}&sort=stars&order=desc&per_page=5";
        
        try {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode) {
                return TryGetFallbackResult(fallbackCacheKey,
                    $"{Name} API return status code {(int)response.StatusCode}. Returned fallback data if available.");
            }

            var githubResponse = await response.Content.ReadFromJsonAsync<GitHubSearchResponse>(
                cancellationToken);

            var items = githubResponse?.Items?
                .Select(repo => new AggregatedItemDto {
                    Source = Name,
                    Title = repo.FullName ?? repo.Name ?? "Unknown repository",
                    Description = repo.Description,
                    Category = "technology",
                    Url = repo.HtmlUrl,
                    PublishedAt = repo.UpdatedAt,
                    RelevanceScore = repo.StargazersCount
                })
                .ToList() ?? [];

            var result = ExternalApiResult.Success(Name, items);

            // Update performance cache
            _cache.Set(
                cacheKey,
                result,
                TimeSpan.FromMinutes(5));

            // Update fallback cache
            _cache.Set(
                fallbackCacheKey,
                result,
                TimeSpan.FromHours(1));

            return result;
        }
        catch (OperationCanceledException) {
            throw;
        }
        catch (Exception) {
            return TryGetFallbackResult(
                fallbackCacheKey,
                $"{Name} API failed. Returned fallback data if available.");
        }
    }
    
    private ExternalApiResult TryGetFallbackResult( string fallbackCacheKey, string warning) {
        if (_cache.TryGetValue<ExternalApiResult>(fallbackCacheKey, out var fallbackResult)
            && fallbackResult is not null) {
            return ExternalApiResult.SuccessWithWarning(
                Name,
                fallbackResult.Items,
                warning);
        }

        return ExternalApiResult.Failure(
            Name,
            $"{Name} API failed and no fallback data was available.");
    }
    
    private sealed class GitHubSearchResponse
    {
        [JsonPropertyName("items")]
        public List<GitHubRepositoryDto>? Items { get; init; }
    }
    
    private sealed class GitHubRepositoryDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("full_name")]
        public string? FullName { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("html_url")]
        public string? HtmlUrl { get; init; }

        [JsonPropertyName("updated_at")]
        public DateTimeOffset? UpdatedAt { get; init; }

        [JsonPropertyName("stargazers_count")]
        public int StargazersCount { get; init; }
    }
}