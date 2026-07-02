using System.Globalization;
using DashboardTsy.Api.Data;
using DashboardTsy.Api.Services;
using DashboardTsy.Infrastructure.Data;
using DashboardTsy.Infrastructure.Reports;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var cultureInfo = new CultureInfo("tr-TR")
{
    NumberFormat = { NumberDecimalSeparator = ".", NumberGroupSeparator = "," },
    DateTimeFormat = { ShortDatePattern = "dd.MM.yyyy" }
};
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var mainConn = builder.Configuration["DbConnectionStrings:Main"] ?? builder.Configuration.GetConnectionString("Main");
// SSO_USERVIEW için NorthStarMobile DB kullanılacak (Referans fallback kaldırıldı).
var referansConn = builder.Configuration["DbConnectionStrings:NorthStarMobile"]
                   ?? throw new InvalidOperationException("Missing DbConnectionStrings:NorthStarMobile connection string.");

builder.Services.AddDbContext<DashboardTsyDbContext>(o => o.UseSqlServer(mainConn));
builder.Services.AddScoped<IWindowsAuthService, WindowsAuthService>();
builder.Services.AddSingleton(new ReferansDbOptions { ConnectionString = referansConn });

// Stored procedure / DataLayer (DBRapor-style)
builder.Services.AddSingleton<DashboardTsy.Infrastructure.Data.IConnectionStringProvider, ConnectionStringProvider>();
builder.Services.AddScoped<DashboardTsy.Infrastructure.Data.IStoredProcedureExecutor, StoredProcedureExecutor>();
builder.Services.AddScoped<DashboardTsy.Application.IReportDataProvider, ReportDataProvider>();

// ScoreCard proxy: ServiceBus token (singleton cache) + Pupa API client
builder.Services.Configure<PupaApiOptions>(builder.Configuration.GetSection(PupaApiOptions.SectionName));
builder.Services.Configure<ServiceBusOptions>(builder.Configuration.GetSection(ServiceBusOptions.SectionName));

// Named client for ServiceBus token endpoint (no base address; full URL in options)
builder.Services.AddHttpClient("ServiceBusToken");

// Singleton token service: shares one named HttpClient, caches the token in-process
builder.Services.AddSingleton<IScoreCardTokenService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ServiceBusOptions>>();
    return new ScoreCardTokenService(factory.CreateClient("ServiceBusToken"), options);
});

// Named client for Pupa API; injected into ScoreCardProxyController via IHttpClientFactory
var pupaBaseUrl = builder.Configuration[$"{PupaApiOptions.SectionName}:BaseUrl"]?.TrimEnd('/') ?? string.Empty;
builder.Services.AddHttpClient("PupaApi", client =>
{
    client.BaseAddress = new Uri(pupaBaseUrl + "/");
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Avoid schemaId collisions for nested classes like GetDailyTargetReportResponse.Product vs GetMonthlyTargetReportResponse.Product
    c.CustomSchemaIds(t => t.FullName);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
