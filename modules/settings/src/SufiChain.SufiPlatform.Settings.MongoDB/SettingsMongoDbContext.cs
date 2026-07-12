using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Settings.MongoDB;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiSettingsDbProperties.ConnectionStringName)]
public class SettingsMongoDbContext : AbpMongoDbContext, ISettingsMongoDbContext
{
    public IMongoCollection<Setting> Settings => Collection<Setting>();
    public IMongoCollection<SettingDefinitionRecord> SettingDefinitionRecords => Collection<SettingDefinitionRecord>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.ConfigureSettings();
    }
}
