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
namespace SufiChain.SufiPlatform.Settings;

[DependsOn(
    typeof(AbpSettingsModule),
    typeof(AbpDddDomainModule),
    typeof(SufiSettingsDomainSharedModule),
    typeof(AbpCachingModule)
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
