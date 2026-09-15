using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.SufiCom.EntityFrameworkCore;
using SufiChain.SufiPlatform.Settings.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;

using Volo.Abp.EntityFrameworkCore.Sqlite;
namespace SufiChain.SufiPlatform.SufiCom;

[DependsOn(
    typeof(SufiComTestBaseModule),
    typeof(SufiComApplicationModule),
    typeof(SufiComEntityFrameworkCoreModule),
    typeof(SufiSettingsEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule)
)]
public class SufiComApplicationTestModule : AbpModule
{
    private SqliteConnection? _sqliteConnection;

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Languages.Add(new LanguageInfo("fa", "fa", "فارسی"));
            options.Languages.Add(new LanguageInfo("en", "en", "English"));
        });

        ConfigureInMemorySqlite(context.Services);
    }

    private void ConfigureInMemorySqlite(IServiceCollection services)
    {
        _sqliteConnection = CreateDatabaseAndGetConnection();

        services.Configure<AbpDbContextOptions>(options =>
        {
            options.Configure<SufiComDbContext>(context =>
            {
                context.DbContextOptions.UseSqlite(_sqliteConnection, contextOwnsConnection: false);
            });
            options.Configure<SettingsDbContext>(context =>
            {
                context.DbContextOptions.UseSqlite(_sqliteConnection, contextOwnsConnection: false);
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

        var options = new DbContextOptionsBuilder<SufiComDbContext>()
            .UseSqlite(connection, contextOwnsConnection: false)
            .Options;

        using var context = new SufiComDbContext(options);
        context.GetService<IRelationalDatabaseCreator>().CreateTables();

        var settingsOptions = new DbContextOptionsBuilder<SettingsDbContext>()
            .UseSqlite(connection, contextOwnsConnection: false)
            .Options;

        using var settingsContext = new SettingsDbContext(settingsOptions);
        settingsContext.GetService<IRelationalDatabaseCreator>().CreateTables();

        return connection;
    }
}
