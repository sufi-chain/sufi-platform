using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;
using SufiChain.SufiPlatform.Tenants;
using SufiChain.SufiPlatform.Tenants.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Tenants.EntityFrameworkCore;

public static class SufiTenantsDbContextModelCreatingExtensions
{
    public static void ConfigureSufiTenants(
        this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
        
        if (builder.IsTenantOnlyDatabase())
        {
            return;
        }

        builder.Entity<Tenant>(b =>
        {
            b.ToTable(SufiTenantsDbProperties.DbTablePrefix + "Tenants", SufiTenantsDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(t => t.Name).IsRequired().HasMaxLength(TenantConsts.MaxNameLength);
            b.Property(t => t.NormalizedName).IsRequired().HasMaxLength(TenantConsts.MaxNameLength);
            b.Property(t => t.EditionId);
            b.Property(t => t.OwnerUserId);
            b.Property(t => t.DatabaseName).HasMaxLength(TenantConsts.MaxDatabaseNameLength);
            b.Property(t => t.PrimarySubdomain).HasMaxLength(TenantConsts.MaxSubdomainLength);

            b.HasMany(u => u.ConnectionStrings).WithOne().HasForeignKey(uc => uc.TenantId).IsRequired();
            b.HasMany(u => u.Domains).WithOne().HasForeignKey(domain => domain.TenantId).IsRequired();

            b.HasIndex(u => u.Name);
            b.HasIndex(u => u.NormalizedName);
            b.HasIndex(u => u.EditionId);
            b.HasIndex(u => u.DatabaseName)
                .IsUnique()
                .HasFilter("[DatabaseName] IS NOT NULL");
            b.HasIndex(u => u.PrimarySubdomain)
                .IsUnique()
                .HasFilter("[PrimarySubdomain] IS NOT NULL");

            b.ApplyObjectExtensionMappings();
        });

        builder.Entity<TenantConnectionString>(b =>
        {
            b.ToTable(SufiTenantsDbProperties.DbTablePrefix + "TenantConnectionStrings", SufiTenantsDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.HasKey(x => new { x.TenantId, x.Name });

            b.Property(cs => cs.Name).IsRequired().HasMaxLength(TenantConnectionStringConsts.MaxNameLength);
            b.Property(cs => cs.Value).IsRequired().HasMaxLength(TenantConnectionStringConsts.MaxValueLength);

            b.ApplyObjectExtensionMappings();
        });

        builder.Entity<TenantDomain>(b =>
        {
            b.ToTable(SufiTenantsDbProperties.DbTablePrefix + "TenantDomains", SufiTenantsDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(domain => domain.Host)
                .IsRequired()
                .HasMaxLength(TenantConsts.MaxDomainHostLength);
            b.Property(domain => domain.Type).IsRequired();
            b.Property(domain => domain.IsVerified).IsRequired();
            b.Property(domain => domain.IsActive).IsRequired();

            b.HasIndex(domain => domain.Host).IsUnique();
            b.HasIndex(domain => new { domain.TenantId, domain.IsActive });
        });

        builder.TryConfigureObjectExtensions<TenantsDbContext>();
    }
}
