using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.Chat.EntityFrameworkCore;
using SufiChain.SufiAbp.EntityFrameworkCore.Sqlite;
using SufiChain.SufiAbp.SettingManagement;
using SufiChain.SufiAbp.Features;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.Chat;

[DependsOn(
    typeof(ChatTestBaseModule),
    typeof(ChatApplicationModule),
    typeof(ChatEntityFrameworkCoreModule),
    typeof(ChatConnectorModule),
    typeof(SufiAbpEntityFrameworkCoreSqliteModule),
    typeof(SufiAbpSettingManagementDomainModule),
    typeof(SufiAbpFeaturesModule)
)]
public class ChatApplicationTestModule : AbpModule
{
    private SqliteConnection? _sqliteConnection;

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        _sqliteConnection = CreateDatabaseAndGetConnection();

        context.Services.Configure<AbpDbContextOptions>(options =>
        {
            options.Configure<ChatDbContext>(configuration =>
            {
                configuration.DbContextOptions.UseSqlite(_sqliteConnection);
            });
        });

        ChatTestServiceConfiguration.ConfigureTestServices(context);
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        _sqliteConnection?.Dispose();
    }

    private static SqliteConnection CreateDatabaseAndGetConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ChatDbContext>()
            .UseSqlite(connection)
            .Options;

        using var dbContext = new ChatDbContext(options);
        dbContext.GetService<IRelationalDatabaseCreator>().CreateTables();

        return connection;
    }
}
