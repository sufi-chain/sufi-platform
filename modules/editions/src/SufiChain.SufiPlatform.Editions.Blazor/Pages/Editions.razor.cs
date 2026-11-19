using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Editions;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Features.Blazor.Components;

namespace SufiChain.SufiPlatform.Editions.Blazor.Pages;

public partial class Editions
{
    private const string EditionFeatureProviderName = EditionFeatureValueProvider.ProviderName;

    [Inject] protected IEditionAppService EditionAppService { get; set; } = default!;
    [Inject] protected new IAuthorizationService AuthorizationService { get; set; } = default!;
    [Inject] protected IEnumerable<IEditionPriceSaver> PriceSavers { get; set; } = Array.Empty<IEditionPriceSaver>();

    private FeaturesModal _featuresModal = default!;
    private List<EditionDto>? _items;
    private bool _showCreate;
    private bool _showEdit;
    private bool _loading;
    private bool _saving;
    private int _saveVersion;
    private string? _formError;
    private Guid? _editingId;
    private string? _concurrencyStamp;
    private EditionFormModel _form = new();

    private bool HasCreatePermission { get; set; }
    private bool HasUpdatePermission { get; set; }
    private bool HasDeletePermission { get; set; }
    private bool HasManageFeaturesPermission { get; set; }

    protected override async Task OnInitializedAsync()
    {
        HasCreatePermission = await AuthorizationService.IsGrantedAsync(EditionsPermissions.Editions.Create);
        HasUpdatePermission = await AuthorizationService.IsGrantedAsync(EditionsPermissions.Editions.Update);
        HasDeletePermission = await AuthorizationService.IsGrantedAsync(EditionsPermissions.Editions.Delete);
        HasManageFeaturesPermission = await AuthorizationService.IsGrantedAsync(EditionsPermissions.Editions.ManageFeatures);
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        _loading = true;
        try
        {
            var result = await EditionAppService.GetListAsync(new GetEditionsInput { MaxResultCount = 100 });
            _items = result.Items.ToList();
        }
        finally
        {
            _loading = false;
        }
    }

    private void ShowCreateModal()
    {
        _form = new EditionFormModel { IsActive = true };
        _formError = null;
        _editingId = null;
        _concurrencyStamp = null;
        _showCreate = true;
        _showEdit = false;
    }

    private void ShowEditModal(EditionDto edition)
    {
        _form = MapToForm(edition);
        _formError = null;
        _editingId = edition.Id;
        _concurrencyStamp = edition.ConcurrencyStamp;
        _showEdit = true;
        _showCreate = false;
    }

    /// <summary>Exposed for Active-toggle regression tests (#193).</summary>
    internal static EditionFormModel MapToForm(EditionDto edition)
    {
        var visible = EditionEditorValues.VisibleLabel(edition.LocalizedDisplayName, edition.DisplayName);
        decimal price = 0m;
        var currency = string.Empty;
        if (edition.PriceSummary is { IsPriced: true, Amount: { } stored })
        {
            price = stored;
            currency = edition.PriceSummary.CurrencyCode ?? string.Empty;
        }

        return new EditionFormModel
        {
            Name = edition.Name,
            DisplayName = visible,
            StoredDisplayName = edition.DisplayName,
            InitialDisplayName = visible,
            Code = edition.Code,
            IsActive = edition.IsActive,
            Price = price,
            CurrencyCode = currency
        };
    }

    private string PriceHelper
    {
        get
        {
            if (EditionEditorValues.PriceNeedsCurrency(_form.Price, _form.CurrencyCode))
            {
                return L["EditionPriceCurrencyRequired"];
            }

            if (!EditionEditorValues.ShouldPersistPrice(_form.CurrencyCode))
            {
                return L["PriceNotSet"];
            }

            return EditionPriceDisplay.Resolve(
                new EditionPriceSummary
                {
                    IsPriced = true,
                    Amount = _form.Price,
                    CurrencyCode = _form.CurrencyCode
                },
                L["PriceNotSet"]);
        }
    }

    private void CloseModal()
    {
        _saveVersion++;
        _saving = false;
        _showCreate = false;
        _showEdit = false;
    }

    private async Task SaveAsync()
    {
        if (EditionEditorValues.PriceNeedsCurrency(_form.Price, _form.CurrencyCode))
        {
            _formError = L["EditionPriceCurrencyRequired"];
            return;
        }

        var version = _saveVersion;
        _saving = true;
        _formError = null;
        var displayName = EditionEditorValues.DisplayNameToStore(
            _form.DisplayName,
            _form.InitialDisplayName,
            _form.StoredDisplayName);
        try
        {
            EditionDto? saved = null;
            if (_showCreate)
            {
                saved = await EditionAppService.CreateAsync(new EditionCreateDto
                {
                    Name = _form.Name,
                    DisplayName = displayName,
                    Code = _form.Code,
                    IsActive = _form.IsActive
                });
            }
            else if (_editingId.HasValue)
            {
                saved = await EditionAppService.UpdateAsync(_editingId.Value, new EditionUpdateDto
                {
                    Name = _form.Name,
                    DisplayName = displayName,
                    Code = _form.Code,
                    IsActive = _form.IsActive,
                    ConcurrencyStamp = _concurrencyStamp ?? string.Empty
                });
            }

            if (saved != null && EditionEditorValues.ShouldPersistPrice(_form.CurrencyCode))
            {
                var price = new EditionPriceSaveInput
                {
                    EditionId = saved.Id,
                    Code = saved.Code,
                    Name = saved.Name,
                    DisplayName = saved.DisplayName,
                    Amount = _form.Price,
                    CurrencyCode = _form.CurrencyCode.Trim()
                };
                foreach (var saver in PriceSavers)
                {
                    await saver.SaveAsync(price);
                }
            }

            if (version != _saveVersion)
            {
                return;
            }

            CloseModal();
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            if (version != _saveVersion)
            {
                return;
            }

            await HandleErrorAsync(ex);
        }
        finally
        {
            if (version == _saveVersion)
            {
                _saving = false;
            }
        }
    }

    private async Task DeleteAsync(EditionDto edition)
    {
        await EditionAppService.DeleteAsync(edition.Id);
        await ReloadAsync();
    }

    private Task ShowEditionFeatures(EditionDto edition)
    {
        return _featuresModal.OpenAsync(EditionFeatureProviderName, edition.Id.ToString(), edition.LocalizedDisplayName);
    }

    internal sealed class EditionFormModel
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string StoredDisplayName { get; set; } = string.Empty;
        public string InitialDisplayName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public decimal Price { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
    }
}
