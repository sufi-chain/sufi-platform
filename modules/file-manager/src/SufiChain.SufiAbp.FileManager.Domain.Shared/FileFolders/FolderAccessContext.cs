using System;
using System.Collections.Generic;

namespace SufiChain.SufiAbp.FileManager.FileFolders;

/// <summary>
/// Snapshot of the current user's identity relevant to folder access resolution.
/// Built by the application layer from <c>ICurrentUser</c> (user id + roles) and
/// from organization-unit membership (provided by an injectable provider).
/// </summary>
public sealed class FolderAccessContext
{
    /// <summary>
    /// The authenticated user id, or null for an anonymous caller.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// The current tenant id (null = host).
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Role names granted to the current user.
    /// </summary>
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Role ids granted to the current user (matched against <see cref="FolderPermission.RoleId"/>).
    /// </summary>
    public IReadOnlyList<Guid> RoleIds { get; set; } = Array.Empty<Guid>();

    /// <summary>
    /// Organization-unit ids the current user belongs to (including ancestor OUs).
    /// </summary>
    public IReadOnlyList<Guid> OrganizationUnitIds { get; set; } = Array.Empty<Guid>();

    /// <summary>
    /// The role name that grants administrative (full) access within the caller's tenant.
    /// </summary>
    public string AdminRoleName { get; set; } = "admin";

    /// <summary>
    /// Whether the caller is a host user (no tenant).
    /// </summary>
    public bool IsHost => TenantId == null;

    /// <summary>
    /// Whether the caller is anonymous (no authenticated user).
    /// </summary>
    public bool IsAnonymous => UserId == null;

    /// <summary>
    /// Whether the caller holds the configured admin role.
    /// </summary>
    public bool IsAdmin => !IsAnonymous && ContainsRole(AdminRoleName);

    /// <summary>
    /// Checks role membership by name (case-insensitive).
    /// </summary>
    public bool ContainsRole(string? roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            return false;
        }

        for (var i = 0; i < Roles.Count; i++)
        {
            if (string.Equals(Roles[i], roleName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
