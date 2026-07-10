using DashboardTsy.Domain.AppSettings;
using Microsoft.Extensions.Caching.Memory;

namespace DashboardTsy.Application.AppSettings;

public sealed class AppSettingsService : IAppSettingsService
{
    private const string CacheKey = "AppSettings.All";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private readonly IAppSettingsRepository _repository;
    private readonly IMemoryCache _cache;

    public AppSettingsService(IAppSettingsRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public IReadOnlyList<AppSettingDto> GetAll() => LoadCached();

    public AppSettingDto? Get(string key)
        => LoadCached().FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));

    public T? GetValue<T>(string key)
    {
        var dto = Get(key);
        if (dto?.Value is null) return default;
        if (dto.Value is T typed) return typed;
        return (T)Convert.ChangeType(dto.Value, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
    }

    private IReadOnlyList<AppSettingDto> LoadCached()
    {
        var cached = _cache.GetOrCreate(CacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            return _repository.GetAll().Select(ToDto).ToList();
        });
        return cached ?? (IReadOnlyList<AppSettingDto>)Array.Empty<AppSettingDto>();
    }

    private static AppSettingDto ToDto(AppSetting setting) => new()
    {
        Key = setting.Key,
        Value = setting.GetTypedValue(),
        Type = setting.Type,
        Description = setting.Description
    };
}
