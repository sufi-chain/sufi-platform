using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.SufiCom.Channels;
using SufiChain.SufiPlatform.SufiCom.Channels.Telegram;
using SufiChain.SufiPlatform.SufiCom.Chat.EntityFrameworkCore;
using SufiChain.SufiPlatform.Settings.EntityFrameworkCore;
using SufiChain.SufiPlatform.Features;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

using Volo.Abp.EntityFrameworkCore.Sqlite;
namespace SufiChain.SufiPlatform.SufiCom.Chat;

[DependsOn(
    typeof(SufiComChatTestBaseModule),
    typeof(SufiComChatApplicationModule),
    typeof(SufiComChatEntityFrameworkCoreModule),
    typeof(SufiComChannelsModule),
    typeof(SufiComChannelsTelegramModule),
    typeof(AbpEntityFrameworkCoreSqliteModule),
    typeof(SufiSettingsEntityFrameworkCoreModule),
    typeof(SufiFeaturesModule)
)]
public class SufiComChatApplicationTestModule : AbpModule
{
    private SqliteConnection? _sqliteConnection;

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        _sqliteConnection = CreateDatabaseAndGetConnection();

        context.Services.Configure<AbpDbContextOptions>(options =>
        {
            options.Configure<SufiComChatDbContext>(configuration =>
            {
                configuration.DbContextOptions.UseSqlite(_sqliteConnection, contextOwnsConnection: false);
            });
            options.Configure<SettingsDbContext>(configuration =>
            {
                configuration.DbContextOptions.UseSqlite(_sqliteConnection, contextOwnsConnection: false);
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

        var options = new DbContextOptionsBuilder<SufiComChatDbContext>()
            .UseSqlite(connection, contextOwnsConnection: false)
            .Options;

        using var dbContext = new SufiComChatDbContext(options);
        dbContext.GetService<IRelationalDatabaseCreator>().CreateTables();

        var settingsOptions = new DbContextOptionsBuilder<SettingsDbContext>()
            .UseSqlite(connection, contextOwnsConnection: false)
            .Options;

        using var settingsContext = new SettingsDbContext(settingsOptions);
        settingsContext.GetService<IRelationalDatabaseCreator>().CreateTables();

        return connection;
    }
}
