using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Calendar.EntityFrameworkCore;

[DependsOn(typeof(AbpAutofacModule), typeof(AbpEntityFrameworkCoreSqliteModule),
    typeof(SufiCalendarEntityFrameworkCoreModule), typeof(SufiCalendarApplicationModule))]
public class CalendarSqliteTestModule : AbpModule
{
    private SqliteConnection _connection = null!;

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Each integrated test owns its application and its own open in-memory database.
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        Configure<AbpDbConnectionOptions>(options =>
            options.ConnectionStrings.Default = "Data Source=:memory:");
        Configure<AbpDbContextOptions>(options =>
            options.Configure(configuration => configuration.DbContextOptions.UseSqlite(_connection)));

    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context)
    {
        _connection.Dispose();
    }
}

[DependsOn(typeof(CalendarSqliteTestModule))]
public class CalendarEntityFrameworkCoreTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Business visibility tests deliberately bypass policies. The authorization fixture does not.
        context.Services.AddAlwaysAllowAuthorization();
    }
}
