using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.BlobStoring.Database;
using SufiChain.SufiAbp.MongoDB;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.BlobStoring.Database.MongoDB;

[DependsOn(
    typeof(SufiAbpBlobStoringDatabaseDomainModule),
    typeof(SufiAbpMongoDbModule)
)]
public class SufiAbpBlobStoringDatabaseMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<SufiAbpBlobStoringMongoDbContext>(options =>
        {
            options.AddRepository<DatabaseBlobContainer, MongoDbDatabaseBlobContainerRepository>();
            options.AddRepository<DatabaseBlob, MongoDbDatabaseBlobRepository>();
        });
    }
}
