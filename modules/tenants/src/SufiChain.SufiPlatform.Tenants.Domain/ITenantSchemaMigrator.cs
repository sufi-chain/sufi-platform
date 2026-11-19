using System;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.Tenants;

/// <summary>
/// Host adapter for the existing tenant schema migrator.
/// The tenants module does not migrate databases itself.
/// </summary>
public interface ITenantSchemaMigrator
{
    Task MigrateSchemaAsync(Guid tenantId);
}
