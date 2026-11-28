using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Tenants;

/// <summary>
/// Database-per-tenant connection resolution.
/// When a tenant is current, every named connection resolves to that tenant's
/// explicit row for the name, otherwise the tenant's Default. Host module-specific
/// connection strings are never used for a tenant that has a Default.
/// A shared tenant (no database name) without a Default uses host resolution.
/// A dedicated tenant (database name set) without a Default throws.
/// A missing tenant configuration throws. Neither case writes to the host database.
/// Connection-string names match without case, so a stored "default" still wins
/// over the host's named catalog.
/// </summary>
[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IConnectionStringResolver), typeof(MultiTenantConnectionStringResolver))]
public class SufiMultiTenantConnectionStringResolver : MultiTenantConnectionStringResolver
{
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<SufiMultiTenantConnectionStringResolver> _logger;

    public SufiMultiTenantConnectionStringResolver(
        IOptionsMonitor<AbpDbConnectionOptions> options,
        ICurrentTenant currentTenant,
        IServiceProvider serviceProvider)
        : base(options, currentTenant, serviceProvider)
    {
        _currentTenant = currentTenant;
        _logger = serviceProvider.GetService<ILogger<SufiMultiTenantConnectionStringResolver>>()
            ?? NullLogger<SufiMultiTenantConnectionStringResolver>.Instance;
    }

    public override async Task<string> ResolveAsync(string? connectionStringName = null)
    {
        if (_currentTenant.Id == null)
        {
            return await base.ResolveAsync(connectionStringName);
        }

        var tenantId = _currentTenant.Id.Value;
        var tenant = await FindTenantConfigurationAsync(tenantId);
        if (tenant == null)
        {
            throw new BusinessException(TenantsErrorCodes.TenantUnavailable)
                .WithData("TenantId", tenantId);
        }

        var resolved = ResolveForTenant(tenant, connectionStringName);
        if (resolved != null)
        {
            return resolved;
        }

        if (SufiTenantIsolation.IsDedicated(tenant))
        {
            throw new BusinessException(TenantsErrorCodes.TenantDatabaseNotProvisioned)
                .WithData("TenantId", tenantId);
        }

        _logger.LogWarning(
            "Tenant {TenantId} has no connection string for {ConnectionStringName}; using the host database.",
            tenantId,
            connectionStringName ?? ConnectionStrings.DefaultConnectionStringName);
        return await base.ResolveAsync(connectionStringName);
    }

    [Obsolete("Use ResolveAsync method.")]
    public override string Resolve(string? connectionStringName = null)
    {
        if (_currentTenant.Id == null)
        {
            return base.Resolve(connectionStringName);
        }

        var tenantId = _currentTenant.Id.Value;
        var tenant = FindTenantConfiguration(tenantId);
        if (tenant == null)
        {
            throw new BusinessException(TenantsErrorCodes.TenantUnavailable)
                .WithData("TenantId", tenantId);
        }

        var resolved = ResolveForTenant(tenant, connectionStringName);
        if (resolved != null)
        {
            return resolved;
        }

        if (SufiTenantIsolation.IsDedicated(tenant))
        {
            throw new BusinessException(TenantsErrorCodes.TenantDatabaseNotProvisioned)
                .WithData("TenantId", tenantId);
        }

        _logger.LogWarning(
            "Tenant {TenantId} has no connection string for {ConnectionStringName}; using the host database.",
            tenantId,
            connectionStringName ?? ConnectionStrings.DefaultConnectionStringName);
        return base.Resolve(connectionStringName);
    }

    /// <returns>The tenant's connection string, or <c>null</c> to use host resolution.</returns>
    internal static string? ResolveForTenant(
        TenantConfiguration? tenant,
        string? connectionStringName)
    {
        var connectionStrings = tenant?.ConnectionStrings;
        if (connectionStrings == null || connectionStrings.Count == 0)
        {
            return null;
        }

        if (string.Equals(connectionStringName, SufiTenantIsolation.DatabaseNameKey, StringComparison.OrdinalIgnoreCase))
        {
            connectionStringName = null;
        }

        var tenantDefault = FindConnectionString(connectionStrings, ConnectionStrings.DefaultConnectionStringName);
        var requestingDefault = connectionStringName.IsNullOrWhiteSpace()
            || string.Equals(
                connectionStringName,
                ConnectionStrings.DefaultConnectionStringName,
                StringComparison.OrdinalIgnoreCase);

        if (!requestingDefault)
        {
            var named = FindConnectionString(connectionStrings, connectionStringName!);
            if (!named.IsNullOrWhiteSpace())
            {
                return named;
            }
        }

        // Never fall back to the host's module-specific connection string when the tenant has a Default.
        return tenantDefault.IsNullOrWhiteSpace() ? null : tenantDefault;
    }

    private static string? FindConnectionString(ConnectionStrings connectionStrings, string name)
    {
        if (connectionStrings.TryGetValue(name, out var exact) && !exact.IsNullOrWhiteSpace())
        {
            return exact;
        }

        foreach (var pair in connectionStrings)
        {
            if (string.Equals(pair.Key, SufiTenantIsolation.DatabaseNameKey, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase)
                && !pair.Value.IsNullOrWhiteSpace())
            {
                return pair.Value;
            }
        }

        return null;
    }
}
