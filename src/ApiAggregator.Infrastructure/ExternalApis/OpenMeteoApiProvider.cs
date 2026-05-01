using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ApiAggregator.Application.Caching;
using ApiAggregator.Application.Dtos;
using ApiAggregator.Application.ExternalApis;

namespace ApiAggregator.Infrastructure.ExternalApis;

public sealed class OpenMeteoApiProvider : IExternalApiProvider
{
    private readonly HttpClient _httpClient;
    private readonly IExternalApiCache _cache;

    public string Name => "Open-Meteo";

    public OpenMeteoApiProvider(
        HttpClient httpClient,
        IExternalApiCache cache) {
        _httpClient = httpClient;
        _cache = cache;

        _httpClient.BaseAddress = new Uri("https://api.open-meteo.com/");
    }

    public async Task<ExternalApiResult> GetItemsAsync(
        AggregationQuery query,
        CancellationToken cancellationToken) {
        const double latitude = 37.9838;
        const double longitude = 23.7275;

        var cacheKey = $"open-meteo:forecast:{latitude}:{longitude}";

        if (_cache.TryGetValue<ExternalApiResult>(cacheKey, out var cachedResult)
            && cachedResult is not null) {
            return cachedResult;
        }

        var url =
            $"v1/forecast?latitude={latitude}&longitude={longitude}&current=temperature_2m,wind_speed_10m&timezone=auto";

        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode) {
            return ExternalApiResult.Failure(
                Name,
                $"{Name} API returned status code {(int)response.StatusCode}.");
        }

        var weatherResponse = await response.Content.ReadFromJsonAsync<OpenMeteoResponse>(
            cancellationToken);

        if (weatherResponse?.Current is null) {
            return ExternalApiResult.Failure(
                Name,
                $"{Name} API returned an empty weather response.");
        }

        var item = new AggregatedItemDto {
            Source = Name,
            Title = "Current weather in Athens",
            Description =
                $"Temperature: {weatherResponse.Current.Temperature2m}°C, " +
                $"Wind speed: {weatherResponse.Current.WindSpeed10m} km/h",
            Category = "weather",
            Url = "https://open-meteo.com/",
            PublishedAt = weatherResponse.Current.Time,
            RelevanceScore = 1
        };

        var result = ExternalApiResult.Success(Name, [item]);

        _cache.Set(
            cacheKey,
            result,
            TimeSpan.FromMinutes(10));

        return result;
    }

    private sealed class OpenMeteoResponse 
    {
        [JsonPropertyName("current")]
        public OpenMeteoCurrentWeather? Current { get; init; }
    }

    private sealed class OpenMeteoCurrentWeather
    {
        [JsonPropertyName("time")]
        public DateTimeOffset Time { get; init; }

        [JsonPropertyName("temperature_2m")]
        public double Temperature2m { get; init; }

        [JsonPropertyName("wind_speed_10m")]
        public double WindSpeed10m { get; init; }
    }
}