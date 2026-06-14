using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.AIManagement.Application;
using SufiChain.SufiAbp.AIManagement.EntityFrameworkCore;
using SufiChain.SufiAbp.EntityFrameworkCore.Sqlite;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AIManagement;

[DependsOn(
    typeof(AIManagementTestBaseModule),
    typeof(SufiAbpAIManagementApplicationModule),
    typeof(SufiAbpAIManagementEntityFrameworkCoreModule),
    typeof(SufiAbpEntityFrameworkCoreSqliteModule)
)]
public class AIManagementApplicationTestModule : AbpModule
{
    private SqliteConnection? _sqliteConnection;

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClient();
        ConfigureInMemorySqlite(context.Services);
    }

    private void ConfigureInMemorySqlite(IServiceCollection services)
    {
        _sqliteConnection = CreateDatabaseAndGetConnection();

        services.Configure<AbpDbContextOptions>(options =>
        {
            options.Configure<AIManagementDbContext>(c =>
            {
                c.DbContextOptions.UseSqlite(_sqliteConnection);
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

        var options = new DbContextOptionsBuilder<AIManagementDbContext>()
            .UseSqlite(connection)
            .Options;

        using var context = new AIManagementDbContext(options);
        context.GetService<IRelationalDatabaseCreator>().CreateTables();

        return connection;
    }
}
