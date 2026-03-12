using DashboardTsy.Api.Models;

namespace DashboardTsy.Api.Services;

public interface IWindowsAuthService
{
    Task<ApiResponse<UsersDto>> WindowsLoginAsync(string username, CancellationToken cancellationToken = default);
    Task<ApiResponse<UsersDto>> DomainLoginAsync(string domainName, CancellationToken cancellationToken = default);
    Task<ApiResponse<UsersDto>> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
}
