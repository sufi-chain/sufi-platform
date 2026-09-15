using Microsoft.Extensions.DependencyInjection;
using Mongo2Go;
using MongoDB.Driver;
using SufiChain.SufiPlatform.SufiCom.Channels;
using SufiChain.SufiPlatform.SufiCom.Chat;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Settings;
using Volo.Abp.Data;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.SufiCom.Chat.MongoDB;

public sealed class ChatMongoDbFixture : IDisposable
{
    private readonly MongoDbRunner _runner;

    public string ConnectionString { get; }

    public ChatMongoDbFixture()
    {
        _runner = MongoDbRunner.Start(singleNodeReplSet: true);
        ConnectionString = new MongoUrlBuilder(_runner.ConnectionString)
        {
            DatabaseName = $"ChatTests_{Guid.NewGuid():N}"
        }.ToString();
    }

    public void Dispose()
    {
        // Each integrated test owns its runner, including its temporary data directory.
        _runner.Dispose();
    }
}

[DependsOn(
    typeof(SufiComChatTestBaseModule),
    typeof(SufiComChatApplicationModule),
    typeof(SufiComChatMongoDbModule),
    typeof(SufiComChannelsModule),
    typeof(SufiSettingsDomainModule),
    typeof(SufiFeaturesModule)
)]
public class SufiComChatMongoDbTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register by type so the application's service provider owns disposal.
        context.Services.AddSingleton<ChatMongoDbFixture>();
        context.Services.AddOptions<AbpDbConnectionOptions>()
            .Configure<ChatMongoDbFixture>((options, fixture) =>
            {
                options.ConnectionStrings.Default = fixture.ConnectionString;
                options.ConnectionStrings[ChatDbProperties.ConnectionStringName] = fixture.ConnectionString;
            });

        ChatTestServiceConfiguration.ConfigureTestServices(context);
    }
}
