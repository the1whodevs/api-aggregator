namespace ApiAggregator.Application.Caching;

public interface IExternalApiCache {
    bool TryGetValue<T>(string key, out T? value);

    void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow);
}