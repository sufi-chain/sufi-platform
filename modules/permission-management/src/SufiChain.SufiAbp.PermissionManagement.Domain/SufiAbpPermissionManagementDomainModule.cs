using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SufiChain.SufiAbp.Authorization;
using SufiChain.SufiAbp.Caching;
using SufiChain.SufiAbp.Ddd;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Json;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;

namespace SufiChain.SufiAbp.PermissionManagement;

[DependsOn(typeof(SufiAbpAuthorizationModule))]
[DependsOn(typeof(SufiAbpDddDomainModule))]
[DependsOn(typeof(SufiAbpPermissionManagementDomainSharedModule))]
[DependsOn(typeof(SufiAbpCachingModule))]
[DependsOn(typeof(AbpJsonModule))]
public class SufiAbpPermissionManagementDomainModule : AbpModule
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Replace(ServiceDescriptor.Singleton<IAbpDistributedLock, NullAbpDistributedLock>());

        if (context.Services.IsDataMigrationEnvironment())
        {
            Configure<PermissionManagementOptions>(options =>
            {
                options.SaveStaticPermissionsToDatabase = false;
                options.IsDynamicPermissionStoreEnabled = false;
            });
        }
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        AsyncHelper.RunSync(() => OnApplicationInitializationAsync(context));
    }

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        if (context.ServiceProvider.IsDataMigrationEnvironment())
        {
            return;
        }

        var rootServiceProvider = context.ServiceProvider.GetRequiredService<IRootServiceProvider>();
        var initializer = rootServiceProvider.GetService<PermissionDynamicInitializer>();
        if (initializer == null)
        {
            return;
        }

        await initializer.InitializeAsync(true, _cancellationTokenSource.Token);
    }

    public override Task OnApplicationShutdownAsync(ApplicationShutdownContext context)
    {
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }
}
