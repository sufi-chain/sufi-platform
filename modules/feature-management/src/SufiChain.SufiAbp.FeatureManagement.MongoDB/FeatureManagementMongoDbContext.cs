using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.FeatureManagement.MongoDB;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiAbpFeatureManagementDbProperties.ConnectionStringName)]
public class FeatureManagementMongoDbContext : AbpMongoDbContext, IFeatureManagementMongoDbContext
{
    public IMongoCollection<FeatureGroupDefinitionRecord> FeatureGroups => Collection<FeatureGroupDefinitionRecord>();
    public IMongoCollection<FeatureDefinitionRecord> Features => Collection<FeatureDefinitionRecord>();
    public IMongoCollection<FeatureValue> FeatureValues => Collection<FeatureValue>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.ConfigureFeatureManagement();
    }
}
