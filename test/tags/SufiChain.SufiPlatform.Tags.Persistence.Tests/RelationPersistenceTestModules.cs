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

public sealed class RelationMongoFixture : IDisposable
{
    private readonly MongoDbRunner _runner = MongoDbRunner.Start();
    public string ConnectionString => new MongoUrlBuilder(_runner.ConnectionString)
    {
        DatabaseName = "TagsRelationsTests"
    }.ToString();
    public void Dispose() => _runner.Dispose();
}

[DependsOn(typeof(AbpAutofacModule), typeof(SufiTagsMongoDbModule))]
public class RelationMongoTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<RelationMongoFixture>();
        context.Services.AddOptions<AbpDbConnectionOptions>().Configure<RelationMongoFixture>((options, fixture) =>
        {
            options.ConnectionStrings.Default = fixture.ConnectionString;
            options.ConnectionStrings[SufiTagsDbProperties.ConnectionStringName] = fixture.ConnectionString;
        });
    }
}
