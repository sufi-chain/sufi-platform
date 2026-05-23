using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Caching;
using SufiChain.SufiAbp.Data;
using SufiChain.SufiAbp.Ddd;
using SufiChain.SufiAbp.Mapperly;
using SufiChain.SufiAbp.MultiTenancy;
using SufiChain.SufiAbp.ObjectExtending;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectExtending.Modularity;
using Volo.Abp.Threading;

namespace SufiChain.SufiAbp.TenantManagement;

[DependsOn(typeof(SufiAbpMultiTenancyModule))]
[DependsOn(typeof(SufiAbpTenantManagementDomainSharedModule))]
[DependsOn(typeof(SufiAbpDataModule))]
[DependsOn(typeof(SufiAbpDddDomainModule))]
[DependsOn(typeof(SufiAbpMapperlyModule))]
[DependsOn(typeof(SufiAbpCachingModule))]
public class SufiAbpTenantManagementDomainModule : AbpModule
{
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<SufiAbpTenantManagementDomainModule>();
        context.Services.AddTransient<ITenantValidator, SufiAbpTenantValidator>();

        Configure<AbpDistributedEntityEventOptions>(options =>
        {
        });
    }

    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        OneTimeRunner.Run(() =>
        {
            ModuleExtensionConfigurationHelper.ApplyEntityConfigurationToEntity(
                TenantManagementModuleExtensionConsts.ModuleName,
                TenantManagementModuleExtensionConsts.EntityNames.Tenant,
                typeof(Tenant)
            );
        });
    }
}
