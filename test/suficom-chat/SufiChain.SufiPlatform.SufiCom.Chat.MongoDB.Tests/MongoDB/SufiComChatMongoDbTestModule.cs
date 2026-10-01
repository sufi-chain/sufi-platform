using Mongo2Go;
using MongoDB.Driver;
using SufiChain.SufiPlatform.SufiCom.Channels;
using SufiChain.SufiPlatform.SufiCom.Channels.Telegram;
using SufiChain.SufiPlatform.SufiCom.Chat;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.Settings.MongoDB;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Data;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.SufiCom.Chat.MongoDB;

/// <summary>
/// One mongod per test process, released on process exit. Owning the runner here rather than in
/// the ABP container keeps it from leaking when module initialization throws in a test constructor.
/// </summary>
public static class ChatMongoDbRunner
{
    private static readonly Lazy<MongoDbRunner> Runner = new(() =>
    {
        var runner = MongoDbRunner.Start(singleNodeReplSet: true);
        AppDomain.CurrentDomain.ProcessExit += (_, _) => runner.Dispose();
        return runner;
    });

    public static string CreateDatabaseConnectionString() =>
        new MongoUrlBuilder(Runner.Value.ConnectionString)
        {
            DatabaseName = $"ChatTests_{Guid.NewGuid():N}"
        }.ToString();
}

[DependsOn(
    typeof(SufiComChatTestBaseModule),
    typeof(SufiComChatApplicationModule),
    typeof(SufiComChatMongoDbModule),
    typeof(SufiComChannelsModule),
    typeof(SufiComChannelsTelegramModule),
    typeof(SufiSettingsMongoDbModule),
    typeof(SufiFeaturesModule)
)]
public class SufiComChatMongoDbTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var connectionString = ChatMongoDbRunner.CreateDatabaseConnectionString();
        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = connectionString;
            options.ConnectionStrings[ChatDbProperties.ConnectionStringName] = connectionString;
        });

        // Each test gets a fresh database, and multi-document transactions on collections
        // created mid-transaction fail with "pending collection catalog changes".
        Configure<AbpUnitOfWorkDefaultOptions>(options =>
            options.TransactionBehavior = UnitOfWorkTransactionBehavior.Disabled);
        Configure<AbpBackgroundWorkerOptions>(options => options.IsEnabled = false);
        Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);
        Configure<SettingsOptions>(options =>
        {
            options.SaveStaticSettingsToDatabase = false;
            options.IsDynamicSettingStoreEnabled = false;
        });

        ChatTestServiceConfiguration.ConfigureTestServices(context);
    }
}
