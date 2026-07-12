using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.Users;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace SufiChain.SufiPlatform.Identity.EntityFrameworkCore;

public static class SufiIdentityDbContextModelCreatingExtensions
{
    public static void ConfigureSufiIdentity([NotNull] this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        // IdentityUser
        builder.Entity<IdentityUser>(b =>
        {
            b.ToTable(SufiIdentityDbProperties.DbTablePrefix + "Users", SufiIdentityDbProperties.DbSchema);

            b.ConfigureByConvention();
            b.ConfigureAbpUser();

            b.Property(u => u.NormalizedUserName).IsRequired()
                .HasMaxLength(IdentityUserConsts.MaxNormalizedUserNameLength);
            b.Property(u => u.NormalizedEmail).IsRequired()
                .HasMaxLength(IdentityUserConsts.MaxNormalizedEmailLength);
            b.Property(u => u.PasswordHash).HasMaxLength(IdentityUserConsts.MaxPasswordHashLength);
            b.Property(u => u.SecurityStamp).IsRequired().HasMaxLength(IdentityUserConsts.MaxSecurityStampLength);
            b.Property(u => u.PhoneNumber).HasMaxLength(IdentityUserConsts.MaxPhoneNumberLength);
            b.Property(u => u.Name).HasMaxLength(IdentityUserConsts.MaxNameLength);
            b.Property(u => u.Surname).HasMaxLength(IdentityUserConsts.MaxSurnameLength);

            b.HasMany(u => u.Claims).WithOne().HasForeignKey(uc => uc.UserId).IsRequired();
            b.HasMany(u => u.Logins).WithOne().HasForeignKey(ul => ul.UserId).IsRequired();
            b.HasMany(u => u.Roles).WithOne().HasForeignKey(ur => ur.UserId).IsRequired();
            b.HasMany(u => u.Tokens).WithOne().HasForeignKey(ut => ut.UserId).IsRequired();
            b.HasMany(u => u.OrganizationUnits).WithOne().HasForeignKey(ou => ou.UserId).IsRequired();

            b.HasIndex(u => u.NormalizedUserName);
            b.HasIndex(u => u.NormalizedEmail);
            b.HasIndex(u => u.UserName);
            b.HasIndex(u => u.Email);

            b.ApplyObjectExtensionMappings();
        });

        // IdentityUserClaim
        builder.Entity<IdentityUserClaim>(b =>
        {
            b.ToTable(SufiIdentityDbProperties.DbTablePrefix + "UserClaims", SufiIdentityDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(uc => uc.ClaimType).HasMaxLength(IdentityUserClaimConsts.MaxClaimTypeLength).IsRequired();
            b.Property(uc => uc.ClaimValue).HasMaxLength(IdentityUserClaimConsts.MaxClaimValueLength);

            b.HasIndex(uc => uc.UserId);

            b.ApplyObjectExtensionMappings();
        });

        // IdentityUserRole
        builder.Entity<IdentityUserRole>(b =>
        {
            b.ToTable(SufiIdentityDbProperties.DbTablePrefix + "UserRoles", SufiIdentityDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.HasKey(ur => new { ur.UserId, ur.RoleId });

            b.HasIndex(ur => new { ur.RoleId, ur.UserId });

            b.ApplyObjectExtensionMappings();
        });

        // IdentityUserLogin
        builder.Entity<IdentityUserLogin>(b =>
        {
            b.ToTable(SufiIdentityDbProperties.DbTablePrefix + "UserLogins", SufiIdentityDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.HasKey(x => new { x.UserId, x.LoginProvider });

            b.Property(ul => ul.LoginProvider).HasMaxLength(IdentityUserLoginConsts.MaxLoginProviderLength).IsRequired();
            b.Property(ul => ul.ProviderKey).HasMaxLength(IdentityUserLoginConsts.MaxProviderKeyLength).IsRequired();
            b.Property(ul => ul.ProviderDisplayName).HasMaxLength(IdentityUserLoginConsts.MaxProviderDisplayNameLength);

            b.HasIndex(l => new { l.LoginProvider, l.ProviderKey });

            b.ApplyObjectExtensionMappings();
        });

        // IdentityUserToken
        builder.Entity<IdentityUserToken>(b =>
        {
            b.ToTable(SufiIdentityDbProperties.DbTablePrefix + "UserTokens", SufiIdentityDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.HasKey(l => new { l.UserId, l.LoginProvider, l.Name });

            b.Property(ul => ul.LoginProvider).HasMaxLength(IdentityUserTokenConsts.MaxLoginProviderLength).IsRequired();
            b.Property(ul => ul.Name).HasMaxLength(IdentityUserTokenConsts.MaxNameLength).IsRequired();

            b.ApplyObjectExtensionMappings();
        });

        // IdentityRole
        builder.Entity<IdentityRole>(b =>
        {
            b.ToTable(SufiIdentityDbProperties.DbTablePrefix + "Roles", SufiIdentityDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(r => r.Name).IsRequired().HasMaxLength(IdentityRoleConsts.MaxNameLength);
            b.Property(r => r.NormalizedName).IsRequired().HasMaxLength(IdentityRoleConsts.MaxNormalizedNameLength);

            b.HasMany(r => r.Claims).WithOne().HasForeignKey(rc => rc.RoleId).IsRequired();

            b.HasIndex(r => r.NormalizedName);

            b.ApplyObjectExtensionMappings();
        });

        // IdentityRoleClaim
        builder.Entity<IdentityRoleClaim>(b =>
        {
            b.ToTable(SufiIdentityDbProperties.DbTablePrefix + "RoleClaims", SufiIdentityDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(x => x.Id).ValueGeneratedNever();
            b.Property(rc => rc.ClaimType).HasMaxLength(IdentityRoleClaimConsts.MaxClaimTypeLength).IsRequired();
            b.Property(rc => rc.ClaimValue).HasMaxLength(IdentityRoleClaimConsts.MaxClaimValueLength);

            b.HasIndex(rc => rc.RoleId);

            b.ApplyObjectExtensionMappings();
        });

        // IdentityClaimType
        builder.Entity<IdentityClaimType>(b =>
        {
            b.ToTable(SufiIdentityDbProperties.DbTablePrefix + "ClaimTypes", SufiIdentityDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(ct => ct.Name).IsRequired().HasMaxLength(IdentityClaimTypeConsts.MaxNameLength);
            b.Property(ct => ct.Regex).HasMaxLength(IdentityClaimTypeConsts.MaxRegexLength);
            b.Property(ct => ct.RegexDescription).HasMaxLength(IdentityClaimTypeConsts.MaxRegexDescriptionLength);
            b.Property(ct => ct.Description).HasMaxLength(IdentityClaimTypeConsts.MaxDescriptionLength);

            b.ApplyObjectExtensionMappings();
        });

        // OrganizationUnit
        builder.Entity<OrganizationUnit>(b =>
        {
            b.ToTable(SufiIdentityDbProperties.DbTablePrefix + "OrganizationUnits", SufiIdentityDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(ou => ou.Code).IsRequired().HasMaxLength(OrganizationUnitConsts.MaxCodeLength);
            b.Property(ou => ou.DisplayName).IsRequired().HasMaxLength(OrganizationUnitConsts.MaxDisplayNameLength);

            b.HasMany(ou => ou.Roles).WithOne().HasForeignKey(r => r.OrganizationUnitId).IsRequired();

            b.HasIndex(ou => ou.Code);

            b.ApplyObjectExtensionMappings();
        });

        // OrganizationUnitRole
        builder.Entity<OrganizationUnitRole>(b =>
        {
            b.ToTable(SufiIdentityDbProperties.DbTablePrefix + "OrganizationUnitRoles", SufiIdentityDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.HasKey(x => new { x.OrganizationUnitId, x.RoleId });

            b.HasIndex(x => new { x.RoleId, x.OrganizationUnitId });

            b.ApplyObjectExtensionMappings();
        });

        // IdentitySecurityLog
        builder.Entity<IdentitySecurityLog>(b =>
        {
            b.ToTable(SufiIdentityDbProperties.DbTablePrefix + "SecurityLogs", SufiIdentityDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(x => x.ApplicationName).HasMaxLength(IdentitySecurityLogConsts.MaxApplicationNameLength);
            b.Property(x => x.Identity).HasMaxLength(IdentitySecurityLogConsts.MaxIdentityLength);
            b.Property(x => x.Action).HasMaxLength(IdentitySecurityLogConsts.MaxActionLength);
            b.Property(x => x.UserName).HasMaxLength(IdentitySecurityLogConsts.MaxUserNameLength);
            b.Property(x => x.TenantName).HasMaxLength(IdentitySecurityLogConsts.MaxTenantNameLength);
            b.Property(x => x.ClientId).HasMaxLength(IdentitySecurityLogConsts.MaxClientIdLength);
            b.Property(x => x.CorrelationId).HasMaxLength(IdentitySecurityLogConsts.MaxCorrelationIdLength);
            b.Property(x => x.ClientIpAddress).HasMaxLength(IdentitySecurityLogConsts.MaxClientIpAddressLength);
            b.Property(x => x.BrowserInfo).HasMaxLength(IdentitySecurityLogConsts.MaxBrowserInfoLength);

            b.HasIndex(x => new { x.TenantId, x.UserId });
            b.HasIndex(x => new { x.TenantId, x.ApplicationName });
            b.HasIndex(x => new { x.TenantId, x.Identity });
            b.HasIndex(x => new { x.TenantId, x.Action });

            b.ApplyObjectExtensionMappings();
        });

        // IdentityUserOrganizationUnit
        builder.Entity<IdentityUserOrganizationUnit>(b =>
        {
            b.ToTable(SufiIdentityDbProperties.DbTablePrefix + "UserOrganizationUnits", SufiIdentityDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.HasKey(x => new { x.OrganizationUnitId, x.UserId });

            b.HasIndex(x => new { x.UserId, x.OrganizationUnitId });

            b.ApplyObjectExtensionMappings();
        });

        // IdentityLinkUser
        builder.Entity<IdentityLinkUser>(b =>
        {
            b.ToTable(SufiIdentityDbProperties.DbTablePrefix + "LinkUsers", SufiIdentityDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.HasIndex(x => new { x.SourceUserId, x.SourceTenantId, x.TargetUserId, x.TargetTenantId }).IsUnique();

            b.ApplyObjectExtensionMappings();
        });

        // IdentityUserDelegation
        builder.Entity<IdentityUserDelegation>(b =>
        {
            b.ToTable(SufiIdentityDbProperties.DbTablePrefix + "UserDelegations", SufiIdentityDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.ApplyObjectExtensionMappings();
        });

        // IdentitySession
        builder.Entity<IdentitySession>(b =>
        {
            b.ToTable(SufiIdentityDbProperties.DbTablePrefix + "Sessions", SufiIdentityDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.Property(x => x.SessionId).HasMaxLength(IdentitySessionConsts.MaxSessionIdLength).IsRequired();
            b.Property(x => x.Device).HasMaxLength(IdentitySessionConsts.MaxDeviceLength);
            b.Property(x => x.DeviceInfo).HasMaxLength(IdentitySessionConsts.MaxDeviceInfoLength);
            b.Property(x => x.ClientId).HasMaxLength(IdentitySessionConsts.MaxClientIdLength);
            b.Property(x => x.IpAddresses).HasMaxLength(IdentitySessionConsts.MaxIpAddressesLength);

            b.HasIndex(x => x.SessionId);
            b.HasIndex(x => x.Device);
            b.HasIndex(x => new { x.TenantId, x.UserId });

            b.ApplyObjectExtensionMappings();
        });
    }
}
