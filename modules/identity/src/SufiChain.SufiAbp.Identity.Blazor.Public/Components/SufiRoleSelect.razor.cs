using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.Identity;

namespace SufiChain.SufiAbp.Identity.Blazor.Public.Components;

public partial class SufiRoleSelect : SufiRoleLookupInlineComponentBase
{
    [Parameter]
    public Guid? RoleId { get; set; }

    [Parameter]
    public EventCallback<Guid?> RoleIdChanged { get; set; }

    [Parameter]
    public EventCallback<IdentityRoleDto?> SelectedRoleChanged { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool AllowClear { get; set; } = true;

    [Parameter]
    public int MaxResultCount { get; set; } = DefaultMaxResultCount;

    private IdentityRoleDto? _selectedRole;
    private Guid? _loadedRoleId;

    protected override async Task OnParametersSetAsync()
    {
        if (RoleId.HasValue && RoleId != _loadedRoleId)
        {
            await EnsureSelectedRoleDisplayedAsync(RoleId.Value);
        }
        else if (!RoleId.HasValue && _loadedRoleId.HasValue)
        {
            _selectedRole = null;
            _loadedRoleId = null;
        }
    }

    private async Task EnsureSelectedRoleDisplayedAsync(Guid roleId)
    {
        if (_selectedRole?.Id == roleId)
        {
            _loadedRoleId = roleId;
            return;
        }

        _selectedRole = await TryGetRoleAsync(roleId);
        _loadedRoleId = roleId;
    }

    private async Task<IEnumerable<IdentityRoleDto>> SearchRolesForSelectAsync(string filter)
    {
        return await SearchRolesAsync(filter, MaxResultCount);
    }

    private async Task OnSelectedRoleChangedAsync(IdentityRoleDto? role)
    {
        _selectedRole = role;
        _loadedRoleId = role?.Id;
        await RoleIdChanged.InvokeAsync(role?.Id);
        await SelectedRoleChanged.InvokeAsync(role);
    }
}
