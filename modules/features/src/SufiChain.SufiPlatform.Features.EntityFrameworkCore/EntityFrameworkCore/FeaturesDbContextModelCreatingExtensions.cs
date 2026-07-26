using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SufiChain.SufiPlatform.Features.EntityFrameworkCore;

public static class FeaturesDbContextModelCreatingExtensions
{
    public static void ConfigureFeatures(
        this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        if (builder.IsTenantOnlyDatabase())
        {
            return;
        }

        builder.Entity<FeatureValue>(b =>
        {
            b.ToTable(SufiFeaturesDbProperties.DbTablePrefix + "FeatureValues", SufiFeaturesDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(x => x.Name).HasMaxLength(FeatureValueConsts.MaxNameLength).IsRequired();
            b.Property(x => x.Value).HasMaxLength(FeatureValueConsts.MaxValueLength).IsRequired();
            b.Property(x => x.ProviderName).HasMaxLength(FeatureValueConsts.MaxProviderNameLength).IsRequired();
            // Host-scoped values use ProviderName="T" with null ProviderKey (same as ABP).
            b.Property(x => x.ProviderKey).HasMaxLength(FeatureValueConsts.MaxProviderKeyLength).IsRequired(false);

            b.HasIndex(x => new { x.Name, x.ProviderName, x.ProviderKey })
                .IsUnique()
                .HasFilter("[ProviderName] IS NOT NULL AND [ProviderKey] IS NOT NULL");

            b.ApplyObjectExtensionMappings();
        });
        builder.Entity<FeatureGroupDefinitionRecord>(b =>
        {
            b.ToTable(SufiFeaturesDbProperties.DbTablePrefix + "FeatureGroups", SufiFeaturesDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(x => x.Name).HasMaxLength(FeatureGroupDefinitionRecordConsts.MaxNameLength).IsRequired();
            b.Property(x => x.DisplayName).HasMaxLength(FeatureGroupDefinitionRecordConsts.MaxDisplayNameLength).IsRequired();

            b.HasIndex(x => new { x.Name }).IsUnique();

            b.ApplyObjectExtensionMappings();
        });

        builder.Entity<FeatureDefinitionRecord>(b =>
        {
            b.ToTable(SufiFeaturesDbProperties.DbTablePrefix + "Features", SufiFeaturesDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(x => x.GroupName).HasMaxLength(FeatureGroupDefinitionRecordConsts.MaxNameLength).IsRequired();
            b.Property(x => x.Name).HasMaxLength(FeatureDefinitionRecordConsts.MaxNameLength).IsRequired();
            b.Property(x => x.ParentName).HasMaxLength(FeatureDefinitionRecordConsts.MaxNameLength).IsRequired(false);
            b.Property(x => x.DisplayName).HasMaxLength(FeatureDefinitionRecordConsts.MaxDisplayNameLength).IsRequired();
            b.Property(x => x.Description).HasMaxLength(FeatureDefinitionRecordConsts.MaxDescriptionLength).IsRequired(false);
            b.Property(x => x.DefaultValue).HasMaxLength(FeatureDefinitionRecordConsts.MaxDefaultValueLength).IsRequired(false);
            b.Property(x => x.AllowedProviders).HasMaxLength(FeatureDefinitionRecordConsts.MaxAllowedProvidersLength).IsRequired(false);
            b.Property(x => x.ValueType).HasMaxLength(FeatureDefinitionRecordConsts.MaxValueTypeLength).IsRequired(false);

            b.HasIndex(x => new { x.Name }).IsUnique();
            b.HasIndex(x => new { x.GroupName });

            b.ApplyObjectExtensionMappings();
        });

        builder.TryConfigureObjectExtensions<FeaturesDbContext>();
    }
}
