using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SufiChain.SufiAbp.LocalizationManagement.EntityFrameworkCore;

/// <summary>
/// Design-time DbContext factory for EF Core migrations
/// </summary>
public class LocalizationManagementDbContextFactory : IDesignTimeDbContextFactory<LocalizationManagementDbContext>
{
    public LocalizationManagementDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<LocalizationManagementDbContext>()
            .UseSqlServer(configuration.GetConnectionString("LocalizationManagement") ?? "Server=localhost;Database=LocalizationManagement;Trusted_Connection=True;TrustServerCertificate=True;");

        return new LocalizationManagementDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true);

        return builder.Build();
    }
}
