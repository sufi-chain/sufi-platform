using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.SufiSaas.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;
using Volo.Abp.Testing;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.SufiSaas;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpTestBaseModule),
    typeof(SufiSaasEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule))]
public class IssuedLicenseSqliteTestModule : AbpModule
{
    private SqliteConnection _connection = null!;

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        Configure<AbpDbConnectionOptions>(options =>
            options.ConnectionStrings[SufiSaasDbProperties.ConnectionStringName] = "Data Source=:memory:");
        Configure<AbpDbContextOptions>(options =>
            options.Configure(configuration => configuration.DbContextOptions.UseSqlite(_connection)));
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        using var unit = context.ServiceProvider.GetRequiredService<IUnitOfWorkManager>()
            .Begin(requiresNew: true, isTransactional: false);
        var db = context.ServiceProvider.GetRequiredService<SufiSaasDbContext>();
        var script = db.Database.GenerateCreateScript();
        foreach (var statement in script.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (statement.Contains("IssuedLicense", StringComparison.Ordinal))
            {
                db.Database.ExecuteSqlRaw(statement);
            }
        }

        unit.CompleteAsync().GetAwaiter().GetResult();
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context) => _connection.Dispose();
}
