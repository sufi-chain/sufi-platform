using Microsoft.Extensions.DependencyInjection;
using Mongo2Go;
using MongoDB.Driver;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.MongoDB;
using Volo.Abp.Data;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;

public sealed class KnowledgeBaseMongoFixture : IDisposable
{
    private readonly MongoDbRunner _runner = MongoDbRunner.Start();

    public string ConnectionString => new MongoUrlBuilder(_runner.ConnectionString)
    {
        DatabaseName = "KnowledgeBaseArticleTests"
    }.ToString();

    public void Dispose() => _runner.Dispose();
}

[DependsOn(typeof(HelpDeskKnowledgeBaseTestModule), typeof(HelpDeskKnowledgeBaseMongoDbModule))]
public class KnowledgeBaseMongoTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<KnowledgeBaseMongoFixture>();
        context.Services.AddOptions<AbpDbConnectionOptions>().Configure<KnowledgeBaseMongoFixture>((options, fixture) =>
        {
            options.ConnectionStrings.Default = fixture.ConnectionString;
            options.ConnectionStrings[HelpDeskKnowledgeBaseDbProperties.ConnectionStringName] = fixture.ConnectionString;
        });
    }
}
