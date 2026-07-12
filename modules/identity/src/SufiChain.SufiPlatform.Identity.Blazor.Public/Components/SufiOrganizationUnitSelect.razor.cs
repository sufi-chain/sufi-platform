using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Identity.OrganizationUnits.Dtos;

namespace SufiChain.SufiPlatform.Identity.Blazor.Public.Components;

public partial class SufiOrganizationUnitSelect : SufiOrganizationUnitLookupInlineComponentBase
{
    [Parameter]
    public Guid? OrganizationUnitId { get; set; }

    [Parameter]
    public EventCallback<Guid?> OrganizationUnitIdChanged { get; set; }

    [Parameter]
    public EventCallback<OrganizationUnitDto?> SelectedOrganizationUnitChanged { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool AllowClear { get; set; } = true;

    [Parameter]
    public int MaxResultCount { get; set; } = DefaultMaxResultCount;

    private OrganizationUnitDto? _selectedOrganizationUnit;
    private Guid? _loadedOrganizationUnitId;

    protected override async Task OnParametersSetAsync()
    {
        if (OrganizationUnitId.HasValue && OrganizationUnitId != _loadedOrganizationUnitId)
        {
            await EnsureSelectedOrganizationUnitDisplayedAsync(OrganizationUnitId.Value);
        }
        else if (!OrganizationUnitId.HasValue && _loadedOrganizationUnitId.HasValue)
        {
            _selectedOrganizationUnit = null;
            _loadedOrganizationUnitId = null;
        }
    }

    private async Task EnsureSelectedOrganizationUnitDisplayedAsync(Guid organizationUnitId)
    {
        if (_selectedOrganizationUnit?.Id == organizationUnitId)
        {
            _loadedOrganizationUnitId = organizationUnitId;
            return;
        }

        _selectedOrganizationUnit = await TryGetOrganizationUnitAsync(organizationUnitId);
        _loadedOrganizationUnitId = organizationUnitId;
    }

    private async Task<IEnumerable<OrganizationUnitDto>> SearchOrganizationUnitsForSelectAsync(string filter)
    {
        return await SearchOrganizationUnitsAsync(filter, MaxResultCount);
    }

    private async Task OnSelectedOrganizationUnitChangedAsync(OrganizationUnitDto? organizationUnit)
    {
        _selectedOrganizationUnit = organizationUnit;
        _loadedOrganizationUnitId = organizationUnit?.Id;
        await OrganizationUnitIdChanged.InvokeAsync(organizationUnit?.Id);
        await SelectedOrganizationUnitChanged.InvokeAsync(organizationUnit);
    }
}
