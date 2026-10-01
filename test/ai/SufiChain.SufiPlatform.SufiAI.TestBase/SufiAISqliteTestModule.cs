using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.SufiAI;

[DependsOn(
    typeof(SufiAITestBaseModule),
    typeof(SufiAIEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule))]
public class SufiAISqliteTestModule : AbpModule
{
    private SqliteConnection? _sqliteConnection;

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        _sqliteConnection = CreateDatabaseAndGetConnection();
        context.Services.Configure<AbpDbContextOptions>(options =>
        {
            options.Configure<AIDbContext>(configuration =>
            {
                configuration.DbContextOptions.UseSqlite(_sqliteConnection);
            });
        });
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        _sqliteConnection?.Dispose();
    }

    private static SqliteConnection CreateDatabaseAndGetConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AIDbContext>()
            .UseSqlite(connection)
            .Options;

        using var dbContext = new AIDbContext(options);
        dbContext.GetService<IRelationalDatabaseCreator>().CreateTables();
        return connection;
    }
}
