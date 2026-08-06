using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.UI.Blazor.Circuit;
using SufiChain.SufiPlatform.UI.BlockUi;
using SufiChain.SufiPlatform.UI.Blazor.BlockUi;
using SufiChain.SufiPlatform.UI.Blazor.Browser;
using SufiChain.SufiPlatform.UI.Blazor.ExceptionHandling;
using SufiChain.SufiPlatform.UI.Blazor.Messages;
using SufiChain.SufiPlatform.UI.Blazor.Notifications;
using SufiChain.SufiPlatform.UI.Blazor.Progression;
using SufiChain.SufiPlatform.UI.Blazor.Theming;
using SufiChain.SufiPlatform.UI.Blazor.Timing;
using SufiChain.SufiPlatform.UI.Browser;
using SufiChain.SufiPlatform.UI.ExceptionHandling;
using SufiChain.SufiPlatform.UI.Messages;
using SufiChain.SufiPlatform.UI.Blazor.MultiTenancy;
using SufiChain.SufiPlatform.UI.MultiTenancy;
using SufiChain.SufiPlatform.UI.Notifications;
using SufiChain.SufiPlatform.UI.Progression;
using SufiChain.SufiPlatform.UI.Theming;
using SufiChain.SufiPlatform.UI.Timing;

namespace SufiChain.SufiPlatform.UI.Blazor.DependencyInjection;

/// <summary>
/// Extension methods for registering Blazor UI services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Blazor UI services to the service collection.
    /// </summary>
    public static IServiceCollection AddSufiUIBlazor(
        this IServiceCollection services,
        Action<DynamicLayoutComponentOptions>? configureDynamicComponents = null)
    {
        // Configure dynamic layout components
        if (configureDynamicComponents != null)
        {
            services.Configure(configureDynamicComponents);
        }
        else
        {
            services.Configure<DynamicLayoutComponentOptions>(_ => { });
        }

        // Register UI services as Singleton with circuit-based isolation
        services.TryAddSingleton<IUiMessageService, SufiBlazorMessageService>();
        services.TryAddSingleton<IBlazorCircuitIdAccessor, NullBlazorCircuitIdAccessor>();
        services.TryAddSingleton<IUiNotificationService, SufiBlazorNotificationService>();
        services.TryAddSingleton<IBlockUiService, SufiBlazorBlockUiService>();
        services.TryAddSingleton<IUiPageProgressService, SufiBlazorPageProgressService>();
        services.TryAddScoped<IUserExceptionInformer, DefaultUserExceptionInformer>();

        // Register browser services
        services.TryAddScoped<ICookieService, BrowserCookieService>();
        services.TryAddScoped<ILocalStorageService, BrowserLocalStorageService>();
        services.TryAddScoped<ISessionStorageService, BrowserSessionStorageService>();

        // Register timing services
        services.TryAddSingleton<IClock, DefaultClock>();

        // Register multi-tenancy services
        services.TryAddScoped<ICurrentTenant, DefaultCurrentTenant>();
        services.TryAddScoped<ITenantSwitchService, BlazorTenantSwitchService>();
        services.Configure<TenantSwitchOptions>(_ => { });

        // Register theming services
        services.TryAddScoped<IThemeSwitchService, ThemeSwitchService>();

        return services;
    }
}
