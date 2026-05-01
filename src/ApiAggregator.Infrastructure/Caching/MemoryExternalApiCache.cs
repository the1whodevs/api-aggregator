using ApiAggregator.Application.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace ApiAggregator.Infrastructure.Caching;

public sealed class MemoryExternalApiCache : IExternalApiCache {
    private readonly IMemoryCache _memoryCache;

    public MemoryExternalApiCache(IMemoryCache memoryCache) {
        _memoryCache = memoryCache;
    }
    
    public bool TryGetValue<T>(string key, out T? value) {
        return _memoryCache.TryGetValue(key, out value);
    }
    
    public void Set<T>(string key, T value, TimeSpan absoluteExpirationRelativeToNow) {
        _memoryCache.Set(key, value, absoluteExpirationRelativeToNow);
    }
}