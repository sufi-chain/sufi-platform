using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;

[DependsOn(
    typeof(HelpDeskKnowledgeBaseTestModule),
    typeof(HelpDeskKnowledgeBaseEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule))]
public class KnowledgeBaseSqliteTestModule : AbpModule
{
    private SqliteConnection _connection = null!;

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = "Data Source=:memory:";
            options.ConnectionStrings[HelpDeskKnowledgeBaseDbProperties.ConnectionStringName] = "Data Source=:memory:";
        });
        Configure<AbpDbContextOptions>(options =>
            options.Configure(configuration => configuration.DbContextOptions.UseSqlite(_connection)));
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        context.ServiceProvider.GetRequiredService<KnowledgeBaseDbContext>()
            .GetService<IRelationalDatabaseCreator>()
            .CreateTables();
    }

    public override void OnApplicationShutdown(ApplicationShutdownContext context) => _connection.Dispose();
}
