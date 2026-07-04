using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.LocalizationManagement.Dtos;

namespace SufiChain.SufiAbp.LocalizationManagement.Blazor.Pages;

public partial class LocalizationResources : LocalizationManagementComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadItems = "load-items";
    }

    [Inject]
    private ILocalizationResourceAppService ResourceService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private List<LocalizationResourceSummaryDto> _resources = new();
    private bool _showCreateDialog;
    private CreateUpdateLocalizationResourceDto _newResource = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadResourcesAsync();
    }

    private async Task LoadResourcesAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            _resources = await ResourceService.GetSummaryListAsync();
        }, LoadingKeys.LoadItems);
    }

    private void ViewTexts(string resourceName)
    {
        Navigation.NavigateTo($"/panel/admin/localization-management/texts?resourceName={resourceName}");
    }

    private void ShowCreateDialog()
    {
        _newResource = new CreateUpdateLocalizationResourceDto
        {
            DefaultCulture = "en",
            IsEnabled = true
        };
        _showCreateDialog = true;
    }

    private async Task CreateResourceAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            await ResourceService.CreateAsync(_newResource);
            _showCreateDialog = false;
            await Notify.SuccessAsync(L["TranslationSaved"]);
            await LoadResourcesAsync();
        });
    }
}
