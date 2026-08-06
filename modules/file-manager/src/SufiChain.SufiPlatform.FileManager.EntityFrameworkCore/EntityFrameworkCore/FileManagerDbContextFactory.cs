using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SufiChain.SufiPlatform.FileManager.EntityFrameworkCore;

/// <summary>
/// Design-time DbContext factory for EF Core migrations
/// </summary>
public class FileManagerDbContextFactory : IDesignTimeDbContextFactory<FileManagerDbContext>
{
    public FileManagerDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        var builder = new DbContextOptionsBuilder<FileManagerDbContext>()
            .UseSqlServer(configuration.GetConnectionString("FileManager") ?? "Server=localhost;Database=FileManager;Trusted_Connection=True;TrustServerCertificate=True;");

        return new FileManagerDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true);

        return builder.Build();
    }
}

