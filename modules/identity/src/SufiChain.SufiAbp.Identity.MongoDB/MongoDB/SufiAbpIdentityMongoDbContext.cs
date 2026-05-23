using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.Identity.MongoDB;

[ConnectionStringName(SufiAbpIdentityDbProperties.ConnectionStringName)]
public class SufiAbpIdentityMongoDbContext : AbpMongoDbContext, ISufiAbpIdentityMongoDbContext
{
    public IMongoCollection<IdentityUser> Users => Collection<IdentityUser>();

    public IMongoCollection<IdentityRole> Roles => Collection<IdentityRole>();

    public IMongoCollection<IdentityClaimType> ClaimTypes => Collection<IdentityClaimType>();

    public IMongoCollection<OrganizationUnit> OrganizationUnits => Collection<OrganizationUnit>();

    public IMongoCollection<IdentitySecurityLog> SecurityLogs => Collection<IdentitySecurityLog>();

    public IMongoCollection<IdentityLinkUser> LinkUsers => Collection<IdentityLinkUser>();

    public IMongoCollection<IdentityUserDelegation> UserDelegations => Collection<IdentityUserDelegation>();

    public IMongoCollection<IdentitySession> Sessions => Collection<IdentitySession>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.ConfigureIdentity();
    }
}
