using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Identity.Localization;
using SufiChain.SufiPlatform.UI.Blazor;
using SufiChain.SufiPlatform.Permissions;

namespace SufiChain.SufiPlatform.Identity.Blazor.Components;

public partial class PermissionsModal : IdentityComponentBase
{

    private static class LoadingKeys
    {
        public const string LoadPermissions = "load-permissions";
        public const string Save = "save";
    }

    [Inject] private IPermissionAppService PermissionAppService { get; set; } = default!;

    [Parameter] public EventCallback OnPermissionsUpdated { get; set; }

    private bool _isOpen;
    private string _providerName = string.Empty;
    private string _providerKey = string.Empty;
    private string _entityDisplayName = string.Empty;

    private List<PermissionGroupDto> _allGroups = new();
    private List<PermissionGroupDto> _groups = new();
    private List<PermissionGroupNavNode> _navRoots = new();
    private List<PermissionGrantInfoDto> _disabledPermissions = new();
    private Dictionary<string, int> _permissionDepths = new();

    private string? _selectedTabId;
    private string _searchText = string.Empty;
    private bool _selectAllDisabled;

    private sealed class PermissionGroupNavNode
    {
        public required PermissionGroupDto Group { get; init; }
        public List<PermissionGroupNavNode> Children { get; } = new();

        /// <summary>
        /// Product-family parent with no own permissions — render as a section label.
        /// </summary>
        public bool IsSectionOnly => Group.Permissions.Count == 0 && Children.Count > 0;
    }

    private PermissionGroupDto? SelectedGroup =>
        _groups.FirstOrDefault(g => GetNormalizedGroupName(g.Name) == _selectedTabId);

