using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ApiAggregator.Application.Caching;
using ApiAggregator.Application.Dtos;
using ApiAggregator.Application.ExternalApis;

namespace ApiAggregator.Infrastructure.ExternalApis;

public sealed class HackerNewsApiProvider : IExternalApiProvider
{
    private readonly HttpClient _httpClient;
    private readonly IExternalApiCache _cache;

    public string Name => "Hacker News";

    public HackerNewsApiProvider(
        HttpClient httpClient,
        IExternalApiCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;

        _httpClient.BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/");
    }

    public async Task<ExternalApiResult> GetItemsAsync(
        AggregationQuery query,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"hacker-news:top-stories:{query.Query}:{query.Category}";

        if (_cache.TryGetValue<ExternalApiResult>(cacheKey, out var cachedResult)
            && cachedResult is not null)
        {
            return cachedResult;
        }

        var storyIds = await _httpClient.GetFromJsonAsync<List<int>>(
            "topstories.json",
            cancellationToken);

        if (storyIds is null || storyIds.Count == 0)
        {
            return ExternalApiResult.Failure(
                Name,
                $"{Name} API returned no story ids.");
        }

        var selectedStoryIds = storyIds.Take(10).ToList();

        var storyTasks = selectedStoryIds.Select(storyId =>
            _httpClient.GetFromJsonAsync<HackerNewsItemDto>(
                $"item/{storyId}.json",
                cancellationToken));

        var stories = await Task.WhenAll(storyTasks);

        var items = stories
            .Where(story => story is not null)
            .Select(story => story!)
            .Where(story => !string.IsNullOrWhiteSpace(story.Title))
            .Select(story => new AggregatedItemDto
            {
                Source = Name,
                Title = story.Title!,
                Description = $"Score: {story.Score}, comments: {story.Descendants}",
                Category = "technology",
                Url = !string.IsNullOrWhiteSpace(story.Url)
                    ? story.Url
                    : $"https://news.ycombinator.com/item?id={story.Id}",
                PublishedAt = DateTimeOffset.FromUnixTimeSeconds(story.Time),
                RelevanceScore = story.Score
            })
            .ToList();

        var result = ExternalApiResult.Success(Name, items);

        _cache.Set(
            cacheKey,
            result,
            TimeSpan.FromMinutes(5));

        return result;
    }

    private sealed class HackerNewsItemDto
    {
        [JsonPropertyName("id")]
        public int Id { get; init; }

        [JsonPropertyName("title")]
        public string? Title { get; init; }

        [JsonPropertyName("url")]
        public string? Url { get; init; }

        [JsonPropertyName("score")]
        public int Score { get; init; }

        [JsonPropertyName("descendants")]
        public int Descendants { get; init; }

        [JsonPropertyName("time")]
        public long Time { get; init; }
    }
}