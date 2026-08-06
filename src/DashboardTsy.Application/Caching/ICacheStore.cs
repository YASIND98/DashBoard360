namespace DashboardTsy.Application.Caching;

/// <summary>
/// Cache port'u. Application katmanı concrete cache implementasyonlarını (IMemoryCache, Redis vb.) tanımaz.
/// Adaptörler Infrastructure'da yaşar.
/// </summary>
public interface ICacheStore
{
    bool TryGet<T>(string key, out T? value);

    void Set<T>(string key, T value, TimeSpan ttl);

    /// <summary>
    /// Belirli bir prefix ile başlayan tüm anahtarları temizler.
    /// Manuel invalidate senaryosunda kullanılır (ör. "Target::" → tüm günlük cache).
    /// </summary>
    void RemoveByPrefix(string prefix);

    /// <summary>
    /// Cache'in tümünü temizler.
    /// </summary>
    void Clear();
}
