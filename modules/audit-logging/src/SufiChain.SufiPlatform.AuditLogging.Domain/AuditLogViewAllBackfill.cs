using System;
using System.Collections.Generic;

namespace SufiChain.SufiPlatform.AuditLogging;

/// <summary>
/// One-time backfill of <c>AuditLogs.ViewAll</c> for people who could already see audit data.
/// The completed marker is stored once per host and once per tenant. A later seed sees that
/// marker and does not grant ViewAll again, even if an administrator removed the grant.
/// </summary>
public static class AuditLogViewAllBackfill
{
    public const string CompletedSettingName = "SufiAuditLogging.AuditLogs.ViewAllBackfillCompleted";

    public const string CompletedValue = "true";

    public static bool IsCompleted(string? markerValue)
    {
        return string.Equals(markerValue, CompletedValue, StringComparison.OrdinalIgnoreCase);
    }

    public static PlanResult Plan(
        string? markerValue,
        Guid? tenantId,
        IEnumerable<AuditPermissionGrant> grants,
        string viewAllPermission,
        IReadOnlyCollection<string> qualifyingPermissions,
        IReadOnlyCollection<string> providerNames)
    {
        if (IsCompleted(markerValue))
        {
            return new PlanResult(AlreadyCompleted: true, Holders: Array.Empty<AuditPermissionHolder>());
        }

        return new PlanResult(
            AlreadyCompleted: false,
            Holders: AuditLogViewAllGrantPlanner.HoldersMissingViewAll(
                tenantId,
                grants,
                viewAllPermission,
                qualifyingPermissions,
                providerNames));
    }

    public readonly record struct PlanResult(bool AlreadyCompleted, IReadOnlyList<AuditPermissionHolder> Holders)
    {
        public bool ShouldMarkCompleted => !AlreadyCompleted;
    }
}
