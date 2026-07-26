using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Settings.EntityFrameworkCore;

public static class SettingsDbContextModelBuilderExtensions
{
    //TODO: Instead of getting parameters, get a action of SettingsModelBuilderConfigurationOptions like other modules
    public static void ConfigureSettings(
        [NotNull] this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        if (builder.IsTenantOnlyDatabase())
        {
            return;
        }

        builder.Entity<Setting>(b =>
        {
            b.ToTable(SufiSettingsDbProperties.DbTablePrefix + "Settings", SufiSettingsDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(x => x.Name).HasMaxLength(SettingConsts.MaxNameLength).IsRequired();

            if (builder.IsUsingOracle()) { SettingConsts.MaxValueLengthValue = 2000; }
            b.Property(x => x.Value).HasMaxLength(SettingConsts.MaxValueLengthValue).IsRequired();

            b.Property(x => x.ProviderName).HasMaxLength(SettingConsts.MaxProviderNameLength).IsRequired(false);
            b.Property(x => x.ProviderKey).HasMaxLength(SettingConsts.MaxProviderKeyLength).IsRequired(false);

            b.HasIndex(x => new { x.Name, x.ProviderName, x.ProviderKey }).IsUnique(true);

            b.ApplyObjectExtensionMappings();
        });

        builder.Entity<SettingDefinitionRecord>(b =>
        {
            b.ToTable(SufiSettingsDbProperties.DbTablePrefix + "SettingDefinitions", SufiSettingsDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(x => x.Name).HasMaxLength(SettingDefinitionRecordConsts.MaxNameLength).IsRequired();
            b.Property(x => x.DisplayName).HasMaxLength(SettingDefinitionRecordConsts.MaxDisplayNameLength).IsRequired();
            b.Property(x => x.Description).HasMaxLength(SettingDefinitionRecordConsts.MaxDescriptionLength).IsRequired(false);
            b.Property(x => x.DefaultValue).HasMaxLength(SettingDefinitionRecordConsts.MaxDefaultValueLength).IsRequired(false);
            b.Property(x => x.Providers).HasMaxLength(SettingDefinitionRecordConsts.MaxProvidersLength).IsRequired(false);

            b.HasIndex(x => new { x.Name }).IsUnique();

            b.ApplyObjectExtensionMappings();
        });

        builder.TryConfigureObjectExtensions<SettingsDbContext>();
    }
}
