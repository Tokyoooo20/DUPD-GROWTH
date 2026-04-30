using DupdGrowth.Web.Data;
using DupdGrowth.Web.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<UserTableColumnOptions>(
    builder.Configuration.GetSection(UserTableColumnOptions.SectionName));

var connectionString = DbConfig.GetConnectionString(builder.Configuration);
var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "DupdGrowth.Auth";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.LoginPath = "/Pages/Login";
    });

// Add services to the container.
// Runtime compilation loads .cshtml from the project/content root so Login and other views
// resolve even when a stale precompiled assembly is still running (e.g. locked DLL on rebuild).
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

var app = builder.Build();

// EF migration history may not match this DB; ensure approval column exists for self-service signup.
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userCols = scope.ServiceProvider.GetRequiredService<IOptions<UserTableColumnOptions>>().Value;
    var table = userCols.Table.Trim().Replace("`", string.Empty);
    var col = userCols.IsApproved.Trim().Replace("`", string.Empty);
    if (table.Length > 0 && col.Length > 0
        && table.All(c => char.IsAsciiLetterOrDigit(c) || c == '_')
        && col.All(c => char.IsAsciiLetterOrDigit(c) || c == '_'))
    {
        try
        {
#pragma warning disable EF1002 // Identifiers come from config, validated above (not user input).
            await db.Database.ExecuteSqlRawAsync(
                $"ALTER TABLE `{table}` ADD COLUMN `{col}` TINYINT(1) NOT NULL DEFAULT 1;");
#pragma warning restore EF1002
        }
        catch (MySqlException ex) when (ex.Number == 1060)
        {
            // Duplicate column name — already present.
        }
    }
}

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        await db.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS `paps` (
                `pap_id` INT NOT NULL AUTO_INCREMENT,
                `priority_no` INT NOT NULL,
                `pap_name` VARCHAR(200) NOT NULL,
                `responsible_person` VARCHAR(200) NOT NULL,
                `budget` DECIMAL(18,2) NOT NULL,
                `time_frame_start` INT NOT NULL,
                `time_frame_end` INT NOT NULL,
                `support_office` VARCHAR(100) NOT NULL,
                `alignment_growth` VARCHAR(100) NULL,
                `alignment_achieve` VARCHAR(200) NULL,
                `remarks_type` VARCHAR(50) NOT NULL,
                `remarks_type_other` VARCHAR(200) NULL,
                `remarks` VARCHAR(2000) NULL,
                `status` VARCHAR(50) NOT NULL,
                `created_by_user_id` INT NOT NULL,
                `created_at` DATETIME(6) NOT NULL,
                PRIMARY KEY (`pap_id`),
                INDEX `IX_paps_created_by_user_id` (`created_by_user_id`)
            ) CHARACTER SET utf8mb4;");
    }
    catch { }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
