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

/// <summary>
/// One mongod per test process, released on process exit, so it cannot leak when module
/// initialization throws in a test constructor.
/// </summary>
public static class KnowledgeMongoRunner
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
            DatabaseName = $"KnowledgeApprovalTests_{Guid.NewGuid():N}"
        }.ToString();
}

[DependsOn(typeof(AbpAutofacModule), typeof(SufiAIMongoDbModule))]
public class KnowledgeMongoTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var connectionString = KnowledgeMongoRunner.CreateDatabaseConnectionString();
        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = connectionString;
            options.ConnectionStrings[SufiAIDbProperties.ConnectionStringName] = connectionString;
        });
    }
}
