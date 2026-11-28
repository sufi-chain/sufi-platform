using SufiChain.SufiPlatform.FileManager.AccessControl;
using SufiChain.SufiPlatform.FileManager.FileFolders;

namespace SufiChain.SufiPlatform.Identity.Integration;

/// <summary>
/// Identity-backed check and display names for folder grants.
/// Repositories stay tenant-scoped, so a role or user from another tenant does not match.
/// </summary>
public class IdentityFolderGrantPrincipalStore : IFolderGrantPrincipalStore
{
    private readonly IIdentityUserRepository _userRepository;
    private readonly IIdentityRoleRepository _roleRepository;
    private readonly IOrganizationUnitRepository _organizationUnitRepository;

    public IdentityFolderGrantPrincipalStore(
        IIdentityUserRepository userRepository,
        IIdentityRoleRepository roleRepository,
        IOrganizationUnitRepository organizationUnitRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _organizationUnitRepository = organizationUnitRepository;
    }

    public async Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindAsync(userId, cancellationToken: cancellationToken);
        return user != null;
    }

    public async Task<bool> RoleExistsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.FindAsync(roleId, cancellationToken: cancellationToken);
        return role != null;
    }

    public async Task<bool> OrganizationUnitExistsAsync(Guid organizationUnitId, CancellationToken cancellationToken = default)
    {
        var organizationUnit = await _organizationUnitRepository.FindAsync(
            organizationUnitId,
            cancellationToken: cancellationToken);
        return organizationUnit != null;
    }

    public async Task PopulateAsync(IList<FolderPermissionDto> permissions, CancellationToken cancellationToken = default)
    {
        if (permissions == null || permissions.Count == 0)
        {
            return;
        }

        var userIds = permissions
            .Where(permission => permission.UserId.HasValue)
            .Select(permission => permission.UserId!.Value)
            .Distinct()
            .ToList();
        var users = new Dictionary<Guid, string>();
        foreach (var userId in userIds)
        {
            var user = await _userRepository.FindAsync(userId, cancellationToken: cancellationToken);
            if (user != null)
            {
                users[user.Id] = user.UserName;
            }
        }

        var roleIds = permissions
            .Where(permission => permission.RoleId.HasValue)
            .Select(permission => permission.RoleId!.Value)
            .Distinct()
            .ToList();
        var roles = roleIds.Count == 0
            ? new Dictionary<Guid, string>()
            : (await _roleRepository.GetListAsync(roleIds, cancellationToken))
                .ToDictionary(role => role.Id, role => role.Name);

        var organizationUnitIds = permissions
            .Where(permission => permission.OrganizationUnitId.HasValue)
            .Select(permission => permission.OrganizationUnitId!.Value)
            .Distinct()
            .ToList();
        var organizationUnits = organizationUnitIds.Count == 0
            ? new Dictionary<Guid, string>()
            : (await _organizationUnitRepository.GetListAsync(organizationUnitIds, includeDetails: false, cancellationToken))
                .ToDictionary(unit => unit.Id, unit => unit.DisplayName);

        foreach (var permission in permissions)
        {
            if (permission.UserId.HasValue && users.TryGetValue(permission.UserId.Value, out var userName))
            {
                permission.UserName = userName;
            }

            if (permission.RoleId.HasValue && roles.TryGetValue(permission.RoleId.Value, out var roleName))
            {
                permission.RoleName = roleName;
            }

            if (permission.OrganizationUnitId.HasValue &&
                organizationUnits.TryGetValue(permission.OrganizationUnitId.Value, out var organizationUnitName))
            {
                permission.OrganizationUnitName = organizationUnitName;
            }
        }
    }
}
