using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;
using SufiChain.SufiAbp.TenantManagement;
using SufiChain.SufiAbp.TenantManagement.EntityFrameworkCore;

namespace SufiChain.SufiAbp.TenantManagement.EntityFrameworkCore;

public static class SufiAbpTenantManagementDbContextModelCreatingExtensions
{
    public static void ConfigureSufiAbpTenantManagement(
        this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
        
        if (builder.IsTenantOnlyDatabase())
        {
            return;
        }

        builder.Entity<Tenant>(b =>
        {
            b.ToTable(SufiAbpTenantManagementDbProperties.DbTablePrefix + "Tenants", SufiAbpTenantManagementDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(t => t.Name).IsRequired().HasMaxLength(TenantConsts.MaxNameLength);
            b.Property(t => t.NormalizedName).IsRequired().HasMaxLength(TenantConsts.MaxNameLength);

            b.HasMany(u => u.ConnectionStrings).WithOne().HasForeignKey(uc => uc.TenantId).IsRequired();

            b.HasIndex(u => u.Name);
            b.HasIndex(u => u.NormalizedName);

            b.ApplyObjectExtensionMappings();
        });

        builder.Entity<TenantConnectionString>(b =>
        {
            b.ToTable(SufiAbpTenantManagementDbProperties.DbTablePrefix + "TenantConnectionStrings", SufiAbpTenantManagementDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.HasKey(x => new { x.TenantId, x.Name });

            b.Property(cs => cs.Name).IsRequired().HasMaxLength(TenantConnectionStringConsts.MaxNameLength);
            b.Property(cs => cs.Value).IsRequired().HasMaxLength(TenantConnectionStringConsts.MaxValueLength);

            b.ApplyObjectExtensionMappings();
        });

        builder.TryConfigureObjectExtensions<TenantManagementDbContext>();
    }
}
