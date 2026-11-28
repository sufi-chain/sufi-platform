using System;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Tenants;

/// <summary>
/// Refuses to save a dedicated tenant's row through a host connection.
/// A shared tenant (no database name) may still use the host database.
/// </summary>
public static class DedicatedTenantHostWriteGuard
{
    public static void ThrowIfDedicatedRowUsesHostConnection(
        Guid? tenantId,
        bool tenantIsDedicated,
        string? contextConnection,
        string? hostConnection)
    {
        if (tenantId == null || !tenantIsDedicated)
        {
            return;
        }

        if (contextConnection.IsNullOrWhiteSpace() || hostConnection.IsNullOrWhiteSpace())
        {
            return;
        }

        if (string.Equals(contextConnection.Trim(), hostConnection.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException(TenantsErrorCodes.DedicatedTenantHostWrite)
                .WithData("TenantId", tenantId.Value);
        }
    }
}
