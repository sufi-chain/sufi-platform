using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.BackgroundJobs.MongoDB;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiBackgroundJobsDbProperties.ConnectionStringName)]
public interface ISufiBackgroundJobsMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<BackgroundJobRecord> BackgroundJobs { get; }
}
