using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Identity.MongoDB;

public static class SufiIdentityMongoDbContextExtensions
{
    public static void ConfigureIdentity(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<IdentityUser>(b =>
        {
            b.CollectionName = SufiIdentityDbProperties.DbTablePrefix + "Users";
        });

        builder.Entity<IdentityRole>(b =>
        {
            b.CollectionName = SufiIdentityDbProperties.DbTablePrefix + "Roles";
        });

        builder.Entity<IdentityClaimType>(b =>
        {
            b.CollectionName = SufiIdentityDbProperties.DbTablePrefix + "ClaimTypes";
        });

        builder.Entity<OrganizationUnit>(b =>
        {
            b.CollectionName = SufiIdentityDbProperties.DbTablePrefix + "OrganizationUnits";
        });

        builder.Entity<IdentitySecurityLog>(b =>
        {
            b.CollectionName = SufiIdentityDbProperties.DbTablePrefix + "SecurityLogs";
        });

        builder.Entity<IdentityLinkUser>(b =>
        {
            b.CollectionName = SufiIdentityDbProperties.DbTablePrefix + "LinkUsers";
        });

        builder.Entity<IdentityUserDelegation>(b =>
        {
            b.CollectionName = SufiIdentityDbProperties.DbTablePrefix + "UserDelegations";
        });

        builder.Entity<IdentitySession>(b =>
        {
            b.CollectionName = SufiIdentityDbProperties.DbTablePrefix + "Sessions";
        });
    }
}
