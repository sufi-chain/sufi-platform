using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Users;
using Volo.Abp.Modularity;

using Volo.Abp.EntityFrameworkCore;
namespace SufiChain.SufiAbp.Identity.EntityFrameworkCore;

[DependsOn(
    typeof(SufiAbpIdentityDomainModule),
    typeof(AbpEntityFrameworkCoreModule),
    typeof(SufiAbpUsersEntityFrameworkCoreModule)
)]
public class SufiAbpIdentityEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<SufiAbpIdentityDbContext>(options =>
        {
            options.AddRepository<IdentityUser, EfCoreIdentityUserRepository>();
            options.AddRepository<IdentityRole, EfCoreIdentityRoleRepository>();
            options.AddRepository<IdentityClaimType, EfCoreIdentityClaimTypeRepository>();
            options.AddRepository<OrganizationUnit, EfCoreOrganizationUnitRepository>();
            options.AddRepository<IdentitySecurityLog, EfCoreIdentitySecurityLogRepository>();
            options.AddRepository<IdentityLinkUser, EfCoreIdentityLinkUserRepository>();
            options.AddRepository<IdentityUserDelegation, EfCoreIdentityUserDelegationRepository>();
            options.AddRepository<IdentitySession, EfCoreIdentitySessionRepository>();
        });
    }
}
