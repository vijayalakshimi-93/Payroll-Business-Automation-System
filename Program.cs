using DivineCoreS.HRMS.Data;
using DivineCoreS.HRMS.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using System.Net;
using OfficeOpenXml;


// EPPlus License
ExcelPackage.License.SetNonCommercialPersonal("Divine Core Solutions");

var builder = WebApplication.CreateBuilder(args);

ServicePointManager.SecurityProtocol =
    SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
    });



builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ExcelImportService>();
builder.Services.AddScoped<PdfService>();
//builder.Services.AddScoped<DynamicTableImportService>();
builder.Services.AddScoped<DynamicWorksheetQueryService>();
builder.Services.AddScoped<WorksheetSchemaService>();
builder.Services.AddScoped<SmartExcelImportService>();


var app = builder.Build();

QuestPDF.Settings.License = LicenseType.Community;

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync(); //auto migration of db

    var phone = builder.Configuration["Seed:AdminPhone"] ?? "9999999999";
    await SeedData.InitializeAsync(db, phone);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

await app.RunAsync();
