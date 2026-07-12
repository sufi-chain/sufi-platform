using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;
using SufiChain.SufiPlatform.OpenIddict;
using SufiChain.SufiPlatform.OpenIddict.Applications;
using SufiChain.SufiPlatform.OpenIddict.Authorizations;
using SufiChain.SufiPlatform.OpenIddict.Scopes;
using SufiChain.SufiPlatform.OpenIddict.Tokens;

namespace SufiChain.SufiPlatform.OpenIddict.EntityFrameworkCore;

public static class SufiOpenIddictDbContextModelCreatingExtensions
{
    public static void ConfigureSufiOpenIddict(
        this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        if (builder.IsTenantOnlyDatabase())
        {
            return;
        }

        builder.Entity<OpenIddictApplication>(b =>
        {
            b.ToTable(SufiOpenIddictDbProperties.DbTablePrefix + "Applications", SufiOpenIddictDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.HasIndex(x => x.ClientId);
                //.IsUnique();

            b.Property(x => x.ApplicationType)
                .HasMaxLength(OpenIddictApplicationConsts.ApplicationTypeMaxLength);

            b.Property(x => x.ClientId)
                .HasMaxLength(OpenIddictApplicationConsts.ClientIdMaxLength);

            b.Property(x => x.ConsentType)
                .HasMaxLength(OpenIddictApplicationConsts.ConsentTypeMaxLength);

            b.Property(x => x.ClientType)
                .HasMaxLength(OpenIddictApplicationConsts.ClientTypeMaxLength);

            b.ApplyObjectExtensionMappings();
        });

        builder.Entity<OpenIddictAuthorization>(b =>
        {
            b.ToTable(SufiOpenIddictDbProperties.DbTablePrefix + "Authorizations", SufiOpenIddictDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.HasIndex(x => new
            {
                x.ApplicationId,
                x.Status,
                x.Subject,
                x.Type
            });

            b.Property(x => x.Status)
                .HasMaxLength(OpenIddictAuthorizationConsts.StatusMaxLength);

            b.Property(x => x.Subject)
                .HasMaxLength(OpenIddictAuthorizationConsts.SubjectMaxLength);

            b.Property(x => x.Type)
                .HasMaxLength(OpenIddictAuthorizationConsts.TypeMaxLength);

            b.HasOne<OpenIddictApplication>().WithMany().HasForeignKey(x => x.ApplicationId).IsRequired(false);

            b.ApplyObjectExtensionMappings();
        });

        builder.Entity<OpenIddictScope>(b =>
        {
            b.ToTable(SufiOpenIddictDbProperties.DbTablePrefix + "Scopes", SufiOpenIddictDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.HasIndex(x => x.Name);
                //.IsUnique();

            b.Property(x => x.Name)
                .HasMaxLength(OpenIddictScopeConsts.NameMaxLength);

            b.ApplyObjectExtensionMappings();
        });

        builder.Entity<OpenIddictToken>(b =>
        {
            b.ToTable(SufiOpenIddictDbProperties.DbTablePrefix + "Tokens", SufiOpenIddictDbProperties.DbSchema);

            b.ConfigureByConvention();

            b.HasIndex(x => x.ReferenceId);
                //.IsUnique();

            b.HasIndex(x => new
            {
                x.ApplicationId,
                x.Status,
                x.Subject,
                x.Type
            });

            b.Property(x => x.ReferenceId)
                .HasMaxLength(OpenIddictTokenConsts.ReferenceIdMaxLength);

            b.Property(x => x.Status)
                .HasMaxLength(OpenIddictTokenConsts.StatusMaxLength);

            b.Property(x => x.Subject)
                .HasMaxLength(OpenIddictTokenConsts.SubjectMaxLength);

            b.Property(x => x.Type)
                .HasMaxLength(OpenIddictTokenConsts.TypeMaxLength);

            b.HasOne<OpenIddictApplication>().WithMany().HasForeignKey(x => x.ApplicationId).IsRequired(false);
            b.HasOne<OpenIddictAuthorization>().WithMany().HasForeignKey(x => x.AuthorizationId).IsRequired(false);

            b.ApplyObjectExtensionMappings();
        });

    }
}
