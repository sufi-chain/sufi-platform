using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.BackgroundJobs.MongoDB;

public static class BackgroundJobsMongoDbContextExtensions
{
    public static void ConfigureBackgroundJobs(
        this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<BackgroundJobRecord>(b =>
        {
            b.CollectionName = SufiAbpBackgroundJobsDbProperties.DbTablePrefix + "BackgroundJobs";
        });
    }
}
