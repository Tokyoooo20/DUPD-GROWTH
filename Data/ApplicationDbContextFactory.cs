using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace DupdGrowth.Web.Data;

/// <summary>
/// Ensures <c>dotnet ef</c> loads the same configuration as the app (appsettings + user secrets),
/// using the project folder even when the working directory differs.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var projectRoot = ResolveProjectRoot();
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var config = new ConfigurationBuilder()
            .SetBasePath(projectRoot)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .AddUserSecrets(typeof(ApplicationDbContextFactory).Assembly, optional: true)
            .Build();

        var connectionString = DbConfig.GetConnectionString(config);
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

        var userCols = config.GetSection(UserTableColumnOptions.SectionName)
                .Get<UserTableColumnOptions>()
            ?? new UserTableColumnOptions();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseMySql(connectionString, serverVersion);

        return new ApplicationDbContext(optionsBuilder.Options, Options.Create(userCols));
    }

    private static string ResolveProjectRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        for (var depth = 0; depth < 8 && dir != null; depth++)
        {
            if (File.Exists(Path.Combine(dir.FullName, "DupdGrowth.Web.csproj")))
                return dir.FullName;
            dir = dir.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
