using SufiChain.SufiPlatform.BlobDatabase;
using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.BlobDatabase.MongoDB;

public static class SufiBlobDatabaseMongoDbContextExtensions
{
    public static void ConfigureSufiBlobDatabase(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<DatabaseBlobContainer>(b =>
        {
            b.CollectionName = SufiBlobDatabaseDbProperties.DbTablePrefix + "BlobContainers";
        });

        builder.Entity<DatabaseBlob>(b =>
        {
            b.CollectionName = SufiBlobDatabaseDbProperties.DbTablePrefix + "Blobs";
        });
    }
}
