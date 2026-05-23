using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiAbp.UI.Alerts;
using SufiChain.SufiAbp.UI.Authorization;
using SufiChain.SufiAbp.UI.Bundling;
using SufiChain.SufiAbp.UI.Layout;
using SufiChain.SufiAbp.UI.LayoutHooks;
using SufiChain.SufiAbp.UI.Localization;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.UI.PageToolbars;
using SufiChain.SufiAbp.UI.Services.Alerts;
using SufiChain.SufiAbp.UI.Services.Authorization;
using SufiChain.SufiAbp.UI.Services.Bundling;
using SufiChain.SufiAbp.UI.Services.Layout;
using SufiChain.SufiAbp.UI.Services.LayoutHooks;
using SufiChain.SufiAbp.UI.Services.Localization;
using SufiChain.SufiAbp.UI.Services.Navigation;
using SufiChain.SufiAbp.UI.Services.PageToolbars;
using SufiChain.SufiAbp.UI.Services.Theming;
using SufiChain.SufiAbp.UI.Services.Toolbars;
using SufiChain.SufiAbp.UI.Services.Users;
using SufiChain.SufiAbp.UI.MultiTenancy;
using SufiChain.SufiAbp.UI.Services.MultiTenancy;
using SufiChain.SufiAbp.UI.Theming;
using SufiChain.SufiAbp.UI.Toolbars;
using SufiChain.SufiAbp.UI.Users;

namespace SufiChain.SufiAbp.UI.Services.DependencyInjection;

/// <summary>
/// Extension methods for registering UI services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all UI services to the service collection.
    /// </summary>
    public static IServiceCollection AddSufiAbpUIServices(this IServiceCollection services)
    {
        services.Configure<SufiAbpLocalizationOptions>(_ => { });

        // Note: IBrandingProvider is registered as Singleton in SufiChain.SufiAbp.UI.Abstractions
        // Use AddBrandingProvider<T> to override with a custom implementation

        // Theming (unified for MVC and Blazor)
        services.AddTransient<IThemeSelector, DefaultThemeSelector>();
        services.AddScoped<IThemeManager, DefaultThemeManager>();

        // Navigation
        services.AddScoped<IMenuManager, DefaultMenuManager>();

        // Toolbars
        services.AddScoped<IToolbarManager, DefaultToolbarManager>();

        // Page Toolbars
        services.AddScoped<IPageToolbarManager, DefaultPageToolbarManager>();

        // Layout
        services.AddScoped<IPageLayout, DefaultPageLayout>();
        services.AddScoped<IBreadcrumbService, DefaultBreadcrumbService>();

        // Alerts
        services.AddScoped<IAlertManager, DefaultAlertManager>();

        // Layout Hooks
        services.AddSingleton<ILayoutHookManager, DefaultLayoutHookManager>();

        // Bundling
        services.AddSingleton<IComponentBundleManager, DefaultComponentBundleManager>();

        // Authorization (default: always allow, replace with a product-specific implementation for real authorization)
        services.TryAddScoped<ISufiAbpPermissionChecker, AlwaysAllowPermissionChecker>();

        // Current user (default: anonymous; replace in an authentication-specific UI package if needed)
        services.TryAddScoped<ICurrentUserAccessor, DefaultCurrentUserAccessor>();

        // Localization
        services.TryAddTransient<ILanguageProvider, DefaultLanguageProvider>();

        // Tenant selector visibility (default: never show; replace when multi-tenant)
        services.TryAddScoped<ITenantSelectorVisibilityService, DefaultTenantSelectorVisibilityService>();

        // Tenant lookup (default: empty list; replace with tenant-management implementation when module is loaded)
        services.TryAddScoped<ITenantLookupService, DefaultTenantLookupService>();

        return services;
    }
}
