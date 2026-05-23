using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.BackgroundJobs.EntityFrameworkCore;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiAbpBackgroundJobsDbProperties.ConnectionStringName)]
public interface ISufiAbpBackgroundJobsDbContext : IEfCoreDbContext
{
    DbSet<BackgroundJobRecord> BackgroundJobs { get; }
}
