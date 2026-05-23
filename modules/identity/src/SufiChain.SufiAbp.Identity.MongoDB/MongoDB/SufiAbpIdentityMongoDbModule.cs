using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Identity.MongoDB.Repositories;
using SufiChain.SufiAbp.Users;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Identity.MongoDB;

[DependsOn(
    typeof(SufiAbpIdentityDomainModule),
    typeof(SufiAbpUsersMongoDbModule)
    )]
public class SufiAbpIdentityMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<SufiAbpIdentityMongoDbContext>(options =>
        {
            options.AddRepository<IdentityUser, MongoIdentityUserRepository>();
            options.AddRepository<IdentityRole, MongoIdentityRoleRepository>();
            options.AddRepository<IdentityClaimType, MongoIdentityClaimTypeRepository>();
            options.AddRepository<OrganizationUnit, MongoOrganizationUnitRepository>();
            options.AddRepository<IdentitySecurityLog, MongoIdentitySecurityLogRepository>();
            options.AddRepository<IdentityLinkUser, MongoIdentityLinkUserRepository>();
            options.AddRepository<IdentityUserDelegation, MongoIdentityUserDelegationRepository>();
            options.AddRepository<IdentitySession, MongoIdentitySessionRepository>();
        });
    }
}
