using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace SufiChain.SufiPlatform.Tenants;

/// <summary>
/// Runs when the outbox delivers <see cref="TenantCreatedEto"/>.
/// The event has no admin password. Schema only.
/// </summary>
public class TenantCreatedSchemaMigrationHandler : IDistributedEventHandler<TenantCreatedEto>, ITransientDependency
{
    private readonly TenantDedicatedSchemaMigrator _migrator;

    public TenantCreatedSchemaMigrationHandler(TenantDedicatedSchemaMigrator migrator)
    {
        _migrator = migrator;
    }

    public virtual Task HandleEventAsync(TenantCreatedEto eventData)
    {
        return _migrator.MigrateAsync(eventData.Id);
    }
}
