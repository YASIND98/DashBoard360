namespace DashboardTsy.Application.Caching;

/// <summary>
/// Cache key üretici. Metot adını verilen prefix'in altına yerleştirir; arguments deterministik olarak serialize edilir.
/// SessionId gibi kullanıcı-bazlı alanlar adaptörün elediği alanlar arasında olmalıdır — böylece aynı input
/// farklı oturumlarda tek bir cache girdisine düşer.
/// </summary>
public interface ICacheKeyBuilder
{
    /// <param name="prefix">Kategori prefix'i (ör. "Target", "Productivity"). RemoveByPrefix için de referans olur.</param>
    /// <param name="methodName">Cache'lenen metot adı — key'in okunabilirliği ve invalidation için.</param>
    /// <param name="arguments">Metoda geçirilen argümanlar; null olabilir.</param>
    string Build(string prefix, string methodName, object? arguments);
}
