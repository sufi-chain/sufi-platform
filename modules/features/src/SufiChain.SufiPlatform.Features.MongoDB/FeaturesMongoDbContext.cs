using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Features.MongoDB;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiFeaturesDbProperties.ConnectionStringName)]
public class FeaturesMongoDbContext : AbpMongoDbContext, IFeaturesMongoDbContext
{
    public IMongoCollection<FeatureGroupDefinitionRecord> FeatureGroups => Collection<FeatureGroupDefinitionRecord>();
    public IMongoCollection<FeatureDefinitionRecord> Features => Collection<FeatureDefinitionRecord>();
    public IMongoCollection<FeatureValue> FeatureValues => Collection<FeatureValue>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.ConfigureFeatures();
    }
}
