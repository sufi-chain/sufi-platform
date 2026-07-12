using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SufiChain.SufiPlatform.BackgroundJobs.EntityFrameworkCore;

public static class SufiBackgroundJobsDbContextModelCreatingExtensions
{
    public static void ConfigureSufiBackgroundJobs(
        this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        if (builder.IsTenantOnlyDatabase())
        {
            return;
        }

        builder.Entity<BackgroundJobRecord>(b =>
        {
            b.ToTable(SufiBackgroundJobsDbProperties.DbTablePrefix + "BackgroundJobs", SufiBackgroundJobsDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(x => x.ApplicationName).IsRequired(false).HasMaxLength(BackgroundJobRecordConsts.MaxApplicationNameLength);
            b.Property(x => x.JobName).IsRequired().HasMaxLength(BackgroundJobRecordConsts.MaxJobNameLength);
            b.Property(x => x.JobArgs).IsRequired().HasMaxLength(BackgroundJobRecordConsts.MaxJobArgsLength);
            b.Property(x => x.TryCount).HasDefaultValue(0);
            b.Property(x => x.NextTryTime);
            b.Property(x => x.LastTryTime);
            b.Property(x => x.IsAbandoned).HasDefaultValue(false);
            b.Property(x => x.Priority).HasDefaultValue(BackgroundJobPriority.Normal).HasSentinel(BackgroundJobPriority.Normal);

            b.HasIndex(x => new { x.IsAbandoned, x.NextTryTime });

            b.ApplyObjectExtensionMappings();
        });

        builder.TryConfigureObjectExtensions<BackgroundJobsDbContext>();
    }
}
