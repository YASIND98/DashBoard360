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
var referansConn = builder.Configuration["DbConnectionStrings:Referans"] ?? builder.Configuration.GetConnectionString("Referans");

builder.Services.AddDbContext<DashboardTsyDbContext>(o => o.UseSqlServer(mainConn));
builder.Services.AddScoped<IWindowsAuthService, WindowsAuthService>();
builder.Services.AddSingleton(new ReferansDbOptions { ConnectionString = referansConn });

// Stored procedure / DataLayer (DBRapor-style)
builder.Services.AddSingleton<DashboardTsy.Infrastructure.Data.IConnectionStringProvider, ConnectionStringProvider>();
builder.Services.AddScoped<DashboardTsy.Infrastructure.Data.IStoredProcedureExecutor, StoredProcedureExecutor>();
builder.Services.AddScoped<DashboardTsy.Application.IReportDataProvider, ReportDataProvider>();

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
