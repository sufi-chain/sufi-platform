using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.Settings;
using Volo.Abp.Threading;
using Volo.Abp.Domain;

using Volo.Abp.Caching;
namespace SufiChain.SufiAbp.SettingManagement;

[DependsOn(
    typeof(AbpSettingsModule),
    typeof(AbpDddDomainModule),
    typeof(SufiAbpSettingManagementDomainSharedModule),
    typeof(AbpCachingModule)
    )]
public class SufiAbpSettingManagementDomainModule : AbpModule
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Replace(ServiceDescriptor.Transient<ISettingStore, SettingStore>());

        Configure<SettingManagementOptions>(options =>
        {
            options.Providers.Add<DefaultValueSettingManagementProvider>();
            options.Providers.Add<ConfigurationSettingManagementProvider>();
            options.Providers.Add<GlobalSettingManagementProvider>();
            options.Providers.Add<TenantSettingManagementProvider>();
            options.Providers.Add<UserSettingManagementProvider>();
        });

        if (context.Services.IsDataMigrationEnvironment())
        {
            Configure<SettingManagementOptions>(options =>
            {
                options.SaveStaticSettingsToDatabase = false;
                options.IsDynamicSettingStoreEnabled = false;
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
        var initializer = rootServiceProvider.GetService<SettingDynamicInitializer>();
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
