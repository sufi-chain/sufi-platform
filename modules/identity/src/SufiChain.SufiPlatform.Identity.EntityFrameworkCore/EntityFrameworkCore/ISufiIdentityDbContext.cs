using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Identity.EntityFrameworkCore;

[ConnectionStringName(SufiIdentityDbProperties.ConnectionStringName)]
public interface ISufiIdentityDbContext : IEfCoreDbContext
{
    DbSet<IdentityUser> Users { get; }
    DbSet<IdentityRole> Roles { get; }
    DbSet<IdentityClaimType> ClaimTypes { get; }
    DbSet<OrganizationUnit> OrganizationUnits { get; }
    DbSet<IdentitySecurityLog> SecurityLogs { get; }
    DbSet<IdentityLinkUser> LinkUsers { get; }
    DbSet<IdentityUserDelegation> UserDelegations { get; }
    DbSet<IdentitySession> Sessions { get; }
}
