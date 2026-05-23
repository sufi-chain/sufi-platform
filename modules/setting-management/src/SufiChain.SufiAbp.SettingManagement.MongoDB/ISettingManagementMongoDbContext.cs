using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.SettingManagement.MongoDB;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiAbpSettingManagementDbProperties.ConnectionStringName)]
public interface ISettingManagementMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<Setting> Settings { get; }
    IMongoCollection<SettingDefinitionRecord> SettingDefinitionRecords { get; }
}
