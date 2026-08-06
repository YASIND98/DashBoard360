using DashboardTsy.Application.Caching;
using DashboardTsy.Infrastructure.Caching;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Api.Controllers;

/// <summary>
/// Rapor cache'inin manuel yönetimi. TTL bazlı otomatik eviction yeterli olmadığında
/// (ör. SP'de veri düzeltilir düzeltilmez ekranda yansısın istendiğinde) burası kullanılır.
/// Erişim mevcut Windows Auth ile korunur — API seviyesinde ek yetki kontrolü yok.
/// </summary>
[ApiController]
[Route("cache")]
public class CacheAdminController : ControllerBase
{
    private readonly ICacheStore _cache;

    public CacheAdminController(ICacheStore cache)
    {
        _cache = cache;
    }

    /// <summary>
    /// Belirli bir kategoriyi ya da tüm cache'i temizler.
    /// </summary>
    /// <param name="scope">
    /// "Target" → sadece 24 saatlik günlük rapor cache'i,
    /// "Productivity" → sadece 30 günlük verimlilik cache'i,
    /// "all" (veya boş) → tümü.
    /// </param>
    [HttpPost("invalidate")]
    public IActionResult Invalidate([FromQuery] string? scope = null)
    {
        if (string.IsNullOrWhiteSpace(scope) || scope.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            _cache.Clear();
            return Ok(new { cleared = "all" });
        }

        var normalized = scope.Trim();
        var prefix = normalized switch
        {
            var s when s.Equals(CachingReportDataProvider.TargetPrefix, StringComparison.OrdinalIgnoreCase)
                => CachingReportDataProvider.TargetPrefix,
            var s when s.Equals(CachingReportDataProvider.ProductivityPrefix, StringComparison.OrdinalIgnoreCase)
                => CachingReportDataProvider.ProductivityPrefix,
            _ => null
        };

        if (prefix is null)
            return BadRequest(new { error = $"Unknown scope '{scope}'. Valid: Target, Productivity, all." });

        _cache.RemoveByPrefix(prefix + "::");
        return Ok(new { cleared = prefix });
    }
}
