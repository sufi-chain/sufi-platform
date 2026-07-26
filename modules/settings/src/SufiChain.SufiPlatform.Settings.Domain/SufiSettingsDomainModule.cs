using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain;
using Volo.Abp.Json;
using Volo.Abp.Modularity;
using Volo.Abp.Settings;
using Volo.Abp.Threading;

namespace SufiChain.SufiPlatform.Settings;

[DependsOn(
    typeof(AbpSettingsModule),
    typeof(AbpDddDomainModule),
    typeof(SufiSettingsDomainSharedModule),
    typeof(AbpCachingModule),
    typeof(AbpDistributedLockingAbstractionsModule),
    typeof(AbpJsonModule)
    )]
public class SufiSettingsDomainModule : AbpModule
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Replace(ServiceDescriptor.Transient<ISettingStore, SettingStore>());

        Configure<SettingsOptions>(options =>
        {
            options.Providers.Add<DefaultValueSettingsProvider>();
            options.Providers.Add<ConfigurationSettingsProvider>();
            options.Providers.Add<GlobalSettingsProvider>();
            options.Providers.Add<TenantSettingsProvider>();
            options.Providers.Add<UserSettingsProvider>();
        });

        if (context.Services.IsDataMigrationEnvironment())
        {
            Configure<SettingsOptions>(options =>
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
