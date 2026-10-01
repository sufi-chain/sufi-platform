using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.SufiCom.Channels.Voice.Kavenegar;
using SufiChain.SufiPlatform.SufiCom.EntityFrameworkCore;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.Settings.EntityFrameworkCore;
using SufiChain.SufiPlatform.Tags.EntityFrameworkCore;
using SufiChain.SufiPlatform.UI.Timing;
using Volo.Abp;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;

using Volo.Abp.EntityFrameworkCore.Sqlite;
namespace SufiChain.SufiPlatform.SufiCom;

[DependsOn(
    typeof(SufiComTestBaseModule),
    typeof(SufiComApplicationModule),
    typeof(SufiComEntityFrameworkCoreModule),
    typeof(SufiSettingsEntityFrameworkCoreModule),
    typeof(SufiTagsEntityFrameworkCoreModule),
    typeof(SufiComChannelsVoiceKavenegarModule),
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

        // The UI clock is normally supplied by the Blazor UI module, which this host does not load.
        context.Services.TryAddSingleton<IClock, TestUiClock>();

        // SufiCom, Settings and Tags share one in-memory SQLite connection, which cannot hold
        // more than one transaction at a time or serve background workers concurrently.
        Configure<AbpUnitOfWorkDefaultOptions>(options =>
            options.TransactionBehavior = UnitOfWorkTransactionBehavior.Disabled);
        Configure<AbpBackgroundWorkerOptions>(options => options.IsEnabled = false);
        Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);
        Configure<SettingsOptions>(options =>
        {
            options.SaveStaticSettingsToDatabase = false;
            options.IsDynamicSettingStoreEnabled = false;
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
            options.Configure<TagsDbContext>(context =>
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

        var tagsOptions = new DbContextOptionsBuilder<TagsDbContext>()
            .UseSqlite(connection, contextOwnsConnection: false)
            .Options;

        using var tagsContext = new TagsDbContext(tagsOptions);
        tagsContext.GetService<IRelationalDatabaseCreator>().CreateTables();

        return connection;
    }
}
