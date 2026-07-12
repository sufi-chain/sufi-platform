using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Identity.MongoDB;

[ConnectionStringName(SufiIdentityDbProperties.ConnectionStringName)]
public interface ISufiIdentityMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<IdentityUser> Users { get; }

    IMongoCollection<IdentityRole> Roles { get; }

    IMongoCollection<IdentityClaimType> ClaimTypes { get; }

    IMongoCollection<OrganizationUnit> OrganizationUnits { get; }

    IMongoCollection<IdentitySecurityLog> SecurityLogs { get; }

    IMongoCollection<IdentityLinkUser> LinkUsers { get; }

    IMongoCollection<IdentityUserDelegation> UserDelegations { get; }

    IMongoCollection<IdentitySession> Sessions { get; }
}
