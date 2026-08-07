using System.Globalization;
using System.IO.Compression;
using DashboardTsy.Api.Data;
using DashboardTsy.Api.Middleware;
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

// Rapor cache altyapısı: port'lar Application, adaptörler Infrastructure.
builder.Services.AddSingleton<DashboardTsy.Application.Caching.ICacheStore,
    DashboardTsy.Infrastructure.Caching.MemoryCacheStore>();
builder.Services.AddSingleton<DashboardTsy.Application.Caching.ICacheKeyBuilder,
    DashboardTsy.Infrastructure.Caching.JsonCacheKeyBuilder>();

// IReportDataProvider decorator zinciri:
//   Controller -> CachingReportDataProvider -> ReportDataProvider (inner) -> StoredProcedureExecutor
// Concrete ReportDataProvider'ı ayrıca kayıt ediyoruz ki decorator kendi bağımlısı olarak alabilsin.
builder.Services.AddSingleton<ReportDataProvider>();
builder.Services.AddSingleton<DashboardTsy.Application.IReportDataProvider>(sp =>
    new DashboardTsy.Infrastructure.Caching.CachingReportDataProvider(
        inner: sp.GetRequiredService<ReportDataProvider>(),
        cache: sp.GetRequiredService<DashboardTsy.Application.Caching.ICacheStore>(),
        keyBuilder: sp.GetRequiredService<DashboardTsy.Application.Caching.ICacheKeyBuilder>(),
        configuration: sp.GetRequiredService<IConfiguration>()));

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

// Compression, mobile envelope'dan ÖNCE — response akışında ters yönde çalışır:
// Controller ham JSON yazar → MobileEnvelope sarar → Compression sıkıştırır.
// Compression'ı sonra çağırırsak, mobile envelope zaten sıkıştırılmış byte'ları JSON zannedip parse etmeye çalışır (0x1F 0x8B gzip hatası).
app.UseResponseCompression();

// Mobile envelope: /mobile/* isteklerini yakalayıp response'u { status, message, data } zarfına sarar.
// KRİTİK: UseRouting'den ÖNCE çalışmalı. Aksi halde endpoint routing çoktan karar vermiş olur ve
// path rewrite yeni bir route match tetiklemez — controller'a hiç gitmez, 404 döner.
app.UseMiddleware<MobileEnvelopeMiddleware>();

// Endpoint routing'i explicit çağırıyoruz ki middleware sırası deterministik olsun.
// Aksi halde MapControllers implicit olarak pipeline sonuna UseRouting ekler ve mobile envelope'un
// path rewrite'ı işe yaramaz.
app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
