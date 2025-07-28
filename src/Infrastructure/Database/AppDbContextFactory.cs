using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
/*
## Proper syntax for creating a new migration:
> From Root of the solution
```bash
dotnet ef migrations add Init \
  --project src/Infrastructure \
  --startup-project src/Api
```
> Apply DB Update (From solution root)
```bash
dotnet ef database update --project src/Infrastructure/ --startup-project src/Api/
```
*/
namespace Infrastructure.Database;

/// <summary>
/// A factory for creating `AppDbContext` instances, specifically for Entity Framework Core's design-time tools
/// (like `dotnet ef migrations add`). This factory is necessary because the design-time tools run outside
/// the context of a running application and cannot use the standard dependency injection setup defined in `Program.cs`.
/// It manually constructs the configuration and options needed to instantiate the DbContext.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <summary>
    /// Creates a new instance of the `AppDbContext`. This method is called by the EF Core tools
    /// when a migration or other design-time operation is being performed.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the tool, which are not used in this implementation.</param>
    /// <returns>A new instance of <see cref="AppDbContext"/> configured for the design-time tools.</returns>
    public AppDbContext CreateDbContext(string[] args)
    {
        // Determine the current environment (e.g., "Development", "Staging") to load the correct configuration file.
        // This allows you to use different connection strings for development and production.
        // If the `ASPNETCORE_ENVIRONMENT` variable is not set, it safely defaults to "Production".
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        // Manually build the configuration object to read settings from JSON files,
        // mimicking how the application host would do it at runtime.
        var config = new ConfigurationBuilder()
            // Set the base path to the directory where the application's executable is located.
            // AppContext.BaseDirectory is a reliable way to get this path, even when run from different working directories.
            .SetBasePath(AppContext.BaseDirectory)

            // Load the base appsettings.json file. This file should contain common settings.
            // `optional: false` means the build will fail if this file is not found, which is desired.
            .AddJsonFile("appsettings.json", optional: false)

            // Load the environment-specific appsettings file (e.g., appsettings.Development.json).
            // This file can override settings from the base file.
            // `optional: true` means it's okay if this file doesn't exist (e.g., for Production environment).
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .Build();

        // Retrieve the database connection string from the built configuration.
        // The key 'localdb' must match the name in the 'ConnectionStrings' section of appsettings.json.
        var connectionString = config.GetConnectionString("localdb");

        // Create the DbContextOptions required by the AppDbContext constructor.
        var options = new DbContextOptionsBuilder<AppDbContext>()
            // Configure the context to use SQL Server with the retrieved connection string.
            .UseSqlServer(connectionString)
            // Apply the snake case naming convention for all table and column names.
            .UseSnakeCaseNamingConvention()
            // Finalize the configuration and retrieve the options object.
            .Options;

        // Instantiate and return the AppDbContext with the fully configured options.
        return new AppDbContext(options);
    }
}