namespace DashboardTsy.Api.Services;

public sealed class PupaApiOptions
{
    public const string SectionName = "PupaApi";
    public string BaseUrl { get; set; } = string.Empty;
}

public sealed class ServiceBusOptions
{
    public const string SectionName = "ServiceBus";
    public string TokenUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
