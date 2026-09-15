using Microsoft.Extensions.DependencyInjection;
using Mongo2Go;
using MongoDB.Driver;
using Volo.Abp.Autofac;
using Volo.Abp.Data;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.Calendar.MongoDB;

public sealed class CalendarMongoDBFixture : IDisposable
{
    private readonly MongoDbRunner _runner;
    public string ConnectionString { get; }

    public CalendarMongoDBFixture()
    {
        _runner = MongoDbRunner.Start(singleNodeReplSet: true);
        ConnectionString = new MongoUrlBuilder(_runner.ConnectionString)
        {
            DatabaseName = $"CalendarTests_{Guid.NewGuid():N}"
        }.ToString();
    }

    public void Dispose() => _runner.Dispose();
}

[DependsOn(typeof(AbpAutofacModule), typeof(SufiCalendarMongoDbModule),
    typeof(SufiCalendarApplicationModule))]
public class CalendarMongoDBTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Each integrated test owns a service provider, database, and disposable server process.
        context.Services.AddSingleton<CalendarMongoDBFixture>();
        context.Services.AddOptions<AbpDbConnectionOptions>()
            .Configure<CalendarMongoDBFixture>((options, fixture) =>
            {
                options.ConnectionStrings.Default = fixture.ConnectionString;
                options.ConnectionStrings[SufiCalendarDbProperties.ConnectionStringName] = fixture.ConnectionString;
            });
        Configure<AbpUnitOfWorkDefaultOptions>(options =>
            options.TransactionBehavior = UnitOfWorkTransactionBehavior.Disabled);
        // These scenarios verify business visibility; permission-policy denial is a separate suite.
        context.Services.AddAlwaysAllowAuthorization();
    }
}
