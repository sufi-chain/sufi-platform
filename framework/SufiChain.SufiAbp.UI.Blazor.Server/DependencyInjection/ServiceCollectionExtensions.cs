using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiAbp.UI.Authorization;
using SufiChain.SufiAbp.UI.Blazor.Circuit;
using SufiChain.SufiAbp.UI.Blazor.Server.Circuit;
using SufiChain.SufiAbp.UI.Services.Authorization;

namespace SufiChain.SufiAbp.UI.Blazor.Server.DependencyInjection;

/// <summary>
/// Blazor Server specific service registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers ABP-backed permission checking for menu and toolbar filtering.
    /// Call before <see cref="SufiChain.SufiAbp.UI.Services.DependencyInjection.ServiceCollectionExtensions.AddSufiAbpUIServices"/>.
    /// </summary>
    public static IServiceCollection AddSufiAbpBlazorServerAuthorization(this IServiceCollection services)
    {
        services.Replace(ServiceDescriptor.Scoped<ISufiAbpPermissionChecker, AbpPermissionCheckerAdapter>());
        return services;
    }

    /// <summary>
    /// Registers circuit-aware overlay services for Blazor Server. Replaces the default
    /// <see cref="IBlazorCircuitIdAccessor"/> with the Server implementation and adds
    /// a <see cref="CircuitHandler"/> that sets the current circuit ID per inbound activity.
    /// This isolates toasts, block UI, and other overlay components per user/session.
    /// Call this after <see cref="SufiChain.SufiAbp.UI.Blazor.DependencyInjection.ServiceCollectionExtensions.AddSufiAbpUIBlazor"/>.
    /// </summary>
    public static IServiceCollection AddSufiAbpBlazorServerCircuitServices(this IServiceCollection services)
    {
        services.Replace(ServiceDescriptor.Singleton<IBlazorCircuitIdAccessor, BlazorServerCircuitIdAccessor>());
        services.AddScoped<CircuitHandler, SufiAbpBlazorCircuitHandler>();
        return services;
    }
}
