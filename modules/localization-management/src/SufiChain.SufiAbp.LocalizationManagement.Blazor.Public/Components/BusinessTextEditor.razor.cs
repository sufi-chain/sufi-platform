using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.LocalizationManagement.Blazor.Public.Models;
using SufiChain.SufiAbp.LocalizationManagement.Dtos;

namespace SufiChain.SufiAbp.LocalizationManagement.Blazor.Public.Components;

public partial class BusinessTextEditor
{
    private List<BusinessLocalizationCultureDto> _cultures = new();
    private Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);
    private int _activeCultureTab;
    private bool _loading;
    private bool _culturesLoaded;
    private string? _loadedResourceName;
    private string? _loadedLocalizationKey;
    private string _validationMessage = string.Empty;

    [Parameter] public string? ResourceName { get; set; }
    [Parameter] public string? LocalizationKey { get; set; }
    [Parameter] public EventCallback<string?> LocalizationKeyChanged { get; set; }
    [Parameter] public BusinessTextEditorMode Mode { get; set; }
    [Parameter] public string? LiteralValue { get; set; }
    [Parameter] public EventCallback<string?> LiteralValueChanged { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? Label { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await EnsureCulturesLoadedAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (Mode != BusinessTextEditorMode.Localized)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(ResourceName) || string.IsNullOrWhiteSpace(LocalizationKey))
        {
            _values.Clear();
            return;
        }

        if (string.Equals(_loadedResourceName, ResourceName, StringComparison.Ordinal)
            && string.Equals(_loadedLocalizationKey, LocalizationKey, StringComparison.Ordinal))
        {
            return;
        }

        await LoadValuesAsync();
    }

    public async Task<bool> ValidateAsync()
    {
        _validationMessage = string.Empty;

        if (!Required)
        {
            return true;
        }

        if (Mode == BusinessTextEditorMode.Literal)
        {
            if (string.IsNullOrWhiteSpace(LiteralValue))
            {
                _validationMessage = L["BusinessTextEditor:SaveRequired"].Value ?? string.Empty;
                await InvokeAsync(StateHasChanged);
                return false;
            }

            return true;
        }

        if (string.IsNullOrWhiteSpace(ResourceName) || string.IsNullOrWhiteSpace(LocalizationKey))
        {
            _validationMessage = L["BusinessTextEditor:SaveRequired"].Value ?? string.Empty;
            await InvokeAsync(StateHasChanged);
            return false;
        }

        await EnsureCulturesLoadedAsync();

        var defaultCulture = _cultures.FirstOrDefault(x => x.IsDefault)?.CultureName
                             ?? _cultures.FirstOrDefault()?.CultureName;

        if (!string.IsNullOrWhiteSpace(defaultCulture)
            && (!_values.TryGetValue(defaultCulture, out var defaultValue) || string.IsNullOrWhiteSpace(defaultValue)))
        {
            _validationMessage = L["BusinessTextEditor:DefaultCultureRequired"].Value ?? string.Empty;
            await InvokeAsync(StateHasChanged);
            return false;
        }

        return true;
    }

    public async Task SaveAsync()
    {
        if (Mode == BusinessTextEditorMode.Literal
            || Disabled
            || string.IsNullOrWhiteSpace(ResourceName)
            || string.IsNullOrWhiteSpace(LocalizationKey))
        {
            return;
        }

        await BusinessLocalizationEditorAppService.SaveKeyValuesAsync(new SaveBusinessLocalizationKeyValuesInput
        {
            ResourceName = ResourceName,
            Key = LocalizationKey,
            Values = new Dictionary<string, string>(_values, StringComparer.OrdinalIgnoreCase)
        });
    }

    public string GetStoredValue()
    {
        if (Mode == BusinessTextEditorMode.Literal)
        {
            return LiteralValue?.Trim() ?? string.Empty;
        }

        return LocalizationKey?.Trim() ?? string.Empty;
    }

    private async Task EnsureCulturesLoadedAsync()
    {
        if (_culturesLoaded)
        {
            return;
        }

        _cultures = await BusinessLocalizationEditorAppService.GetEditorCulturesAsync();
        _culturesLoaded = true;
    }

    private async Task LoadValuesAsync()
    {
        if (string.IsNullOrWhiteSpace(ResourceName) || string.IsNullOrWhiteSpace(LocalizationKey))
        {
            return;
        }

        _loading = true;
        await InvokeAsync(StateHasChanged);

        try
        {
            await EnsureCulturesLoadedAsync();

            var result = await BusinessLocalizationEditorAppService.GetKeyValuesAsync(new GetBusinessLocalizationKeyValuesInput
            {
                ResourceName = ResourceName,
                Key = LocalizationKey
            });

            _values = new Dictionary<string, string>(result.Values, StringComparer.OrdinalIgnoreCase);
            _loadedResourceName = ResourceName;
            _loadedLocalizationKey = LocalizationKey;
        }
        finally
        {
            _loading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private string GetCultureValue(string cultureName)
    {
        if (!_values.TryGetValue(cultureName, out var value))
        {
            value = string.Empty;
            _values[cultureName] = value;
        }

        return value;
    }

    private void SetCultureValue(string cultureName, string value)
    {
        _values[cultureName] = value;
    }

    private string GetCultureTabLabel(BusinessLocalizationCultureDto culture)
    {
        var label = culture.DisplayName;
        if (culture.IsDefault)
        {
            label += " *";
        }

        return label;
    }

    private async Task OnLiteralValueChangedAsync(string value)
    {
        LiteralValue = value;
        await LiteralValueChanged.InvokeAsync(value);
    }
}
