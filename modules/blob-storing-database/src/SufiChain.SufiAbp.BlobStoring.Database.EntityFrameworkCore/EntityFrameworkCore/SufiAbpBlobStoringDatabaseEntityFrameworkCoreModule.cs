using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.BlobStoring.Database;
using SufiChain.SufiAbp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.BlobStoring.Database.EntityFrameworkCore;

[DependsOn(
    typeof(SufiAbpBlobStoringDatabaseDomainModule),
    typeof(SufiAbpEntityFrameworkCoreModule)
)]
public class SufiAbpBlobStoringDatabaseEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<SufiAbpBlobStoringDbContext>(options =>
        {
            options.AddRepository<DatabaseBlobContainer, EfCoreDatabaseBlobContainerRepository>();
            options.AddRepository<DatabaseBlob, EfCoreDatabaseBlobRepository>();
        });
    }
}
