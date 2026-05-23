using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;
using Sufihain.SufiAbp.ShortLinkGenerator.EntityFrameworkCore;

namespace Sufihain.SufiAbp.ShortLinkGenerator.EntityFrameworkCore;

[DependsOn(
    typeof(ShortLinkGeneratorApplicationTestModule),
    typeof(ShortLinkGeneratorEntityFrameworkCoreModule),
    typeof(SufiAbpEntityFrameworkCoreSqliteModule)
    )]
public class SufiAbpShortLinkGeneratorEntityFrameworkCoreTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var sqliteConnection = CreateDatabaseAndGetConnection();

        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(abpDbContextConfigurationContext =>
            {
                abpDbContextConfigurationContext.DbContextOptions.UseSqlite(sqliteConnection);
            });
        });
    }

    private static SqliteConnection CreateDatabaseAndGetConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        return connection;
    }
}
