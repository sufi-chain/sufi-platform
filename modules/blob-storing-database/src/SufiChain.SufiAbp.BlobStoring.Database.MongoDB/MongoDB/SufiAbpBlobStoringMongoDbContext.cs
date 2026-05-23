using MongoDB.Driver;
using SufiChain.SufiAbp.BlobStoring.Database;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.BlobStoring.Database.MongoDB;

[ConnectionStringName(SufiAbpBlobStoringDatabaseDbProperties.ConnectionStringName)]
public class SufiAbpBlobStoringMongoDbContext : AbpMongoDbContext, ISufiAbpBlobStoringMongoDbContext
{
    public IMongoCollection<DatabaseBlobContainer> BlobContainers => Collection<DatabaseBlobContainer>();

    public IMongoCollection<DatabaseBlob> Blobs => Collection<DatabaseBlob>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);
        modelBuilder.ConfigureSufiAbpBlobStoringDatabase();
    }
}
