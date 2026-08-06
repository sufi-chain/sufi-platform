using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.Caching;
using Volo.Abp.Modularity;
using Volo.Abp.Domain;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.Mapperly;
using Volo.Abp.Security.Claims;
using Volo.Abp.Settings;
using SufiChain.SufiPlatform.Users;

namespace SufiChain.SufiPlatform.Identity;

[DependsOn(
    typeof(SufiIdentityDomainSharedModule),
    typeof(SufiUsersDomainModule),
    typeof(AbpDddDomainModule),
    typeof(AbpMapperlyModule),
    typeof(AbpSettingsModule),
    typeof(AbpCachingModule)
)]
public class SufiIdentityDomainModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsRemoteRefreshEnabled = false;
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<SufiIdentityDomainModule>();

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
            .AddRoles<IdentityRole>()
            .AddPasswordValidator<SufiExtendedPasswordValidator>();

        context.Services.AddObjectAccessor(identityBuilder);
        context.Services.ExecutePreConfiguredActions(identityBuilder);

        Configure<AbpDistributedEntityEventOptions>(options =>
        {
            options.EtoMappings.Add<IdentityUser, UserEto>(typeof(SufiIdentityDomainModule));
            options.AutoEventSelectors.Add<IdentityUser>();
        });

        Configure<IdentityOptions>(options =>
        {
            options.ClaimsIdentity.UserIdClaimType = AbpClaimTypes.UserId;
            options.ClaimsIdentity.UserNameClaimType = AbpClaimTypes.UserName;
            options.ClaimsIdentity.RoleClaimType = AbpClaimTypes.Role;
            options.ClaimsIdentity.EmailClaimType = AbpClaimTypes.Email;
        });

        context.Services.AddAbpDynamicOptions<IdentityOptions, SufiIdentityOptionsManager>();
    }
}
