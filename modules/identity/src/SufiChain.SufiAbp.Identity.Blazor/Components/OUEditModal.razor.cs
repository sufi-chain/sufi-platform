using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.Identity.Localization;
using SufiChain.SufiAbp.Identity.OrganizationUnits;
using SufiChain.SufiAbp.Identity.OrganizationUnits.Dtos;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.SufiAbp.Identity.Blazor.Components;

public partial class OUEditModal : IdentityComponentBase
{

    private static class LoadingKeys
    {
        public const string Save = "save";
    }

    private IOrganizationUnitAppService OrganizationUnitAppService => LazyGetRequiredService(ref _organizationUnitAppService);
    private IOrganizationUnitAppService? _organizationUnitAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public OrganizationUnitDto? OrganizationUnit { get; set; }
    [Parameter] public EventCallback OnUpdated { get; set; }

    private UpdateOrganizationUnitDto _model = new();

    protected override void OnParametersSet()
    {
        if (Open && OrganizationUnit != null)
        {
            _model = new UpdateOrganizationUnitDto
            {
                DisplayName = OrganizationUnit.DisplayName
            };
        }
    }

    private Task Hide()
    {
        return SetOpenAsync(false);
    }

    private async Task SetOpenAsync(bool open)
    {
        Open = open;
        await OpenChanged.InvokeAsync(open);
    }

    private Task OnValidSubmitAsync() => ExecuteWithLoadingAsync(async () =>
    {
        if (OrganizationUnit == null) return;

        await OrganizationUnitAppService.UpdateAsync(OrganizationUnit.Id, _model);
        await OnUpdated.InvokeAsync();
        await SetOpenAsync(false);
    }, LoadingKeys.Save);
}
