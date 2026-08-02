using GEC.ApplicationCore.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;

namespace GEC.Infrastructure.Services;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

        _cache.TryGetValue(key, out T? value);

        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? ttl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));
        if (value is null) return Task.CompletedTask;

        var options = new MemoryCacheEntryOptions();

        if (ttl.HasValue)
        {
            options.SetAbsoluteExpiration(ttl.Value);
        }

        _cache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

        _cache.Remove(key);
        return Task.CompletedTask;
    }
}