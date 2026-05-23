using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using Volo.Abp.MultiTenancy;
using SufiChain.SufiAbp.SettingManagement;

namespace SufiChain.SufiAbp.SettingManagement.MongoDB;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiAbpSettingManagementDbProperties.ConnectionStringName)]
public interface ISufiAbpSettingManagementMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<Setting> Settings { get; }
    IMongoCollection<SettingDefinitionRecord> SettingDefinitionRecords { get; }
}
