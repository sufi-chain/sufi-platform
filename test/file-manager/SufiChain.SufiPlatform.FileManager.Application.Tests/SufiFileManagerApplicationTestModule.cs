using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.BlobDatabase.EntityFrameworkCore;
using SufiChain.SufiPlatform.FileManager.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.FileManager;

[DependsOn(
    typeof(SufiFileManagerApplicationModule),
    typeof(SufiFileManagerDomainTestModule),
    typeof(SufiFileManagerEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule)
    )]
public class SufiFileManagerApplicationTestModule : AbpModule
{
    private SqliteConnection _connection = null!;

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(configuration =>
                configuration.DbContextOptions.UseSqlite(_connection));
        });
        CreateTables<FileManagerDbContext>();
        CreateTables<SufiBlobDatabaseDbContext>();

        context.Services.AddSingleton(Substitute.For<ILocalizationTextSeeder>());
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        _connection.Dispose();
    }

    private void CreateTables<TContext>()
        where TContext : DbContext
    {
        var options = new DbContextOptionsBuilder<TContext>()
            .UseSqlite(_connection)
            .Options;
        using var dbContext = (TContext)Activator.CreateInstance(typeof(TContext), options)!;
        dbContext.GetService<IRelationalDatabaseCreator>().CreateTables();
    }
}
