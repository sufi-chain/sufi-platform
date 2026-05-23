using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using SufiChain.SufiAbp.SettingManagement;

namespace SufiChain.SufiAbp.SettingManagement.EntityFrameworkCore;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiAbpSettingManagementDbProperties.ConnectionStringName)]
public interface ISufiAbpSettingManagementDbContext : IEfCoreDbContext
{
    DbSet<Setting> Settings { get; }
    DbSet<SettingDefinitionRecord> SettingDefinitionRecords { get; }
}
