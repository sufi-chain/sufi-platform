using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SufiChain.SufiAbp.PermissionManagement.EntityFrameworkCore;

public static class AbpPermissionManagementDbContextModelBuilderExtensions
{
    public static void ConfigurePermissionManagement(
        [NotNull] this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<PermissionGrant>(b =>
        {
            b.ToTable(SufiAbpPermissionManagementDbProperties.DbTablePrefix + "PermissionGrants", SufiAbpPermissionManagementDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(x => x.Name).HasMaxLength(PermissionDefinitionRecordConsts.MaxNameLength).IsRequired();
            b.Property(x => x.ProviderName).HasMaxLength(PermissionGrantConsts.MaxProviderNameLength).IsRequired();
            b.Property(x => x.ProviderKey).HasMaxLength(PermissionGrantConsts.MaxProviderKeyLength).IsRequired();

            b.HasIndex(x => new { x.TenantId, x.Name, x.ProviderName, x.ProviderKey }).IsUnique();

            b.ApplyObjectExtensionMappings();
        });

        builder.Entity<ResourcePermissionGrant>(b =>
        {
            b.ToTable(SufiAbpPermissionManagementDbProperties.DbTablePrefix + "ResourcePermissionGrants", SufiAbpPermissionManagementDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(x => x.Name).HasMaxLength(PermissionDefinitionRecordConsts.MaxNameLength).IsRequired();
            b.Property(x => x.ResourceName).HasMaxLength(PermissionGrantConsts.MaxResourceNameLength).IsRequired();
            b.Property(x => x.ResourceKey).HasMaxLength(PermissionGrantConsts.MaxResourceKeyLength).IsRequired();
            b.Property(x => x.ProviderName).HasMaxLength(PermissionGrantConsts.MaxProviderNameLength).IsRequired();
            b.Property(x => x.ProviderKey).HasMaxLength(PermissionGrantConsts.MaxProviderKeyLength).IsRequired();

            b.HasIndex(x => new { x.TenantId, x.Name, x.ResourceName, x.ResourceKey, x.ProviderName, x.ProviderKey }).IsUnique();

            b.ApplyObjectExtensionMappings();
        });

        if (builder.IsHostDatabase())
        {
            builder.Entity<PermissionGroupDefinitionRecord>(b =>
            {
                b.ToTable(SufiAbpPermissionManagementDbProperties.DbTablePrefix + "PermissionGroups",
                    SufiAbpPermissionManagementDbProperties.DbSchema);

                b.ConfigureByConvention();

                b.Property(x => x.Name).HasMaxLength(PermissionGroupDefinitionRecordConsts.MaxNameLength).IsRequired();
                b.Property(x => x.DisplayName).HasMaxLength(PermissionGroupDefinitionRecordConsts.MaxDisplayNameLength)
                    .IsRequired();

                b.HasIndex(x => new { x.Name }).IsUnique();

                b.ApplyObjectExtensionMappings();
            });

            builder.Entity<PermissionDefinitionRecord>(b =>
            {
                b.ToTable(SufiAbpPermissionManagementDbProperties.DbTablePrefix + "Permissions",
                    SufiAbpPermissionManagementDbProperties.DbSchema);

                b.ConfigureByConvention();

                b.Property(x => x.GroupName).HasMaxLength(PermissionGroupDefinitionRecordConsts.MaxNameLength).IsRequired(false);
                b.Property(x => x.Name).HasMaxLength(PermissionDefinitionRecordConsts.MaxNameLength).IsRequired();
                b.Property(x => x.ResourceName).HasMaxLength(PermissionDefinitionRecordConsts.MaxResourceNameLength).IsRequired(false);
                b.Property(x => x.ManagementPermissionName).HasMaxLength(PermissionDefinitionRecordConsts.MaxManagementPermissionNameLength).IsRequired(false);
                b.Property(x => x.ParentName).HasMaxLength(PermissionDefinitionRecordConsts.MaxNameLength).IsRequired(false);
                b.Property(x => x.DisplayName).HasMaxLength(PermissionDefinitionRecordConsts.MaxDisplayNameLength).IsRequired();
                b.Property(x => x.Providers).HasMaxLength(PermissionDefinitionRecordConsts.MaxProvidersLength).IsRequired(false);
                b.Property(x => x.StateCheckers).HasMaxLength(PermissionDefinitionRecordConsts.MaxStateCheckersLength).IsRequired(false);

                b.HasIndex(x => new { x.ResourceName, x.Name }).IsUnique();
                b.HasIndex(x => new { x.GroupName });

                b.ApplyObjectExtensionMappings();
            });
        }

        builder.TryConfigureObjectExtensions<PermissionManagementDbContext>();
    }
}
