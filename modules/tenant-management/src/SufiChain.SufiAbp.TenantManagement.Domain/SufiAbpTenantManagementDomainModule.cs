using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Data;
using SufiChain.SufiAbp.ObjectExtending;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectExtending.Modularity;
using Volo.Abp.Threading;
using Volo.Abp.Domain;

using Volo.Abp.Mapperly;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Caching;
namespace SufiChain.SufiAbp.TenantManagement;

[DependsOn(typeof(AbpMultiTenancyModule))]
[DependsOn(typeof(SufiAbpTenantManagementDomainSharedModule))]
[DependsOn(typeof(SufiAbpDataModule))]
[DependsOn(typeof(AbpDddDomainModule))]
[DependsOn(typeof(AbpMapperlyModule))]
[DependsOn(typeof(AbpCachingModule))]
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
