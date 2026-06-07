using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.Identity.Localization;
using SufiChain.SufiAbp.Identity.OrganizationUnits;
using SufiChain.SufiAbp.Identity.OrganizationUnits.Dtos;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.SufiAbp.Identity.Blazor.Components;

public partial class OUCreateModal : IdentityComponentBase
{

    private static class LoadingKeys
    {
        public const string Create = "create";
    }

    private IOrganizationUnitAppService OrganizationUnitAppService => LazyGetRequiredService(ref _organizationUnitAppService);
    private IOrganizationUnitAppService? _organizationUnitAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public Guid? ParentId { get; set; }
    [Parameter] public EventCallback OnCreated { get; set; }

    private CreateOrganizationUnitDto _model = new();
    private string _parentDisplayName = string.Empty;

    protected override async Task OnParametersSetAsync()
    {
        if (Open)
        {
            _model = new CreateOrganizationUnitDto
            {
                ParentId = ParentId
            };

            if (ParentId.HasValue)
            {
                try
                {
                    var parent = await OrganizationUnitAppService.GetAsync(ParentId.Value);
                    _parentDisplayName = parent.DisplayName;
                }
                catch
                {
                    _parentDisplayName = string.Empty;
                }
            }
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
        await OrganizationUnitAppService.CreateAsync(_model);
        await OnCreated.InvokeAsync();
        await SetOpenAsync(false);
    }, LoadingKeys.Create);
}
