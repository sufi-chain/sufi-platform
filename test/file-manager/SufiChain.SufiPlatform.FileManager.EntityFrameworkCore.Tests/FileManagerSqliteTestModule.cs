using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using SufiChain.SufiPlatform.BlobDatabase.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.FileManager.EntityFrameworkCore;

[DependsOn(
    typeof(SufiFileManagerTestBaseModule),
    typeof(SufiFileManagerEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule))]
public class FileManagerSqliteTestModule : AbpModule
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
