using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.FileManager.FileFolders;
using SufiChain.SufiAbp.FileManager.Localization;

namespace SufiChain.SufiAbp.FileManager.Blazor.Pages;

/// <summary>
/// Admin page for assigning folder access grants (user / role / organization unit).
/// </summary>
public partial class FolderAccess
{
    [Inject] private IFolderAppService FolderAppService { get; set; } = default!;

    private Guid? _selectedFolderId;
    private List<FolderTreeNodeDto> _flatFolders = new();
    private List<FolderPermissionDto> _permissions = new();
    private readonly FolderGrantInput _addGrant = new();

    private static readonly FolderGrantType[] _grantTypes =
    {
        FolderGrantType.User,
        FolderGrantType.Role,
        FolderGrantType.OrganizationUnit
    };

    private static readonly FolderPermissionLevelDto[] _levels =
    {
        FolderPermissionLevelDto.Read,
        FolderPermissionLevelDto.Write,
        FolderPermissionLevelDto.Delete,
        FolderPermissionLevelDto.Share,
        FolderPermissionLevelDto.Full
    };

    protected override async Task OnInitializedAsync()
    {
        var tree = await FolderAppService.GetTreeAsync();
        _flatFolders = Flatten(tree).Where(n => n.Id.HasValue).ToList()!;
    }

    private static IEnumerable<FolderTreeNodeDto?> Flatten(IEnumerable<FolderTreeNodeDto> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node;
            if (node.Children != null)
            {
                foreach (var child in Flatten(node.Children))
                {
                    yield return child;
                }
            }
        }
    }

    private async Task OnFolderChangedAsync(Guid? folderId)
    {
        _selectedFolderId = folderId;
        if (folderId.HasValue)
        {
            _permissions = await FolderAppService.GetPermissionsAsync(folderId.Value);
        }
        else
        {
            _permissions.Clear();
        }
    }

    private async Task AddGrantAsync()
    {
        if (!_selectedFolderId.HasValue || !Guid.TryParse(_addGrant.PrincipalId, out var principalId) || principalId == Guid.Empty)
        {
            return;
        }

        var dto = new FolderPermissionDto
        {
            Level = _addGrant.Level,
            InheritToChildren = _addGrant.InheritToChildren
        };

        switch (_addGrant.GrantType)
        {
            case FolderGrantType.User:
                dto.UserId = principalId;
                break;
            case FolderGrantType.Role:
                dto.RoleId = principalId;
                break;
            case FolderGrantType.OrganizationUnit:
                dto.OrganizationUnitId = principalId;
                break;
        }

        var updated = _permissions.Append(dto).ToList();
        await SaveAsync(updated);
        _addGrant.PrincipalId = string.Empty;
    }

    private async Task RemoveGrantAsync(Guid? permissionId)
    {
        if (!permissionId.HasValue)
        {
            return;
        }

        var updated = _permissions.Where(p => p.Id != permissionId.Value).ToList();
        await SaveAsync(updated);
    }

    private async Task ClearAllAsync()
    {
        await SaveAsync(new List<FolderPermissionDto>());
    }

    private async Task SaveAsync(List<FolderPermissionDto> permissions)
    {
        if (!_selectedFolderId.HasValue)
        {
            return;
        }

        // Strip ids so the backend treats the set as the new full state.
        foreach (var p in permissions)
        {
            p.Id = null;
        }

        await FolderAppService.SetPermissionsAsync(
            _selectedFolderId.Value,
            new SetFolderPermissionsInput { Permissions = permissions });

        _permissions = await FolderAppService.GetPermissionsAsync(_selectedFolderId.Value);
    }

    private string GrantTypeLabel(FolderGrantType grantType) => grantType switch
    {
        FolderGrantType.User => L["FolderAccess:User"],
        FolderGrantType.Role => L["FolderAccess:Role"],
        FolderGrantType.OrganizationUnit => L["FolderAccess:OrganizationUnit"],
        _ => grantType.ToString()
    };

    private string LevelLabel(FolderPermissionLevelDto level) => level switch
    {
        FolderPermissionLevelDto.Read => L["FolderAccess:Read"],
        FolderPermissionLevelDto.Write => L["FolderAccess:Write"],
        FolderPermissionLevelDto.Delete => L["FolderAccess:Delete"],
        FolderPermissionLevelDto.Share => L["FolderAccess:Share"],
        FolderPermissionLevelDto.Full => L["FolderAccess:Full"],
        _ => level.ToString()
    };
}

/// <summary>
/// The kind of principal a folder grant applies to.
/// </summary>
public enum FolderGrantType
{
    User = 0,
    Role = 1,
    OrganizationUnit = 2
}

/// <summary>
/// Backing model for the add-grant form.
/// </summary>
public class FolderGrantInput
{
    public FolderGrantType GrantType { get; set; } = FolderGrantType.User;
    public string PrincipalId { get; set; } = string.Empty;
    public FolderPermissionLevelDto Level { get; set; } = FolderPermissionLevelDto.Read;
    public bool InheritToChildren { get; set; } = true;
}
