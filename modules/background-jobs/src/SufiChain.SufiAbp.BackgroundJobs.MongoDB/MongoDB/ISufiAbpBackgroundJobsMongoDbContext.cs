using MongoDB.Driver;
using SufiChain.SufiAbp.BackgroundJobs;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.BackgroundJobs.MongoDB;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiAbpBackgroundJobsDbProperties.ConnectionStringName)]
public interface ISufiAbpBackgroundJobsMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<BackgroundJobRecord> BackgroundJobs { get; }
}
