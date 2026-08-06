using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.ObjectExtending;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectExtending.Modularity;
using Volo.Abp.Threading;
using Volo.Abp.Domain;

using Volo.Abp.Mapperly;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Caching;
namespace SufiChain.SufiPlatform.Tenants;

[DependsOn(typeof(AbpMultiTenancyModule))]
[DependsOn(typeof(SufiTenantsDomainSharedModule))]
[DependsOn(typeof(SufiDataModule))]
[DependsOn(typeof(AbpDddDomainModule))]
[DependsOn(typeof(AbpMapperlyModule))]
[DependsOn(typeof(AbpCachingModule))]
public class SufiTenantsDomainModule : AbpModule
{
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<SufiTenantsDomainModule>();
        context.Services.AddTransient<ITenantValidator, SufiTenantValidator>();

        Configure<AbpDistributedEntityEventOptions>(options =>
        {
        });
    }

    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        OneTimeRunner.Run(() =>
        {
            ModuleExtensionConfigurationHelper.ApplyEntityConfigurationToEntity(
                TenantsModuleExtensionConsts.ModuleName,
                TenantsModuleExtensionConsts.EntityNames.Tenant,
                typeof(Tenant)
            );
        });
    }
}
