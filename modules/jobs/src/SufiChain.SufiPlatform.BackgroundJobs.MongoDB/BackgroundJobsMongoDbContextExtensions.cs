using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.BackgroundJobs.MongoDB;

public static class BackgroundJobsMongoDbContextExtensions
{
    public static void ConfigureBackgroundJobs(
        this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<BackgroundJobRecord>(b =>
        {
            b.CollectionName = SufiBackgroundJobsDbProperties.DbTablePrefix + "BackgroundJobs";
        });
    }
}
