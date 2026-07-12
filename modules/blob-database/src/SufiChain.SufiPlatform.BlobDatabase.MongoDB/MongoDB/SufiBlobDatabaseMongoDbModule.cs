using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.BlobDatabase;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.BlobDatabase.MongoDB;

[DependsOn(
    typeof(SufiBlobDatabaseDomainModule),
    typeof(AbpMongoDbModule)
)]
public class SufiBlobDatabaseMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<SufiBlobDatabaseMongoDbContext>(options =>
        {
            options.AddRepository<DatabaseBlobContainer, MongoDbDatabaseBlobContainerRepository>();
            options.AddRepository<DatabaseBlob, MongoDbDatabaseBlobRepository>();
        });
    }
}
