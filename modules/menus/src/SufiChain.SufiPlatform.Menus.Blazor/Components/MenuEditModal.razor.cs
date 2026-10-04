using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Localization.Blazor.Public.Components;
using SufiChain.SufiPlatform.Localization.Blazor.Public.Models;
using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Menus.Blazor.Components;

public partial class MenuEditModal : MenusComponentBase
{
    private static class LoadingKeys
    {
        public const string Save = "save";
    }

    private IMenuAppService MenuAppService => LazyGetRequiredService(ref _menuAppService);
    private IMenuAppService? _menuAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public MenuDto? Menu { get; set; }
    [Parameter] public EventCallback OnMenuUpdated { get; set; }

    private UpdateMenuDto _model = new();
    private Guid _menuId;
    private Dictionary<string, string> _displayNames = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, string> _displayNameBases = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, object> KeyFieldAttributes = new()
    {
        ["dir"] = "ltr",
        ["lang"] = "en"
    };

    private string LabelKeyText => _labelKey ?? string.Empty;
    private List<MultilingualCulture> _labelCultures = new();
    private MultilingualTextField? _labels;
    private string? _labelKey;
    private bool _labelBroken;
    private bool _sameForAll;
    private bool _culturesFailed;
    private string? _saveError;

    protected override async Task OnParametersSetAsync()
    {
        if (!Open || Menu == null || Menu.Id == _menuId)
        {
            return;
        }

        _menuId = Menu.Id;
        _model = new UpdateMenuDto
        {
            DisplayName = Menu.DisplayName,
            Description = Menu.Description,
            IsActive = Menu.IsActive
        };
        _saveError = null;
        _sameForAll = false;
        await LoadCulturesAsync();
        ApplyLoadedLabel(Menu);
    }

    private async Task LoadCulturesAsync()
    {
        try
        {
            _labelCultures = MenuLabelCultureMapper.Map(await MenuAppService.GetLabelCulturesAsync());
            _culturesFailed = _labelCultures.Count == 0;
        }
        catch (Exception)
        {
            _labelCultures = new List<MultilingualCulture>();
            _culturesFailed = true;
        }
    }

    private void ApplyLoadedLabel(MenuDto menu)
    {
        _labelBroken = BusinessTextEditorStorage.IsPlaceholder(menu.DisplayName);
        _labelKey = BusinessLocalizationHelper.IsBusinessLocalizationKey(menu.DisplayName) && !_labelBroken
            ? menu.DisplayName.Trim()
            : null;
        _displayNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _displayNameBases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var culture in _labelCultures)
        {
            _displayNames[culture.CultureName] = string.Empty;
        }

        if (menu.DisplayNameBases != null)
        {
            foreach (var pair in menu.DisplayNameBases)
            {
                _displayNameBases[pair.Key] = pair.Value ?? string.Empty;
            }
        }

        if (_displayNames.Count == 0)
        {
            _displayNames["fa"] = string.Empty;
        }

        if (_labelBroken || menu.DisplayNames == null)
        {
            return;
        }

        foreach (var pair in menu.DisplayNames)
        {
            _displayNames[pair.Key] = pair.Value ?? string.Empty;
        }
    }

    private Task CopyLabelKeyAsync() => CopyTextAsync(_labelKey);

    private Task Hide() => SetOpenAsync(false);

    private async Task SetOpenAsync(bool open)
    {
        Open = open;
        if (!open)
        {
            _menuId = Guid.Empty;
        }

        await OpenChanged.InvokeAsync(open);
    }

    private Task OnDisplayNamesChanged(Dictionary<string, string> values)
    {
        _displayNames = values;
        return Task.CompletedTask;
    }

    private Task OnValidSubmitAsync() => ExecuteWithLoadingAsync(async () =>
    {
        _saveError = null;
        if (_labels == null || !await _labels.ValidateAsync())
        {
            return;
        }

        _model.DisplayNames = _labels.GetValuesForSave();
        _model.DisplayName = _labelKey ?? Menu?.Name ?? string.Empty;
        try
        {
            await MenuAppService.UpdateAsync(_menuId, _model);
        }
        catch (Exception ex) when (ex is BusinessException or UserFriendlyException)
        {
            _saveError = string.IsNullOrWhiteSpace(ex.Message) ? L["Sufi.Menus:DisplayNameRequired"] : ex.Message;
            return;
        }

        await OnMenuUpdated.InvokeAsync();
        await Hide();
    }, LoadingKeys.Save);
}
