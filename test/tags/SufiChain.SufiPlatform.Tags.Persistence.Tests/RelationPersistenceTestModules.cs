using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mongo2Go;
using MongoDB.Driver;
using SufiChain.SufiPlatform.Tags.EntityFrameworkCore;
using SufiChain.SufiPlatform.Tags.MongoDB;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Tags;

[DependsOn(typeof(AbpAutofacModule), typeof(AbpEntityFrameworkCoreSqliteModule), typeof(SufiTagsEntityFrameworkCoreModule))]
public class RelationSqliteTestModule : AbpModule
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
public static class RelationMongoRunner
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
            DatabaseName = $"TagsRelationsTests_{Guid.NewGuid():N}"
        }.ToString();
}

[DependsOn(typeof(AbpAutofacModule), typeof(SufiTagsMongoDbModule))]
public class RelationMongoTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var connectionString = RelationMongoRunner.CreateDatabaseConnectionString();
        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = connectionString;
            options.ConnectionStrings[SufiTagsDbProperties.ConnectionStringName] = connectionString;
        });
    }
}
