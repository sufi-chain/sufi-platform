using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.FeatureManagement.MongoDB;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiAbpFeatureManagementDbProperties.ConnectionStringName)]
public interface IFeatureManagementMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<FeatureGroupDefinitionRecord> FeatureGroups { get; }

    IMongoCollection<FeatureDefinitionRecord> Features { get; }

    IMongoCollection<FeatureValue> FeatureValues { get; }
}
