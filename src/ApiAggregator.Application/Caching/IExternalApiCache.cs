namespace ApiAggregator.Application.Caching;

/// <summary>
/// Small cache abstraction so provider code does not depend directly on IMemoryCache.
/// </summary>
public interface IExternalApiCache {
    bool TryGetValue<T>(string key, out T? value);

    void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow);
}
