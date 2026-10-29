using System;
using System.Collections.Generic;
using System.Linq;

namespace SufiChain.SufiPlatform.AuditLogging;

/// <summary>
/// One stored permission grant used to decide who already sees audit data.
/// </summary>
public readonly record struct AuditPermissionGrant(
    Guid? TenantId,
    string ProviderName,
    string? ProviderKey,
    string PermissionName);

/// <summary>
/// A role or user that should receive <c>AuditLogs.ViewAll</c>.
/// </summary>
public readonly record struct AuditPermissionHolder(string ProviderName, string ProviderKey);

/// <summary>
/// Keeps today's audit visibility: every current role or user grant of the audit-log
/// or entity-change permission also receives ViewAll, once per tenant.
/// Holders that already have ViewAll are left unchanged.
/// </summary>
public static class AuditLogViewAllGrantPlanner
{
    public static IReadOnlyList<AuditPermissionHolder> HoldersMissingViewAll(
        Guid? tenantId,
        IEnumerable<AuditPermissionGrant> grants,
        string viewAllPermission,
        IReadOnlyCollection<string> qualifyingPermissions,
        IReadOnlyCollection<string> providerNames)
    {
        var qualifying = new HashSet<string>(qualifyingPermissions, StringComparer.Ordinal);
        var providers = new HashSet<string>(providerNames, StringComparer.Ordinal);
        var byHolder = new Dictionary<(string Provider, string Key), HashSet<string>>();

        foreach (var grant in grants)
        {
            if (grant.TenantId != tenantId
                || string.IsNullOrWhiteSpace(grant.ProviderName)
                || string.IsNullOrWhiteSpace(grant.ProviderKey)
                || !providers.Contains(grant.ProviderName))
            {
                continue;
            }

            if (!qualifying.Contains(grant.PermissionName)
                && !string.Equals(grant.PermissionName, viewAllPermission, StringComparison.Ordinal))
            {
                continue;
            }

            var holderKey = (grant.ProviderName, grant.ProviderKey);
            if (!byHolder.TryGetValue(holderKey, out var names))
            {
                names = new HashSet<string>(StringComparer.Ordinal);
                byHolder[holderKey] = names;
            }

            names.Add(grant.PermissionName);
        }

        return byHolder
            .Where(pair => !pair.Value.Contains(viewAllPermission) && pair.Value.Overlaps(qualifying))
            .Select(pair => new AuditPermissionHolder(pair.Key.Provider, pair.Key.Key))
            .OrderBy(holder => holder.ProviderName, StringComparer.Ordinal)
            .ThenBy(holder => holder.ProviderKey, StringComparer.Ordinal)
            .ToList();
    }
}
