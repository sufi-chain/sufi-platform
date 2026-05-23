using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.Identity.MongoDB;

public static class SufiAbpIdentityMongoDbContextExtensions
{
    public static void ConfigureIdentity(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<IdentityUser>(b =>
        {
            b.CollectionName = SufiAbpIdentityDbProperties.DbTablePrefix + "Users";
        });

        builder.Entity<IdentityRole>(b =>
        {
            b.CollectionName = SufiAbpIdentityDbProperties.DbTablePrefix + "Roles";
        });

        builder.Entity<IdentityClaimType>(b =>
        {
            b.CollectionName = SufiAbpIdentityDbProperties.DbTablePrefix + "ClaimTypes";
        });

        builder.Entity<OrganizationUnit>(b =>
        {
            b.CollectionName = SufiAbpIdentityDbProperties.DbTablePrefix + "OrganizationUnits";
        });

        builder.Entity<IdentitySecurityLog>(b =>
        {
            b.CollectionName = SufiAbpIdentityDbProperties.DbTablePrefix + "SecurityLogs";
        });

        builder.Entity<IdentityLinkUser>(b =>
        {
            b.CollectionName = SufiAbpIdentityDbProperties.DbTablePrefix + "LinkUsers";
        });

        builder.Entity<IdentityUserDelegation>(b =>
        {
            b.CollectionName = SufiAbpIdentityDbProperties.DbTablePrefix + "UserDelegations";
        });

        builder.Entity<IdentitySession>(b =>
        {
            b.CollectionName = SufiAbpIdentityDbProperties.DbTablePrefix + "Sessions";
        });
    }
}
