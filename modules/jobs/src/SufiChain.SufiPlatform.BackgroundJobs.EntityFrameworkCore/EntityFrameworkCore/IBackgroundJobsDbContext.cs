using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.BackgroundJobs.EntityFrameworkCore;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiBackgroundJobsDbProperties.ConnectionStringName)]
public interface ISufiBackgroundJobsDbContext : IEfCoreDbContext
{
    DbSet<BackgroundJobRecord> BackgroundJobs { get; }
}
