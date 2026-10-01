using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using SufiChain.SufiPlatform.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;

namespace SufiChain.SufiPlatform.BackgroundJobs;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpTestBaseModule),
    typeof(SufiBackgroundJobsEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule))]
public class BackgroundJobsSqliteTestModule : AbpModule
{
    private SqliteConnection _connection = null!;

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        Configure<AbpDbConnectionOptions>(options =>
            options.ConnectionStrings[SufiBackgroundJobsDbProperties.ConnectionStringName] = "Data Source=:memory:");
        Configure<AbpDbContextOptions>(options =>
            options.Configure(configuration => configuration.DbContextOptions.UseSqlite(_connection)));
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        context.ServiceProvider.GetRequiredService<BackgroundJobsDbContext>()
            .GetService<IRelationalDatabaseCreator>()
            .CreateTables();
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context) => _connection.Dispose();
}