    private string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                FilterPermissionGroups();
            }
        }
    }

    /// <summary>
    /// Opens the permission management modal for a specific provider.
    /// </summary>
    /// <param name="providerName">The provider name ("U" for users, "R" for roles)</param>
    /// <param name="providerKey">The provider key (user ID or role name)</param>
    /// <param name="entityDisplayName">Display name for the entity (optional)</param>
    public async Task OpenAsync(string providerName, string providerKey, string? entityDisplayName = null)
    {
        _providerName = providerName;
        _providerKey = providerKey;
        _searchText = string.Empty;
        _selectedTabId = null;

        await ExecuteWithLoadingAsync(async () =>
        {
            var result = await PermissionAppService.GetAsync(_providerName, _providerKey);
            _entityDisplayName = entityDisplayName ?? result.EntityDisplayName;
            _allGroups = result.Groups.ToList();
            _groups = _allGroups.ToList();

            NormalizePermissionGroups();
        }, LoadingKeys.LoadPermissions);

        _isOpen = true;
        StateHasChanged();
    }

    private void NormalizePermissionGroups(bool checkDisabledPermissions = true)
    {
        _selectAllDisabled = _allGroups.Count > 0 && _allGroups.All(IsPermissionGroupDisabled);

        if (checkDisabledPermissions)
        {
            _disabledPermissions.Clear();

            foreach (var permission in _allGroups.SelectMany(x => x.Permissions))
            {
                // Permission is disabled if it's granted but not by the current provider
                if (permission.IsGranted && permission.GrantedProviders.All(x => x.ProviderName != _providerName))
                {
                    _disabledPermissions.Add(permission);
                }
            }
        }

        _permissionDepths.Clear();

        // Calculate permission depths for indentation
        foreach (var group in _groups)
        {
            SetPermissionDepths(group.Permissions, null, 0, new HashSet<string>());
        }

        RebuildNavTree();
        EnsureValidSelectedTab();
    }

    private void RebuildNavTree()
    {
        var byName = _groups.ToDictionary(g => g.Name, StringComparer.Ordinal);
        var nodes = _groups.ToDictionary(
            g => g.Name,
            g => new PermissionGroupNavNode { Group = g },
            StringComparer.Ordinal);

        var childNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (var group in _groups)
        {
            var parentName = FindParentGroupName(group.Name, byName);
            if (parentName == null || !nodes.TryGetValue(parentName, out var parentNode))
            {
                continue;
            }

            parentNode.Children.Add(nodes[group.Name]);
            childNames.Add(group.Name);
        }

        var roots = _groups
            .Where(g => !childNames.Contains(g.Name))
            .Select(g => nodes[g.Name])
            .ToList();

        SortNavNodes(roots);
        _navRoots = roots;
    }

    private static void SortNavNodes(List<PermissionGroupNavNode> nodes)
    {
        nodes.Sort(static (a, b) =>
            string.Compare(a.Group.DisplayName, b.Group.DisplayName, StringComparison.CurrentCultureIgnoreCase));

        foreach (var node in nodes)
        {
            if (node.Children.Count > 0)
            {
                SortNavNodes(node.Children);
            }
        }
    }

    /// <summary>
    /// Resolves product-family parents from dotted group names
    /// (e.g. SufiHelpDesk.Ticketing → SufiHelpDesk when that group exists).
    /// </summary>
    private static string? FindParentGroupName(string groupName, IReadOnlyDictionary<string, PermissionGroupDto> byName)
    {
        var name = groupName;
        while (true)
        {
            var dot = name.LastIndexOf('.');
            if (dot <= 0)
            {
                return null;
            }

            var parent = name[..dot];
            if (byName.ContainsKey(parent))
            {
                return parent;
            }

            name = parent;
        }
    }

    private (int Granted, int Total) GetSubtreeCounts(PermissionGroupNavNode node)
    {
        var granted = GetGrantedCount(node.Group);
        var total = node.Group.Permissions.Count;

        foreach (var child in node.Children)
        {
            var (childGranted, childTotal) = GetSubtreeCounts(child);
            granted += childGranted;
            total += childTotal;
        }

        return (granted, total);
    }

    private bool TryGetParentDisplayName(PermissionGroupDto group, out string parentDisplayName)
    {
        parentDisplayName = string.Empty;
        var byName = _allGroups.ToDictionary(g => g.Name, StringComparer.Ordinal);
        var parentName = FindParentGroupName(group.Name, byName);
        if (parentName == null || !byName.TryGetValue(parentName, out var parent))
        {
            return false;
        }

        parentDisplayName = parent.DisplayName;
        return true;
    }

    private void SelectGroup(PermissionGroupDto group)
    {
        _selectedTabId = GetNormalizedGroupName(group.Name);
    }

    private void EnsureValidSelectedTab()
    {
        if (_groups.Count == 0)
        {
            _selectedTabId = null;
            return;
        }

        var selectedStillVisible = !string.IsNullOrEmpty(_selectedTabId)
            && _groups.Any(g => GetNormalizedGroupName(g.Name) == _selectedTabId);

        if (!selectedStillVisible)
        {
            var first = GetFirstNavGroup(_navRoots) ?? _groups.First();
            _selectedTabId = GetNormalizedGroupName(first.Name);
        }
    }

    private static PermissionGroupDto? GetFirstNavGroup(IEnumerable<PermissionGroupNavNode> roots)
    {
        foreach (var root in roots)
        {
            if (!root.IsSectionOnly)
            {
                return root.Group;
            }

            var child = GetFirstNavGroup(root.Children);
            if (child != null)
            {
                return child;
            }
        }

        return null;
    }

    private void SetPermissionDepths(
        List<PermissionGrantInfoDto> permissions,
        string? currentParent,
        int currentDepth,
        HashSet<string> path)
    {
        foreach (var item in permissions)
        {
            if (item.ParentName == currentParent)
            {
                if (!path.Add(item.Name))
                {
                    continue;
                }

                _permissionDepths[item.Name] = currentDepth;
                SetPermissionDepths(permissions, item.Name, currentDepth + 1, path);
                path.Remove(item.Name);
            }
        }
    }

    private int GetPermissionDepth(string permissionName)
    {
        return _permissionDepths.GetValueOrDefault(permissionName, 0);
    }

    private string GetNormalizedGroupName(string name)
    {
        return "PermissionGroup_" + name.Replace(".", "_");
    }

    private bool IsPermissionDisabled(PermissionGrantInfoDto permission)
    {
        return _disabledPermissions.Contains(permission);
    }

    private bool IsPermissionGroupDisabled(PermissionGroupDto group)
    {
        var permissions = group.Permissions;
        var grantedProviders = permissions.SelectMany(x => x.GrantedProviders);

        return permissions.All(x => x.IsGranted) && grantedProviders.Any(p => p.ProviderName != _providerName);
    }

    private string GetInheritedProvidersLabel(PermissionGrantInfoDto permission)
    {
        var providers = permission.GrantedProviders
            .Where(p => p.ProviderName != _providerName)
            .Select(p => p.ProviderName)
            .Distinct()
            .ToArray();

        if (providers.Length == 0)
        {
            return string.Empty;
        }

        return L["InheritedFromProvider", string.Join(", ", providers)];
    }

    private bool GrantAll => _allGroups.SelectMany(x => x.Permissions).Any()
        && _allGroups.SelectMany(x => x.Permissions).All(p => p.IsGranted);

    private bool GrantAny => !GrantAll && _allGroups.SelectMany(x => x.Permissions).Any(p => p.IsGranted);

    private int GetGrantedCount(PermissionGroupDto group)
    {
        return group.Permissions.Count(x => x.IsGranted);
    }

    private int GetTotalGrantedCount()
    {
        return _allGroups.SelectMany(x => x.Permissions).Count(x => x.IsGranted);
    }

    private int GetTotalPermissionCount()
    {
        return _allGroups.SelectMany(x => x.Permissions).Count();
    }

    private bool IsGroupFullyGranted(PermissionGroupDto group)
    {
        return group.Permissions.Count > 0 && group.Permissions.All(x => x.IsGranted);
    }

    private bool IsGroupPartiallyGranted(PermissionGroupDto group)
    {
        if (IsGroupFullyGranted(group))
        {
            return false;
        }

        return group.Permissions.Any(x => x.IsGranted);
    }

    private bool HasChildPermissions(PermissionGroupDto group, PermissionGrantInfoDto permission)
    {
        return group.Permissions.Any(x => x.ParentName == permission.Name);
    }

    private bool IsPermissionSearchMatch(PermissionGrantInfoDto permission)
    {
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            return false;
        }

        return permission.DisplayName.Contains(_searchText, StringComparison.OrdinalIgnoreCase);
    }

    private IEnumerable<PermissionGrantInfoDto> GetVisiblePermissions(PermissionGroupDto group)
    {
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            return group.Permissions;
        }

        // Group title match with no permission-name hits → keep full tree for context
        var permissionMatches = group.Permissions
            .Where(p => p.DisplayName.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (permissionMatches.Count == 0
            && group.DisplayName.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
        {
            return group.Permissions;
        }

        // Include ancestors so nested matches stay readable in the tree
        var visible = permissionMatches
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var match in permissionMatches)
        {
            var current = match;
            while (!string.IsNullOrEmpty(current.ParentName))
            {
                if (!visible.Add(current.ParentName))
                {
                    break;
                }

                current = group.Permissions.FirstOrDefault(p => p.Name == current.ParentName);
                if (current == null)
                {
                    break;
                }
            }
        }

        return group.Permissions.Where(p => visible.Contains(p.Name));
    }

    private static string GetPermissionItemClass(
        int depth,
        bool isParent,
        bool isMatch,
        bool isGranted,
        bool isDisabled)
    {
        var classes = new List<string> { "permission-item", $"permission-item--depth-{Math.Min(depth, 4)}" };

        if (isParent)
        {
            classes.Add("permission-item--parent");
        }

        if (isMatch)
        {
            classes.Add("permission-item--match");
        }

        if (isGranted)
        {
            classes.Add("permission-item--granted");
        }

        if (isDisabled)
        {
            classes.Add("permission-item--disabled");
        }

        return string.Join(' ', classes);
    }

    private void FilterPermissionGroups()
    {
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            _groups = _allGroups.ToList();
        }
        else
        {
            var matched = _allGroups
                .Where(g => g.DisplayName.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                           g.Permissions.Any(p => p.DisplayName.Contains(_searchText, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            // Keep ancestor groups so product-family nesting stays intact while searching
            var byName = _allGroups.ToDictionary(g => g.Name, StringComparer.Ordinal);
            var visibleNames = matched.Select(g => g.Name).ToHashSet(StringComparer.Ordinal);
            foreach (var group in matched)
            {
                var parentName = FindParentGroupName(group.Name, byName);
                while (parentName != null)
                {
                    if (!visibleNames.Add(parentName))
                    {
                        break;
                    }

                    parentName = FindParentGroupName(parentName, byName);
                }
            }

            _groups = _allGroups.Where(g => visibleNames.Contains(g.Name)).ToList();
        }

        NormalizePermissionGroups(checkDisabledPermissions: false);
        StateHasChanged();
    }

    private void OnGrantAllChanged(bool value)
    {
        foreach (var permission in _allGroups.SelectMany(x => x.Permissions))
        {
            if (!IsPermissionDisabled(permission))
            {
                permission.IsGranted = value;
            }
        }

        // Reset search to show all groups
        _searchText = string.Empty;
        _groups = _allGroups.ToList();
        NormalizePermissionGroups(checkDisabledPermissions: false);
    }

    private void OnGroupGrantAllChanged(bool value, PermissionGroupDto group)
    {
        foreach (var permission in group.Permissions)
        {
            if (!IsPermissionDisabled(permission))
            {
                permission.IsGranted = value;
            }
        }

        StateHasChanged();
    }

    private void OnPermissionChanged(bool value, PermissionGroupDto group, PermissionGrantInfoDto permission)
    {
        permission.IsGranted = value;

        if (value)
        {
            // Grant parent permissions recursively
            GrantParentPermissions(group, permission);
        }
        else
        {
            // Revoke child permissions recursively
            RevokeChildPermissions(group, permission);
        }

        StateHasChanged();
    }

    private void GrantParentPermissions(PermissionGroupDto group, PermissionGrantInfoDto permission)
    {
        if (string.IsNullOrEmpty(permission.ParentName))
        {
            return;
        }

        var parentPermission = group.Permissions.FirstOrDefault(x => x.Name == permission.ParentName);
        if (parentPermission != null && !parentPermission.IsGranted)
        {
            parentPermission.IsGranted = true;
            GrantParentPermissions(group, parentPermission);
        }
    }

    private void RevokeChildPermissions(PermissionGroupDto group, PermissionGrantInfoDto permission)
    {
        var childPermissions = group.Permissions.Where(x => x.ParentName == permission.Name).ToList();

        foreach (var child in childPermissions)
        {
            if (child.IsGranted && !IsPermissionDisabled(child))
            {
                child.IsGranted = false;
                RevokeChildPermissions(group, child);
            }
        }
    }

    private void Hide()
    {
        _selectedTabId = null;
        _isOpen = false;
        StateHasChanged();
    }

    private Task SaveAsync() => ExecuteWithLoadingAsync(async () =>
    {
        var updateDto = new UpdatePermissionsDto
        {
            Permissions = _allGroups
                .SelectMany(g => g.Permissions)
                .Select(p => new UpdatePermissionDto
                {
                    Name = p.Name,
                    IsGranted = p.IsGranted
                })
                .ToArray()
        };

        // Warn if no permissions are granted
        if (!updateDto.Permissions.Any(x => x.IsGranted))
        {
            if (!await Message.ConfirmAsync(L["SaveWithoutAnyPermissionsWarningMessage"]))
            {
                return;
            }
        }

        await PermissionAppService.UpdateAsync(_providerName, _providerKey, updateDto);

        await Notify.SuccessAsync(L["PermissionsSavedSuccessfully"]);
        await OnPermissionsUpdated.InvokeAsync();
        Hide();
    }, LoadingKeys.Save);
}
