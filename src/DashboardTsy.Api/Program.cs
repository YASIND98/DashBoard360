using System.Globalization;
using System.IO.Compression;
using DashboardTsy.Api.Data;
using DashboardTsy.Api.Services;
using DashboardTsy.Infrastructure.Data;
using DashboardTsy.Infrastructure.Reports;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Thread pool'un ani yükte gecikmeli genişlemesini engellemek için taban değer.
// Default (~makine core sayısı) rapor endpoint'lerinde SP dönene kadar thread bloke kaldığından hızlı tükenir;
// SetMinThreads ile pool "sıcak" başlar ve ilk 200 eşzamanlı istek büyüme gecikmesi yaşamaz.
ThreadPool.SetMinThreads(200, 200);

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
// Provider'lar stateless: instance-level alan yok, cache'ler static, bağımlı oldukları IStoredProcedureExecutor
// ve IConfiguration da Singleton. Bu nedenle Singleton'a alarak per-request DI resolution ve allocation'ı elde ediyoruz.
builder.Services.AddSingleton<DashboardTsy.Infrastructure.Data.IConnectionStringProvider, ConnectionStringProvider>();
builder.Services.AddSingleton<DashboardTsy.Infrastructure.Data.IStoredProcedureExecutor, StoredProcedureExecutor>();
builder.Services.AddSingleton<DashboardTsy.Application.IReportDataProvider, ReportDataProvider>();

// AppSettings (generic key/value/type feature-flag store)
// AppSettings bilinçli olarak Scoped bırakıldı — Repository/Service'in state/lifetime denetimi bu kapsamda yapılmadı.
builder.Services.AddMemoryCache();
builder.Services.AddScoped<DashboardTsy.Application.AppSettings.IAppSettingsRepository, DashboardTsy.Infrastructure.AppSettings.AppSettingsRepository>();
builder.Services.AddScoped<DashboardTsy.Application.AppSettings.IAppSettingsService, DashboardTsy.Application.AppSettings.AppSettingsService>();

// SalaryCustomerReport — ReportDataProvider ile aynı gerekçe: stateless + Singleton bağımlılıklar.
builder.Services.AddSingleton<DashboardTsy.Application.SalaryCustomerReport.ISalaryCustomerReportProvider, DashboardTsy.Infrastructure.SalaryCustomerReport.SalaryCustomerReportProvider>();

// NplReport — ReportDataProvider ile aynı gerekçe: stateless + Singleton bağımlılıklar.
builder.Services.AddSingleton<DashboardTsy.Application.NplReport.INplReportProvider, DashboardTsy.Infrastructure.NplReport.NplReportProvider>();

// ScoreCard proxy: ServiceBus OAuth token (singleton cache) + Pupa API HttpClient
builder.Services.Configure<PupaApiOptions>(builder.Configuration.GetSection(PupaApiOptions.SectionName));
builder.Services.Configure<ServiceBusOptions>(builder.Configuration.GetSection(ServiceBusOptions.SectionName));

builder.Services.AddHttpClient("ServiceBusToken");
builder.Services.AddSingleton<IScoreCardTokenService>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var options = sp.GetRequiredService<IOptions<ServiceBusOptions>>();
    var logger = sp.GetRequiredService<ILogger<ScoreCardTokenService>>();
    return new ScoreCardTokenService(factory.CreateClient("ServiceBusToken"), options, logger);
});

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

// Response compression: rapor JSON'ları genellikle 10x sıkışır (tekrarlı property isimleri, sayısal veri).
// HTTPS üzerinden default kapalıdır (BREACH/CRIME saldırı riskleri için) — açıkça enable ediyoruz;
// bankada TLS uçtan uca olduğundan risk sınırlı, kazanç net.
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" });
});

// Fastest = daha az CPU + daha büyük payload; rapor JSON'ları için doğru trade-off (CPU zaten boşta beklerken sıkıştırma yapılıyor).
builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Compression, response yazımından önce; controller'a girmeden pipeline'a bağlanmalı.
app.UseResponseCompression();

app.UseAuthorization();

app.MapControllers();

app.Run();
