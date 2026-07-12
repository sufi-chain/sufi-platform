using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Settings.MongoDB;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiSettingsDbProperties.ConnectionStringName)]
public interface ISettingsMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<Setting> Settings { get; }
    IMongoCollection<SettingDefinitionRecord> SettingDefinitionRecords { get; }
}
