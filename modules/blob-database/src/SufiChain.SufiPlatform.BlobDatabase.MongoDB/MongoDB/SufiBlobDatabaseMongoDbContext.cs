using MongoDB.Driver;
using SufiChain.SufiPlatform.BlobDatabase;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.BlobDatabase.MongoDB;

[ConnectionStringName(SufiBlobDatabaseDbProperties.ConnectionStringName)]
public class SufiBlobDatabaseMongoDbContext : AbpMongoDbContext, ISufiBlobDatabaseMongoDbContext
{
    public IMongoCollection<DatabaseBlobContainer> BlobContainers => Collection<DatabaseBlobContainer>();

    public IMongoCollection<DatabaseBlob> Blobs => Collection<DatabaseBlob>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);
        modelBuilder.ConfigureSufiBlobDatabase();
    }
}
