using System.Collections.Concurrent;
using DashboardTsy.Application.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace DashboardTsy.Infrastructure.Caching;

/// <summary>
/// ICacheStore'un IMemoryCache adaptörü.
/// Prefix bazlı invalidation için aktif key'lerin ayrı bir index'ini tutar; TTL sonu / manuel eviction
/// olduğunda PostEvictionCallback ile index'ten temizlenir.
/// </summary>
public sealed class MemoryCacheStore : ICacheStore
{
    private readonly IMemoryCache _cache;

    // Aktif key'lerin ikinci bir listesi. IMemoryCache "tüm key'leri gez" API'sini vermez —
    // RemoveByPrefix / Clear implementasyonu için kendi indeksimizi tutuyoruz.
    private readonly ConcurrentDictionary<string, byte> _keyIndex = new(StringComparer.Ordinal);

    public MemoryCacheStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public bool TryGet<T>(string key, out T? value)
    {
        if (_cache.TryGetValue(key, out var raw) && raw is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }

    public void Set<T>(string key, T value, TimeSpan ttl)
    {
        if (value is null) return;

        var entryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        };

        // Entry düştüğünde (TTL / manuel remove / bellek baskısı) index'i temizle — memory leak yok.
        entryOptions.RegisterPostEvictionCallback((evictedKey, _, _, _) =>
        {
            if (evictedKey is string s)
                _keyIndex.TryRemove(s, out _);
        });

        _cache.Set(key, value, entryOptions);
        _keyIndex[key] = 0;
    }

    public void RemoveByPrefix(string prefix)
    {
        if (string.IsNullOrEmpty(prefix)) return;

        foreach (var key in _keyIndex.Keys)
        {
            if (key.StartsWith(prefix, StringComparison.Ordinal))
                _cache.Remove(key); // Callback index'i güncelleyecek
        }
    }

    public void Clear()
    {
        foreach (var key in _keyIndex.Keys)
            _cache.Remove(key);
    }
}
