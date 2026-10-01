using Mongo2Go;
using MongoDB.Driver;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.MongoDB;
using Volo.Abp.Data;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;

/// <summary>
/// One mongod per test process, released on process exit, so it cannot leak when module
/// initialization throws in a test constructor.
/// </summary>
public static class KnowledgeBaseMongoDbRunner
{
    private static readonly Lazy<MongoDbRunner> Runner = new(() =>
    {
        var runner = MongoDbRunner.Start();
        AppDomain.CurrentDomain.ProcessExit += (_, _) => runner.Dispose();
        return runner;
    });

    public static string CreateDatabaseConnectionString() =>
        new MongoUrlBuilder(Runner.Value.ConnectionString)
        {
            DatabaseName = $"KnowledgeBaseArticleTests_{Guid.NewGuid():N}"
        }.ToString();
}

[DependsOn(typeof(HelpDeskKnowledgeBaseTestModule), typeof(HelpDeskKnowledgeBaseMongoDbModule))]
public class KnowledgeBaseMongoTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var connectionString = KnowledgeBaseMongoDbRunner.CreateDatabaseConnectionString();
        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = connectionString;
            options.ConnectionStrings[HelpDeskKnowledgeBaseDbProperties.ConnectionStringName] = connectionString;
        });
    }
}
