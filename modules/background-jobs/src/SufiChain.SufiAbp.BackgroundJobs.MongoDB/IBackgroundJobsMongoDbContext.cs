using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.BackgroundJobs.MongoDB;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiAbpBackgroundJobsDbProperties.ConnectionStringName)]
public interface IBackgroundJobsMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<BackgroundJobRecord> BackgroundJobs { get; }
}
