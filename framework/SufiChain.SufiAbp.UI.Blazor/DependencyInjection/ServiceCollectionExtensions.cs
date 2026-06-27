using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiAbp.UI.Blazor.Circuit;
using SufiChain.SufiAbp.UI.BlockUi;
using SufiChain.SufiAbp.UI.Blazor.BlockUi;
using SufiChain.SufiAbp.UI.Blazor.Browser;
using SufiChain.SufiAbp.UI.Blazor.ExceptionHandling;
using SufiChain.SufiAbp.UI.Blazor.Messages;
using SufiChain.SufiAbp.UI.Blazor.Notifications;
using SufiChain.SufiAbp.UI.Blazor.Progression;
using SufiChain.SufiAbp.UI.Blazor.Theming;
using SufiChain.SufiAbp.UI.Blazor.Timing;
using SufiChain.SufiAbp.UI.Browser;
using SufiChain.SufiAbp.UI.ExceptionHandling;
using SufiChain.SufiAbp.UI.Messages;
using SufiChain.SufiAbp.UI.Blazor.MultiTenancy;
using SufiChain.SufiAbp.UI.MultiTenancy;
using SufiChain.SufiAbp.UI.Notifications;
using SufiChain.SufiAbp.UI.Progression;
using SufiChain.SufiAbp.UI.Theming;
using SufiChain.SufiAbp.UI.Timing;

namespace SufiChain.SufiAbp.UI.Blazor.DependencyInjection;

/// <summary>
/// Extension methods for registering Blazor UI services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Blazor UI services to the service collection.
    /// </summary>
    public static IServiceCollection AddSufiAbpUIBlazor(
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
