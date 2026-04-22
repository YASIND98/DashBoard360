using System.Globalization;
using Microsoft.AspNetCore.Authentication.Negotiate;
using DashboardTsy.Web.Services;

var builder = WebApplication.CreateBuilder(args);

var cultureInfo = new CultureInfo("tr-TR")
{
    NumberFormat =
    {
        NumberDecimalSeparator = ".",
        NumberGroupSeparator = ","
    },
    DateTimeFormat = { ShortDatePattern = "dd.MM.yyyy" }
};
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Add services to the container.
// Session storage:
// - Memory cache is instance-local (OK for single server).
// - In IIS recycle / multi-instance / web garden setups it causes intermittent "lost session" -> 401.
// Prefer Redis/SQL Server if configured; fallback to memory otherwise.
var redisConnectionString =
    builder.Configuration.GetConnectionString("Redis")
    ?? builder.Configuration["Redis:ConnectionString"];

var sessionSqlConnectionString =
    builder.Configuration.GetConnectionString("SessionDb")
    ?? builder.Configuration["SessionDb:ConnectionString"];

if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
    });
}
else if (!string.IsNullOrWhiteSpace(sessionSqlConnectionString))
{
    builder.Services.AddDistributedSqlServerCache(options =>
    {
        options.ConnectionString = sessionSqlConnectionString;
        options.SchemaName = builder.Configuration["SessionDb:SchemaName"] ?? "dbo";
        options.TableName = builder.Configuration["SessionDb:TableName"] ?? "SessionCache";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(6);
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Windows Authentication (Negotiate works for Kestrel / IIS)
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<DashboardApiOptions>(builder.Configuration.GetSection(DashboardApiOptions.SectionName));
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<ITargetReportApiClient, TargetReportApiClient>();
builder.Services.AddHttpClient<IProductivityReportApiClient, ProductivityReportApiClient>();
builder.Services.AddControllersWithViews()
    .AddJsonOptions(o =>
    {
        // Login page JS expects "Result"/"Message" (PascalCase), not "result"/"message".
        o.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Development'ta HTTP ile çalıştırıyorsan (örn. http profile) HTTPS yönlendirme kapalı; sertifika hatası olmaz.
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllers();
app.MapControllerRoute(
    name: "login",
    pattern: "login",
    defaults: new { controller = "Auth", action = "Login" });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
