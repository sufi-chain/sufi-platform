using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Features.MongoDB;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiFeaturesDbProperties.ConnectionStringName)]
public interface IFeaturesMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<FeatureGroupDefinitionRecord> FeatureGroups { get; }

    IMongoCollection<FeatureDefinitionRecord> Features { get; }

    IMongoCollection<FeatureValue> FeatureValues { get; }
}
