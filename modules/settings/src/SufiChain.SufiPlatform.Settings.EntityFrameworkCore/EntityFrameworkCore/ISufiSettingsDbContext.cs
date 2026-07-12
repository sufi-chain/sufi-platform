using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using SufiChain.SufiPlatform.Settings;

namespace SufiChain.SufiPlatform.Settings.EntityFrameworkCore;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiSettingsDbProperties.ConnectionStringName)]
public interface ISufiSettingsDbContext : IEfCoreDbContext
{
    DbSet<Setting> Settings { get; }
    DbSet<SettingDefinitionRecord> SettingDefinitionRecords { get; }
}
