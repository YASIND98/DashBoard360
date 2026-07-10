using DashboardTsy.Domain.AppSettings;

namespace DashboardTsy.Application.AppSettings;

public interface IAppSettingsRepository
{
    IReadOnlyList<AppSetting> GetAll();
}
