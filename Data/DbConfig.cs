using MySqlConnector;

namespace DupdGrowth.Web.Data;

public static class DbConfig
{
    public const string ConnectionName = "DefaultConnection";

    public static string GetConnectionString(IConfiguration configuration)
    {
        var cs = configuration.GetConnectionString(ConnectionName);
        if (string.IsNullOrWhiteSpace(cs))
        {
            throw new InvalidOperationException(
                $"Set ConnectionStrings:{ConnectionName} in appsettings, user secrets, or environment variables.");
        }

        try
        {
            var builder = new MySqlConnectionStringBuilder(cs);

            // Lets you set the password without putting special characters (;, =, etc.) in appsettings.json:
            //   PowerShell: $env:MYSQL_PASSWORD = 'your-password'
            //   CMD:        set MYSQL_PASSWORD=your-password
            var envPassword = Environment.GetEnvironmentVariable("MYSQL_PASSWORD");
            if (!string.IsNullOrEmpty(envPassword))
                builder.Password = envPassword;

            if (string.IsNullOrWhiteSpace(builder.Database))
            {
                throw new InvalidOperationException(
                    "Connection string is missing a database name. Add e.g. database=dupd_growth; (check user secrets and " +
                    "environment variables — ConnectionStrings__DefaultConnection overrides appsettings.json).");
            }

            return builder.ConnectionString;
        }
        catch (ArgumentException ex)
        {
            throw new InvalidOperationException(
                "Connection string could not be parsed. Fix ConnectionStrings:DefaultConnection.", ex);
        }
    }
}
