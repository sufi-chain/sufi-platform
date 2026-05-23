using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.BackgroundJobs.EntityFrameworkCore;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiAbpBackgroundJobsDbProperties.ConnectionStringName)]
public class BackgroundJobsDbContext : AbpDbContext<BackgroundJobsDbContext>, ISufiAbpBackgroundJobsDbContext
{
    public DbSet<BackgroundJobRecord> BackgroundJobs { get; set; }

    public BackgroundJobsDbContext(DbContextOptions<BackgroundJobsDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureBackgroundJobs();
    }
}
