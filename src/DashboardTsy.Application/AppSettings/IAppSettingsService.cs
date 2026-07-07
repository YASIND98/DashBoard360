namespace DashboardTsy.Application.AppSettings;

public interface IAppSettingsService
{
    IReadOnlyList<AppSettingDto> GetAll();
    AppSettingDto? Get(string key);
    T? GetValue<T>(string key);
}
