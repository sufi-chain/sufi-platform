using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SufiChain.SufiPlatform.Localization.EntityFrameworkCore;

/// <summary>
/// Design-time DbContext factory for EF Core migrations
/// </summary>
public class LocalizationDbContextFactory : IDesignTimeDbContextFactory<LocalizationDbContext>
{
    public LocalizationDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<LocalizationDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Localization") ?? "Server=localhost;Database=Localization;Trusted_Connection=True;TrustServerCertificate=True;");

        return new LocalizationDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true);

        return builder.Build();
    }
}
