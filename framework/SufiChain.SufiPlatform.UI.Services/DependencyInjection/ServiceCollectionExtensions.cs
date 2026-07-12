using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.UI.Alerts;
using SufiChain.SufiPlatform.UI.Authorization;
using SufiChain.SufiPlatform.UI.Bundling;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiPlatform.UI.LayoutHooks;
using SufiChain.SufiPlatform.UI.Localization;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.PageToolbars;
using SufiChain.SufiPlatform.UI.Services.Alerts;
using SufiChain.SufiPlatform.UI.Services.Authorization;
using SufiChain.SufiPlatform.UI.Services.Bundling;
using SufiChain.SufiPlatform.UI.Services.Layout;
using SufiChain.SufiPlatform.UI.Services.LayoutHooks;
using SufiChain.SufiPlatform.UI.Services.Localization;
using SufiChain.SufiPlatform.UI.Services.Navigation;
using SufiChain.SufiPlatform.UI.Services.PageToolbars;
using SufiChain.SufiPlatform.UI.Services.Theming;
using SufiChain.SufiPlatform.UI.Services.Toolbars;
using SufiChain.SufiPlatform.UI.Services.Users;
using SufiChain.SufiPlatform.UI.MultiTenancy;
using SufiChain.SufiPlatform.UI.Services.MultiTenancy;
using SufiChain.SufiPlatform.UI.Theming;
using SufiChain.SufiPlatform.UI.Toolbars;
using SufiChain.SufiPlatform.UI.Users;

namespace SufiChain.SufiPlatform.UI.Services.DependencyInjection;

/// <summary>
/// Extension methods for registering UI services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all UI services to the service collection.
    /// </summary>
    public static IServiceCollection AddSufiUIServices(this IServiceCollection services)
    {
        // Note: IBrandingProvider is registered as Singleton in SufiChain.SufiPlatform.UI.Abstractions
        // Use AddBrandingProvider<T> to override with a custom implementation
        // Languages: configure once via AbpLocalizationOptions (DefaultLanguageProvider reads that)

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
        services.TryAddScoped<ISufiPermissionChecker, AlwaysAllowPermissionChecker>();

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
