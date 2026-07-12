using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.BackgroundJobs.MongoDB;

public static class SufiBackgroundJobsMongoDbContextExtensions
{
    public static void ConfigureSufiBackgroundJobs(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<BackgroundJobRecord>(b =>
        {
            b.CollectionName = SufiBackgroundJobsDbProperties.DbTablePrefix + "BackgroundJobs";
        });
    }
}
