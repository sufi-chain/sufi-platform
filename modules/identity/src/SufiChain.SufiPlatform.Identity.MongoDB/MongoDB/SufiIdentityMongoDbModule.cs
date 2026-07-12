using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Identity.MongoDB.Repositories;
using SufiChain.SufiPlatform.Users;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Identity.MongoDB;

[DependsOn(
    typeof(SufiIdentityDomainModule),
    typeof(SufiUsersMongoDbModule)
    )]
public class SufiIdentityMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<SufiIdentityMongoDbContext>(options =>
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
