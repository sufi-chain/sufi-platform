using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Tenants;

/// <summary>
/// Migrates a dedicated tenant database when a connection string is already stored.
/// Called directly during create, before the admin is seeded. The distributed
/// <see cref="TenantCreatedEto"/> is also handled later, after the outbox sends it.
/// </summary>
public class TenantDedicatedSchemaMigrator : ITransientDependency
{
    private readonly ITenantRepository _tenants;
    private readonly IEnumerable<ITenantSchemaMigrator> _migrators;

    public TenantDedicatedSchemaMigrator(
        ITenantRepository tenants,
        IEnumerable<ITenantSchemaMigrator> migrators)
    {
        _tenants = tenants;
        _migrators = migrators;
    }

    public virtual async Task MigrateAsync(Guid tenantId)
    {
        var tenant = await _tenants.FindAsync(tenantId, includeDetails: true);
        if (tenant == null || tenant.ConnectionStrings.Count == 0)
        {
            return;
        }

        foreach (var migrator in _migrators)
        {
            await migrator.MigrateSchemaAsync(tenant.Id);
        }
    }
}
