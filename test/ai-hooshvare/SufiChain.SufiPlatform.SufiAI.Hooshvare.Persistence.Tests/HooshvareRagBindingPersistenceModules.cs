using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Mongo2Go;
using MongoDB.Driver;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.MongoDB;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpTestBaseModule),
    typeof(SufiAIHooshvareEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule))]
public class HooshvareRagBindingSqliteTestModule : AbpModule
{
    private SqliteConnection _connection = null!;

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = "Data Source=:memory:";
            options.ConnectionStrings[EntityFrameworkCore.AIHooshvareDbProperties.ConnectionStringName] = "Data Source=:memory:";
        });
        Configure<AbpDbContextOptions>(options =>
            options.Configure(configuration => configuration.DbContextOptions.UseSqlite(_connection)));
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        context.ServiceProvider.GetRequiredService<AIHooshvareDbContext>()
            .GetService<IRelationalDatabaseCreator>()
            .CreateTables();
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context) => _connection.Dispose();
}

/// <summary>
/// One mongod per test process, released on process exit, so it cannot leak when module
/// initialization throws in a test constructor.
/// </summary>
public static class HooshvareRagBindingMongoRunner
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
            DatabaseName = $"HooshvareRagBindingTests_{Guid.NewGuid():N}"
        }.ToString();
}

[DependsOn(typeof(AbpAutofacModule), typeof(AbpTestBaseModule), typeof(SufiAIHooshvareMongoDBModule))]
public class HooshvareRagBindingMongoTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var connectionString = HooshvareRagBindingMongoRunner.CreateDatabaseConnectionString();
        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = connectionString;
            options.ConnectionStrings["AIHooshvare"] = connectionString;
        });
    }
}
