using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.UI.Authorization;
using SufiChain.SufiPlatform.UI.Blazor.Circuit;
using SufiChain.SufiPlatform.UI.Blazor.Server.Circuit;
using SufiChain.SufiPlatform.UI.Services.Authorization;

namespace SufiChain.SufiPlatform.UI.Blazor.Server.DependencyInjection;

/// <summary>
/// Blazor Server specific service registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers ABP-backed permission checking for menu and toolbar filtering.
    /// Call before <see cref="SufiChain.SufiPlatform.UI.Services.DependencyInjection.ServiceCollectionExtensions.AddSufiUIServices"/>.
    /// </summary>
    public static IServiceCollection AddSufiBlazorServerAuthorization(this IServiceCollection services)
    {
        services.Replace(ServiceDescriptor.Scoped<ISufiPermissionChecker, AbpPermissionCheckerAdapter>());
        return services;
    }

    /// <summary>
    /// Registers circuit-aware overlay services for Blazor Server. Replaces the default
    /// <see cref="IBlazorCircuitIdAccessor"/> with the Server implementation and adds
    /// a <see cref="CircuitHandler"/> that sets the current circuit ID per inbound activity.
    /// This isolates toasts, block UI, and other overlay components per user/session.
    /// Call this after <see cref="SufiChain.SufiPlatform.UI.Blazor.DependencyInjection.ServiceCollectionExtensions.AddSufiUIBlazor"/>.
    /// </summary>
    public static IServiceCollection AddSufiBlazorServerCircuitServices(this IServiceCollection services)
    {
        services.Replace(ServiceDescriptor.Singleton<IBlazorCircuitIdAccessor, BlazorServerCircuitIdAccessor>());
        services.AddScoped<CircuitHandler, SufiBlazorCircuitHandler>();
        return services;
    }
}
