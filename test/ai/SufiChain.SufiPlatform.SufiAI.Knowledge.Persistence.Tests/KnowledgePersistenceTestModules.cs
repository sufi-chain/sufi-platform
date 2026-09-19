using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mongo2Go;
using MongoDB.Driver;
using SufiChain.SufiPlatform.SufiAI.MongoDB;
using SufiChain.SufiPlatform.SufiAI;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.SufiAI;

[DependsOn(typeof(AbpAutofacModule), typeof(AbpEntityFrameworkCoreSqliteModule), typeof(SufiAIEntityFrameworkCoreModule))]
public class KnowledgeSqliteTestModule : AbpModule
{
    private SqliteConnection _connection = null!;
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        Configure<AbpDbConnectionOptions>(options => options.ConnectionStrings.Default = "Data Source=:memory:");
        Configure<AbpDbContextOptions>(options => options.Configure(
            configuration => configuration.DbContextOptions.UseSqlite(_connection)));
    }
    public override void OnApplicationShutdown(ApplicationShutdownContext context) => _connection.Dispose();
}

public sealed class KnowledgeMongoFixture : IDisposable
{
    private readonly MongoDbRunner _runner = MongoDbRunner.Start();
    public string ConnectionString => new MongoUrlBuilder(_runner.ConnectionString)
    {
        DatabaseName = "KnowledgeApprovalTests"
    }.ToString();
    public void Dispose() => _runner.Dispose();
}

[DependsOn(typeof(AbpAutofacModule), typeof(SufiAIMongoDbModule))]
public class KnowledgeMongoTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<KnowledgeMongoFixture>();
        context.Services.AddOptions<AbpDbConnectionOptions>().Configure<KnowledgeMongoFixture>((options, fixture) =>
        {
            options.ConnectionStrings.Default = fixture.ConnectionString;
            options.ConnectionStrings[SufiAIDbProperties.ConnectionStringName] = fixture.ConnectionString;
        });
    }
}
