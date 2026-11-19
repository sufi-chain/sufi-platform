using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.BlobDatabase;
using Volo.Abp.Data;
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

        // Same lifetime rule as File Manager: the upload stores bytes through this
        // context, and a scoped instance opened on the host catalog would keep them
        // there after the tenant is current.
        context.Services.Replace(ServiceDescriptor.Transient<ISufiBlobDatabaseDbContext, SufiBlobDatabaseDbContext>());

        Configure<AbpDbConnectionOptions>(options =>
        {
            options.Databases.Configure(SufiBlobDatabaseDbProperties.ConnectionStringName, db =>
            {
                db.IsUsedByTenants = true;
            });
        });
    }
}
