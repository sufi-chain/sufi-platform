using Mongo2Go;
using SufiChain.Chat;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.SettingManagement;
using Volo.Abp.Data;
using Volo.Abp.Modularity;

namespace SufiChain.Chat.MongoDB;

public static class ChatMongoDbFixture
{
    private static readonly Lazy<MongoDbRunner> Runner = new(() => MongoDbRunner.Start(singleNodeReplSet: true));

    public static string ConnectionString => Runner.Value.ConnectionString;
}

[DependsOn(
    typeof(ChatTestBaseModule),
    typeof(ChatApplicationModule),
    typeof(ChatMongoDbModule),
    typeof(ChatConnectorModule),
    typeof(SufiAbpSettingManagementDomainModule),
    typeof(SufiAbpFeaturesModule)
)]
public class ChatMongoDbTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = ChatMongoDbFixture.ConnectionString;
        });

        ChatTestServiceConfiguration.ConfigureTestServices(context);
    }
}
