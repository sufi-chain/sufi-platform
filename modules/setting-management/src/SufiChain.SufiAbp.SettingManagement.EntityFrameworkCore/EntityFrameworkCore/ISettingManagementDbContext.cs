using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.SettingManagement.EntityFrameworkCore;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiAbpSettingManagementDbProperties.ConnectionStringName)]
public interface ISettingManagementDbContext : IEfCoreDbContext
{
    DbSet<Setting> Settings { get; }

    DbSet<SettingDefinitionRecord> SettingDefinitionRecords { get; }
}
