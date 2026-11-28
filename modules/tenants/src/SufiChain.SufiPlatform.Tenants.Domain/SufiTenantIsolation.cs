using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Tenants;

/// <summary>
/// Marks a dedicated tenant on <see cref="TenantConfiguration"/> without a new column.
/// <see cref="Tenant.DatabaseName"/> is copied into the configuration connection-string bag
/// under <see cref="DatabaseNameKey"/>. The resolver never returns that entry as a connection string.
/// A tenant with no database name stays on the shared host database.
/// </summary>
public static class SufiTenantIsolation
{
    public const string DatabaseNameKey = "SufiDatabaseName";

    public static void Stamp(ConnectionStrings connectionStrings, string? databaseName)
    {
        if (databaseName.IsNullOrWhiteSpace())
        {
            return;
        }

        connectionStrings[DatabaseNameKey] = databaseName;
    }

    public static bool IsDedicated(TenantConfiguration? tenant)
    {
        var connectionStrings = tenant?.ConnectionStrings;
        return connectionStrings != null
            && connectionStrings.TryGetValue(DatabaseNameKey, out var databaseName)
            && !databaseName.IsNullOrWhiteSpace();
    }
}
