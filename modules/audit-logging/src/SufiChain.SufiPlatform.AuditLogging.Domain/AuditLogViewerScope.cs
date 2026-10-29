namespace SufiChain.SufiPlatform.AuditLogging;

/// <summary>
/// Limits audit visibility so a viewer cannot read another user's request URLs or admin activity.
/// The admin role and the ViewAll permission keep the full log.
/// </summary>
public static class AuditLogViewerScope
{
    public const string AdminRoleName = "admin";

    public static bool CanViewOtherUsers(bool isAdminRole, bool hasViewAllPermission)
    {
        return isAdminRole || hasViewAllPermission;
    }

    public static Guid? EffectiveUserId(bool canViewOtherUsers, Guid? currentUserId, Guid? requestedUserId)
    {
        if (canViewOtherUsers)
        {
            return requestedUserId;
        }

        return currentUserId ?? Guid.Empty;
    }

    public static string? EffectiveUserName(bool canViewOtherUsers, string? requestedUserName)
    {
        return canViewOtherUsers ? requestedUserName : null;
    }

    public static bool CanReadEntry(bool canViewOtherUsers, Guid? currentUserId, Guid? entryUserId)
    {
        if (canViewOtherUsers)
        {
            return true;
        }

        return currentUserId.HasValue && entryUserId == currentUserId;
    }
}
