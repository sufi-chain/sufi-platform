using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.BlobDatabase;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.BlobDatabase.EntityFrameworkCore;

[DependsOn(
    typeof(SufiBlobDatabaseDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SufiBlobDatabaseEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<SufiBlobDatabaseDbContext>(options =>
        {
            options.AddRepository<DatabaseBlobContainer, EfCoreDatabaseBlobContainerRepository>();
            options.AddRepository<DatabaseBlob, EfCoreDatabaseBlobRepository>();
        });
    }
}
