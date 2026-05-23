using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.Modularity;
using Volo.Abp.Domain;
using Volo.Abp.Security.Claims;
using SufiChain.SufiAbp.Ddd;
using SufiChain.SufiAbp.Users;

namespace SufiChain.SufiAbp.Identity;

[DependsOn(
    typeof(SufiAbpIdentityDomainSharedModule),
    typeof(SufiAbpUsersDomainModule),
    typeof(SufiAbpDddDomainModule)
)]
public class SufiAbpIdentityDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.TryAddScoped<IdentityRoleManager>();
        context.Services.TryAddScoped<RoleManager<IdentityRole>>(provider =>
            provider.GetRequiredService<IdentityRoleManager>());

        context.Services.TryAddScoped<IdentityUserManager>();
        context.Services.TryAddScoped<UserManager<IdentityUser>>(provider =>
            provider.GetRequiredService<IdentityUserManager>());

        context.Services.TryAddScoped<IdentityUserStore>();
        context.Services.TryAddScoped<IUserStore<IdentityUser>>(provider =>
            provider.GetRequiredService<IdentityUserStore>());

        context.Services.TryAddScoped<IdentityRoleStore>();
        context.Services.TryAddScoped<IRoleStore<IdentityRole>>(provider =>
            provider.GetRequiredService<IdentityRoleStore>());

        var identityBuilder = context.Services
            .AddIdentityCore<IdentityUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>();

        context.Services.AddObjectAccessor(identityBuilder);
        context.Services.ExecutePreConfiguredActions(identityBuilder);

        Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.UserIdClaimType = AbpClaimTypes.UserId;
            options.ClaimsIdentity.UserNameClaimType = AbpClaimTypes.UserName;
            options.ClaimsIdentity.RoleClaimType = AbpClaimTypes.Role;
            options.ClaimsIdentity.EmailClaimType = AbpClaimTypes.Email;
        });

        context.Services.AddAbpDynamicOptions<IdentityOptions, SufiAbpIdentityOptionsManager>();
    }
}
