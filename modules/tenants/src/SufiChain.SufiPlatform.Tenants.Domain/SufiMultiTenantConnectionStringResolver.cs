using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
/// connection strings are never used for a tenant.
/// </summary>
[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IConnectionStringResolver), typeof(MultiTenantConnectionStringResolver))]
public class SufiMultiTenantConnectionStringResolver : MultiTenantConnectionStringResolver
{
    private readonly ICurrentTenant _currentTenant;

    public SufiMultiTenantConnectionStringResolver(
        IOptionsMonitor<AbpDbConnectionOptions> options,
        ICurrentTenant currentTenant,
        IServiceProvider serviceProvider)
        : base(options, currentTenant, serviceProvider)
    {
        _currentTenant = currentTenant;
    }

    public override async Task<string> ResolveAsync(string? connectionStringName = null)
    {
        if (_currentTenant.Id == null)
        {
            return await base.ResolveAsync(connectionStringName);
        }

        var tenant = await FindTenantConfigurationAsync(_currentTenant.Id.Value);
        return ResolveForTenant(tenant, _currentTenant.Id.Value, connectionStringName);
    }

    [Obsolete("Use ResolveAsync method.")]
    public override string Resolve(string? connectionStringName = null)
    {
        if (_currentTenant.Id == null)
        {
            return base.Resolve(connectionStringName);
        }

        var tenant = FindTenantConfiguration(_currentTenant.Id.Value);
        return ResolveForTenant(tenant, _currentTenant.Id.Value, connectionStringName);
    }

    private static string ResolveForTenant(
        TenantConfiguration? tenant,
        Guid tenantId,
        string? connectionStringName)
    {
        if (tenant?.ConnectionStrings == null || tenant.ConnectionStrings.Count == 0)
        {
            throw new AbpException(
                $"Tenant '{tenantId:D}' has no connection strings. Database-per-tenant requires a Default connection string.");
        }

        var tenantDefault = tenant.ConnectionStrings.Default;
        var requestingDefault = connectionStringName.IsNullOrWhiteSpace()
            || connectionStringName == ConnectionStrings.DefaultConnectionStringName;

        if (requestingDefault)
        {
            if (tenantDefault.IsNullOrWhiteSpace())
            {
                throw new AbpException(
                    $"Tenant '{tenantId:D}' has no Default connection string.");
            }

            return tenantDefault!;
        }

        var named = tenant.ConnectionStrings.GetOrDefault(connectionStringName!);
        if (!named.IsNullOrWhiteSpace())
        {
            return named!;
        }

        if (tenantDefault.IsNullOrWhiteSpace())
        {
            throw new AbpException(
                $"Tenant '{tenantId:D}' has no connection string for '{connectionStringName}' and no Default fallback.");
        }

        // Never fall back to the host's module-specific connection string.
        return tenantDefault!;
    }
}
