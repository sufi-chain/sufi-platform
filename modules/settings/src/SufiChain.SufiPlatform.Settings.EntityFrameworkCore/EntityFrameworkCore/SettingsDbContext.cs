using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Settings.EntityFrameworkCore;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiSettingsDbProperties.ConnectionStringName)]
public class SettingsDbContext : AbpDbContext<SettingsDbContext>, ISettingsDbContext
{
    public DbSet<Setting> Settings { get; set; }
    public DbSet<SettingDefinitionRecord> SettingDefinitionRecords { get; set; }

    public SettingsDbContext(DbContextOptions<SettingsDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureSettings();
    }
}
