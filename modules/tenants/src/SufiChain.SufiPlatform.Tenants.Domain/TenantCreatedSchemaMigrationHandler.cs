using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace SufiChain.SufiPlatform.Tenants;

/// <summary>
/// Migrates a dedicated tenant database when the tenant already has a connection string.
/// Schema only. The DbMigrator seed uses the default admin password, so the create
/// dialog seeds the submitted password itself after this handler returns.
/// A database name without a connection string is still created by the existing
/// workspace provisioner, which stores the connection string and then migrates.
/// </summary>
public class TenantCreatedSchemaMigrationHandler : IDistributedEventHandler<TenantCreatedEto>, ITransientDependency
{
    private readonly ITenantRepository _tenants;
    private readonly IEnumerable<ITenantSchemaMigrator> _migrators;

    public TenantCreatedSchemaMigrationHandler(
        ITenantRepository tenants,
        IEnumerable<ITenantSchemaMigrator> migrators)
    {
        _tenants = tenants;
        _migrators = migrators;
    }

    public virtual async Task HandleEventAsync(TenantCreatedEto eventData)
    {
        var tenant = await _tenants.FindAsync(eventData.Id, includeDetails: true);
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
