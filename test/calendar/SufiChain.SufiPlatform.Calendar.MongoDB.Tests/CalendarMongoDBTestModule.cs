using Microsoft.Extensions.DependencyInjection;
using Mongo2Go;
using MongoDB.Driver;
using Volo.Abp.Autofac;
using Volo.Abp.Data;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.Calendar.MongoDB;

/// <summary>
/// One mongod per test process, released on process exit, so it cannot leak when module
/// initialization throws in a test constructor.
/// </summary>
public static class CalendarMongoDBRunner
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
            DatabaseName = $"CalendarTests_{Guid.NewGuid():N}"
        }.ToString();
}

[DependsOn(typeof(AbpAutofacModule), typeof(SufiCalendarMongoDbModule),
    typeof(SufiCalendarApplicationModule))]
public class CalendarMongoDBTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Each integrated test owns a service provider and database on the shared server process.
        var connectionString = CalendarMongoDBRunner.CreateDatabaseConnectionString();
        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = connectionString;
            options.ConnectionStrings[SufiCalendarDbProperties.ConnectionStringName] = connectionString;
        });
        Configure<AbpUnitOfWorkDefaultOptions>(options =>
            options.TransactionBehavior = UnitOfWorkTransactionBehavior.Disabled);
        // These scenarios verify business visibility; permission-policy denial is a separate suite.
        context.Services.AddAlwaysAllowAuthorization();
    }
}
