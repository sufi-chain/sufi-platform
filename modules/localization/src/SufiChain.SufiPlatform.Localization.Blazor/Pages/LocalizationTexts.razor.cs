using System.Web;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Localization.Dtos;
using SufiChain.SufiBlazor.Components.Data;
using SufiChain.SufiBlazor.Contracts.Data;

namespace SufiChain.SufiPlatform.Localization.Blazor.Pages;

public partial class LocalizationTexts : LocalizationComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadItems = "load-items";
        public const string LoadFilters = "load-filters";
    }

    [Inject]
    private ILocalizationTextAppService AppService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    // Filter state
    private List<string> _resourceNames = new();
    private List<string> _cultureNames = new();
    private string? _selectedResource;
    private string? _selectedCulture;
    private string? _keyFilter;

    // Pagination state (SbDataGrid uses 0-based PageIndex)
    private int _pageIndex = 0;
    private int _pageSize = 10;
    private int _totalCount;

    // Data: DB-only mode (uses ItemsProvider for server-side paging)
    private SbDataGrid<LocalizationTextDto>? _dbOnlyGridRef;

    // Data: Merged mode uses ItemsProvider (server-side paging)
    private SbDataGrid<LocalizationTextWithBaseValueDto>? _mergedGridRef;

    /// <summary>
    /// Whether both resource and culture are selected (required to show any results).
    /// </summary>
    private bool HasValidSelection => !string.IsNullOrEmpty(_selectedResource) && !string.IsNullOrEmpty(_selectedCulture);

    /// <summary>
    /// Whether the page is showing the merged view (base JSON + DB overrides)
    /// vs. DB-only mode.
    /// </summary>
    private bool IsMergedMode => HasValidSelection;

    // Dialog state
    private bool _showCreateDialog;
    private bool _showEditDialog;
    private CreateUpdateLocalizationTextDto _newItem = new();
    private CreateUpdateLocalizationTextDto _editingItem = new();
    private Guid _editingItemId;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        ReadQueryParameters();
        await LoadFiltersAsync();
        await LoadItemsAsync();
    }

    private void ReadQueryParameters()
    {
        var uri = new Uri(Navigation.Uri);
        var queryParams = HttpUtility.ParseQueryString(uri.Query);

        var resourceName = queryParams["resourceName"];
        if (!string.IsNullOrWhiteSpace(resourceName))
        {
            _selectedResource = resourceName;
        }

        var cultureName = queryParams["cultureName"];
        if (!string.IsNullOrWhiteSpace(cultureName))
        {
            _selectedCulture = cultureName;
        }
    }

    private async Task LoadFiltersAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            try
            {
                _resourceNames = await AppService.GetResourceNamesAsync();
                _cultureNames = await AppService.GetCultureNamesAsync();

                // Validate query params: only keep if they exist in the loaded lists
                if (_resourceNames.Count > 0 && !string.IsNullOrEmpty(_selectedResource) && !_resourceNames.Contains(_selectedResource))
                {
                    _selectedResource = null;
                }
                if (_cultureNames.Count > 0 && !string.IsNullOrEmpty(_selectedCulture) && !_cultureNames.Contains(_selectedCulture))
                {
                    _selectedCulture = null;
                }
            }
            catch
            {
                // Ignore filter loading errors
            }
        }, LoadingKeys.LoadFilters);
    }

    private async Task<SbDataResponse<LocalizationTextWithBaseValueDto>> LoadMergedDataAsync(SbDataRequest request)
    {
        // Do NOT use ExecuteWithLoadingAsync here - it triggers StateHasChanged which causes
        // the grid to refresh again (OnParametersSet sets _parametersChanged), creating an infinite loop.
        var input = new GetMergedLocalizationTextsInput
        {
            ResourceName = _selectedResource!,
            CultureName = _selectedCulture!,
            KeyFilter = _keyFilter,
            SkipCount = Math.Max(0, request.PageIndex * request.PageSize),
            MaxResultCount = request.PageSize
        };

        var result = await AppService.GetMergedListAsync(input);
        return new SbDataResponse<LocalizationTextWithBaseValueDto>(result.Items, result.TotalCount);
    }

    private async Task<SbDataResponse<LocalizationTextDto>> LoadDbOnlyDataAsync(SbDataRequest request)
    {
        // Do NOT use ExecuteWithLoadingAsync here - it triggers StateHasChanged which causes
        // the grid to refresh again (OnParametersSet sets _parametersChanged), creating an infinite loop.
        var input = new GetLocalizationTextsInput
        {
            ResourceName = _selectedResource,
            CultureName = _selectedCulture,
            KeyFilter = _keyFilter,
            SkipCount = Math.Max(0, request.PageIndex * request.PageSize),
            MaxResultCount = request.PageSize
        };

        var result = await AppService.GetListAsync(input);
        return new SbDataResponse<LocalizationTextDto>(result.Items, result.TotalCount);
    }

    private async Task LoadItemsAsync()
    {
        if (!HasValidSelection)
        {
            return;
        }

        if (IsMergedMode)
        {
            await (_mergedGridRef?.RefreshDataAsync() ?? Task.CompletedTask);
        }
        else
        {
            await (_dbOnlyGridRef?.RefreshDataAsync() ?? Task.CompletedTask);
        }
    }

    private async Task SearchAsync()
    {
        _pageIndex = 0;
        await LoadItemsAsync();
    }

    private async Task ClearFilterAsync()
    {
        _selectedResource = null;
        _selectedCulture = null;
        _keyFilter = null;
        _pageIndex = 0;
        StateHasChanged();
    }

    private async Task OnPageIndexChangedAsync(int pageIndex)
    {
        _pageIndex = pageIndex;
    }

    private async Task OnPageSizeChangedAsync(int newSize)
    {
        _pageSize = newSize;
        _pageIndex = 0;
    }

    private void ShowCreateDialog()
    {
        _newItem = new CreateUpdateLocalizationTextDto
        {
            ResourceName = _selectedResource ?? string.Empty,
            CultureName = _selectedCulture ?? string.Empty
        };
        _showCreateDialog = true;
    }

    private void ShowEditDialog(LocalizationTextDto item)
    {
        _editingItemId = item.Id;
        _editingItem = new CreateUpdateLocalizationTextDto
        {
            ResourceName = item.ResourceName,
            CultureName = item.CultureName,
            Key = item.Key,
            Value = item.Value
        };
        _showEditDialog = true;
    }

    private void ShowEditDialogForMerged(LocalizationTextWithBaseValueDto item)
    {
        _editingItemId = item.Id;
        _editingItem = new CreateUpdateLocalizationTextDto
        {
            ResourceName = item.ResourceName,
            CultureName = item.CultureName,
            Key = item.Key,
            Value = item.Value
        };
        _showEditDialog = true;
    }

    private async Task CreateAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            await AppService.CreateOrUpdateAsync(_newItem);
            _showCreateDialog = false;
            await Notify.SuccessAsync(L["TranslationSaved"]);
            await LoadItemsAsync();
        });
    }

    private async Task UpdateAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            // Use CreateOrUpdate which handles both cases (new override or update existing)
            await AppService.CreateOrUpdateAsync(_editingItem);
            _showEditDialog = false;
            await Notify.SuccessAsync(L["TranslationSaved"]);
            await LoadItemsAsync();
        });
    }

    private async Task DeleteAsync(LocalizationTextDto item)
    {
        var confirmed = await Message.ConfirmAsync(
            L["ConfirmDelete"],
            L["AreYouSure"]);

        if (!confirmed)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await AppService.DeleteAsync(item.Id);
            await Notify.SuccessAsync(L["TranslationDeleted"]);
            await LoadItemsAsync();
        });
    }

    private async Task DeleteMergedAsync(LocalizationTextWithBaseValueDto item)
    {
        if (!item.IsOverride || item.Id == Guid.Empty)
        {
            return;
        }

        var confirmed = await Message.ConfirmAsync(
            L["ConfirmDelete"],
            L["AreYouSure"]);

        if (!confirmed)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await AppService.DeleteAsync(item.Id);
            await Notify.SuccessAsync(L["TranslationDeleted"]);
            await LoadItemsAsync();
        });
    }
}
