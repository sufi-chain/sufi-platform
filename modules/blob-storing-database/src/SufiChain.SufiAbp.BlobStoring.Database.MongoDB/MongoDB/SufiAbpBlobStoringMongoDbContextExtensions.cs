using SufiChain.SufiAbp.BlobStoring.Database;
using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.BlobStoring.Database.MongoDB;

public static class SufiAbpBlobStoringMongoDbContextExtensions
{
    public static void ConfigureSufiAbpBlobStoringDatabase(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<DatabaseBlobContainer>(b =>
        {
            b.CollectionName = SufiAbpBlobStoringDatabaseDbProperties.DbTablePrefix + "BlobContainers";
        });

        builder.Entity<DatabaseBlob>(b =>
        {
            b.CollectionName = SufiAbpBlobStoringDatabaseDbProperties.DbTablePrefix + "Blobs";
        });
    }
}
