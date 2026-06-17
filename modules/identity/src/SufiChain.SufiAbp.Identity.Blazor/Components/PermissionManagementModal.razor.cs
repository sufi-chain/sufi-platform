using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.Identity.Localization;
using SufiChain.SufiAbp.UI.Blazor;
using SufiChain.SufiAbp.PermissionManagement;

namespace SufiChain.SufiAbp.Identity.Blazor.Components;

public partial class PermissionManagementModal : IdentityComponentBase
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
    private List<PermissionGrantInfoDto> _disabledPermissions = new();
    private Dictionary<string, int> _permissionDepths = new();

    private string? _selectedTabId;
    private string _searchText = string.Empty;
    private bool _selectAllDisabled;

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
            _allGroups = result.Groups
                .OrderBy(x => x.DisplayName)
                .ToList();
            _groups = _allGroups.ToList();

            NormalizePermissionGroups();
        }, LoadingKeys.LoadPermissions);

        _isOpen = true;
        StateHasChanged();
    }

    private void NormalizePermissionGroups(bool checkDisabledPermissions = true)
    {
        _selectAllDisabled = _groups.All(IsPermissionGroupDisabled);

        if (checkDisabledPermissions)
        {
            _disabledPermissions.Clear();

            foreach (var permission in _groups.SelectMany(x => x.Permissions))
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

        // Select first tab if not already selected
        if (_groups.Count > 0 && string.IsNullOrEmpty(_selectedTabId))
        {
            _selectedTabId = GetNormalizedGroupName(_groups.First().Name);
        }
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

    private string GetPermissionDisplayName(PermissionGrantInfoDto permission)
    {
        if (!IsPermissionDisabled(permission))
        {
            return permission.DisplayName;
        }

        // Show which provider granted this permission
        var providers = permission.GrantedProviders
            .Where(p => p.ProviderName != _providerName)
            .Select(p => p.ProviderName)
            .Distinct();

        return $"{permission.DisplayName} ({string.Join(", ", providers)})";
    }

    private bool GrantAll => _groups.SelectMany(x => x.Permissions).All(p => p.IsGranted);
    private bool GrantAny => !GrantAll && _groups.SelectMany(x => x.Permissions).Any(p => p.IsGranted);

    private int GetGrantedCount(PermissionGroupDto group)
    {
        return group.Permissions.Count(x => x.IsGranted);
    }

    private void FilterPermissionGroups()
    {
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            _groups = _allGroups.ToList();
        }
        else
        {
            _groups = _allGroups
                .Where(g => g.DisplayName.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                           g.Permissions.Any(p => p.DisplayName.Contains(_searchText, StringComparison.OrdinalIgnoreCase)))
                .ToList();
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
