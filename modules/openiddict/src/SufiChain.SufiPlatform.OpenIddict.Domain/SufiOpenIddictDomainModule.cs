using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Caching;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.ObjectExtending.Modularity;
using Volo.Abp.Threading;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.OpenIddict.Applications;
using SufiChain.SufiPlatform.OpenIddict.Authorizations;
using SufiChain.SufiPlatform.OpenIddict.Scopes;
using SufiChain.SufiPlatform.OpenIddict.Tokens;

namespace SufiChain.SufiPlatform.OpenIddict;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(SufiIdentityDomainModule),
    typeof(SufiOpenIddictDomainSharedModule),
    typeof(AbpDistributedLockingAbstractionsModule),
    typeof(AbpCachingModule),
    typeof(AbpGuidsModule),
    typeof(AbpMapperlyModule)
)]
public class SufiOpenIddictDomainModule : AbpModule
{
    private readonly static OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<SufiOpenIddictDomainModule>();

        AddOpenIddictCore(context.Services);

        Configure<AbpDistributedEntityEventOptions>(options =>
        {
            options.EtoMappings.Add<OpenIddictApplication, OpenIddictApplicationEto>(typeof(SufiOpenIddictDomainModule));
            options.AutoEventSelectors.Add<OpenIddictApplication>();
        });
    }

    private void AddOpenIddictCore(IServiceCollection services)
    {
        var openIddictBuilder = services.AddOpenIddict()
            .AddCore(builder =>
            {
                builder
                    .SetDefaultApplicationEntity<OpenIddictApplicationModel>()
                    .SetDefaultAuthorizationEntity<OpenIddictAuthorizationModel>()
                    .SetDefaultScopeEntity<OpenIddictScopeModel>()
                    .SetDefaultTokenEntity<OpenIddictTokenModel>();

                builder
                    .ReplaceApplicationStore<OpenIddictApplicationModel, OpenIddictApplicationStore>()
                    .ReplaceAuthorizationStore<OpenIddictAuthorizationModel, OpenIddictAuthorizationStore>()
                    .ReplaceScopeStore<OpenIddictScopeModel, OpenIddictScopeStore>()
                    .ReplaceTokenStore<OpenIddictTokenModel, OpenIddictTokenStore>();

                services.ExecutePreConfiguredActions(builder);
            });

        services.ExecutePreConfiguredActions(openIddictBuilder);
    }

    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        OneTimeRunner.Run(() =>
        {
            ModuleExtensionConfigurationHelper.ApplyEntityConfigurationToEntity(
                OpenIddictModuleExtensionConsts.ModuleName,
                OpenIddictModuleExtensionConsts.EntityNames.Application,
                typeof(OpenIddictApplication)
            );

            ModuleExtensionConfigurationHelper.ApplyEntityConfigurationToEntity(
                OpenIddictModuleExtensionConsts.ModuleName,
                OpenIddictModuleExtensionConsts.EntityNames.Application,
                typeof(OpenIddictApplicationModel)
            );

            ModuleExtensionConfigurationHelper.ApplyEntityConfigurationToEntity(
                OpenIddictModuleExtensionConsts.ModuleName,
                OpenIddictModuleExtensionConsts.EntityNames.Authorization,
                typeof(OpenIddictAuthorization)
            );

            ModuleExtensionConfigurationHelper.ApplyEntityConfigurationToEntity(
                OpenIddictModuleExtensionConsts.ModuleName,
                OpenIddictModuleExtensionConsts.EntityNames.Authorization,
                typeof(OpenIddictAuthorizationModel)
            );

            ModuleExtensionConfigurationHelper.ApplyEntityConfigurationToEntity(
                OpenIddictModuleExtensionConsts.ModuleName,
                OpenIddictModuleExtensionConsts.EntityNames.Scope,
                typeof(OpenIddictScope)
            );

            ModuleExtensionConfigurationHelper.ApplyEntityConfigurationToEntity(
                OpenIddictModuleExtensionConsts.ModuleName,
                OpenIddictModuleExtensionConsts.EntityNames.Scope,
                typeof(OpenIddictScopeModel)
            );

            ModuleExtensionConfigurationHelper.ApplyEntityConfigurationToEntity(
                OpenIddictModuleExtensionConsts.ModuleName,
                OpenIddictModuleExtensionConsts.EntityNames.Token,
                typeof(OpenIddictToken)
            );

            ModuleExtensionConfigurationHelper.ApplyEntityConfigurationToEntity(
                OpenIddictModuleExtensionConsts.ModuleName,
                OpenIddictModuleExtensionConsts.EntityNames.Token,
                typeof(OpenIddictTokenModel)
            );
        });
    }
}
