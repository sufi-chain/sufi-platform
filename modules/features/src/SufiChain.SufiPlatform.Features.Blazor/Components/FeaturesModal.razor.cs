using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Linq.Expressions;
using SufiChain.SufiPlatform.Features.Blazor;
using SufiChain.SufiPlatform.Features;
using Volo.Abp.Localization;
using Volo.Abp.Localization.External;
using SufiChain.SufiPlatform.Features;

namespace SufiChain.SufiPlatform.Features.Blazor.Components;

public partial class FeaturesModal
{
    private const string ToggleValueTypeName = "TOGGLE";
    private const string SelectionValueTypeName = "SELECTION";
    private const string ToggleStringValueTypeName = "ToggleStringValueType";
    private const string SelectionStringValueTypeName = "SelectionStringValueType";

    private static class LoadingKeys
    {
        public const string LoadFeatures = "load-features";
        public const string SaveFeatures = "save-features";
        public const string ResetFeatures = "reset-features";
    }

    [Inject] private IFeatureAppService FeatureAppService { get; set; } = default!;

    [Parameter] public EventCallback OnFeaturesSaved { get; set; }

    private bool _isOpen;
    private string _providerName = string.Empty;
    private string? _providerKey;
    private string _entityDisplayName = string.Empty;

    private List<FeatureGroupDto> _allGroups = new();
    private List<FeatureGroupDto> _groups = new();
    private List<FeatureDto> _disabledFeatures = new();
    private Dictionary<string, int> _featureDepths = new();

    private Dictionary<string, bool> _toggleValues = new();
    private Dictionary<string, string?> _textValues = new();
    private Dictionary<string, string?> _selectValues = new();

    private string? _selectedTabId;
    private string _searchText = string.Empty;
    private bool _selectAllDisabled;
    private bool _isClosing;

    private string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                FilterFeatureGroups();
            }
        }
    }

    /// <summary>
    /// Opens the feature management modal for a specific provider.
    /// </summary>
    /// <param name="providerName">The provider name ("T" for tenants)</param>
    /// <param name="providerKey">The provider key (tenant ID or null for host)</param>
    /// <param name="entityDisplayName">Display name for the entity (optional)</param>
    public async Task OpenAsync(string providerName, string? providerKey, string? entityDisplayName = null)
    {
        _isClosing = false;
        _providerName = providerName;
        _providerKey = providerKey;
        _entityDisplayName = entityDisplayName ?? L["Features"];
        _searchText = string.Empty;
        _selectedTabId = null;

        await ExecuteWithLoadingAsync(async () =>
        {
            var result = await FeatureAppService.GetAsync(_providerName, _providerKey);
            _allGroups = result.Groups.OrderBy(x => x.DisplayName).ToList();
            _groups = _allGroups.ToList();

            // Initialize value dictionaries
            _toggleValues.Clear();
            _textValues.Clear();
            _selectValues.Clear();

            foreach (var group in _allGroups)
            {
                foreach (var feature in group.Features)
                {
                    if (IsToggleFeature(feature))
                    {
                        _toggleValues[feature.Name] = feature.Value?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
                    }
                    else if (IsSelectionFeature(feature))
                    {
                        _selectValues[feature.Name] = feature.Value;
                    }
                    else
                    {
                        _textValues[feature.Name] = feature.Value;
                    }
                }
            }

            NormalizeFeatureGroups();
        }, LoadingKeys.LoadFeatures);

        _isOpen = true;
        StateHasChanged();
    }

    private void NormalizeFeatureGroups(bool checkDisabledFeatures = true)
    {
        _selectAllDisabled = _groups.All(IsFeatureGroupDisabled);

        if (checkDisabledFeatures)
        {
            _disabledFeatures.Clear();

            foreach (var feature in _groups.SelectMany(x => x.Features))
            {
                // Feature is disabled if it has a value set by a different provider (per ABP logic).
                // Exception: DefaultValueFeatureValueProvider ("D") - current scope can override defaults.
                if (feature.Value != null &&
                    feature.Provider != null &&
                    !string.IsNullOrEmpty(feature.Provider.Name) &&
                    feature.Provider.Name != _providerName &&
                    feature.Provider.Name != DefaultValueFeatureValueProvider.ProviderName)
                {
                    _disabledFeatures.Add(feature);
                }
            }
        }

        // Calculate feature depths for indentation
        foreach (var group in _groups)
        {
            SetFeatureDepths(group.Features, null, 0, new HashSet<string>());
        }

        // Select first tab if not already selected
        if (_groups.Count > 0 && string.IsNullOrEmpty(_selectedTabId))
        {
            _selectedTabId = GetNormalizedGroupName(_groups.First().Name);
        }
    }

    private void SetFeatureDepths(List<FeatureDto> features, string? currentParent, int currentDepth, HashSet<string> path)
    {
        foreach (var item in features)
        {
            if (item.ParentName == currentParent)
            {
                if (!path.Add(item.Name))
                {
                    continue;
                }

                _featureDepths[item.Name] = currentDepth;
                SetFeatureDepths(features, item.Name, currentDepth + 1, path);
                path.Remove(item.Name);
            }
        }
    }

    private int GetFeatureDepth(string featureName)
    {
        return _featureDepths.GetValueOrDefault(featureName, 0);
    }

    private string GetNormalizedGroupName(string name)
    {
        return "FeatureGroup_" + name.Replace(".", "_");
    }

    /// <summary>
    /// Gets the localized display text for a selection item (e.g. dropdown option).
    /// </summary>
    private string GetLocalizedSelectionItemText(FeatureSelectionItemDto item)
    {
        var localizer = StringLocalizerFactory.CreateByResourceNameOrNull(item.DisplayTextResourceName);

        return localizer?[item.DisplayTextName].Value ?? item.Value;
    }

    private bool IsFeatureDisabled(FeatureDto feature)
    {
        return _disabledFeatures.Contains(feature);
    }

    private bool IsFeatureGroupDisabled(FeatureGroupDto group)
    {
        var toggleFeatures = group.Features.Where(IsToggleFeature).ToList();
        if (!toggleFeatures.Any())
        {
            return true; // No toggle features to enable/disable
        }

        return toggleFeatures.All(f => _toggleValues.GetValueOrDefault(f.Name) && IsFeatureDisabled(f));
    }

    private bool HasToggleFeatures(FeatureGroupDto group)
    {
        return group.Features.Any(IsToggleFeature);
    }

    private bool AreAllToggleFeaturesEnabled(FeatureGroupDto group)
    {
        var toggleFeatures = group.Features.Where(IsToggleFeature);
        return toggleFeatures.All(f => _toggleValues.GetValueOrDefault(f.Name));
    }

    private string GetFeatureDisplayName(FeatureDto feature)
    {
        if (!IsFeatureDisabled(feature))
        {
            return feature.DisplayName;
        }

        // Show which provider set this feature
        return $"{feature.DisplayName} ({feature.Provider?.Name})";
    }

    private string? GetTextValue(string featureName)
    {
        return _textValues.GetValueOrDefault(featureName);
    }

    private void SetTextValue(string featureName, string? value)
    {
        _textValues[featureName] = value;
    }

    private Expression<Func<string?>> GetTextValueExpression(string featureName)
    {
        return () => _textValues[featureName];
    }

    private bool EnableAll => _allGroups
        .SelectMany(x => x.Features)
        .Where(IsToggleFeature)
        .All(f => _toggleValues.GetValueOrDefault(f.Name));

    private int GetEnabledCount(FeatureGroupDto group)
    {
        return group.Features
            .Where(IsToggleFeature)
            .Count(f => _toggleValues.GetValueOrDefault(f.Name));
    }

    private void FilterFeatureGroups()
    {
        if (string.IsNullOrWhiteSpace(_searchText))
        {
            _groups = _allGroups.ToList();
        }
        else
        {
            _groups = _allGroups
                .Where(g => g.DisplayName.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                           g.Features.Any(f => f.DisplayName.Contains(_searchText, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        NormalizeFeatureGroups(checkDisabledFeatures: false);
        StateHasChanged();
    }

    private async Task OnEnableAllChanged(bool value)
    {
        try
        {
        foreach (var feature in _allGroups.SelectMany(x => x.Features))
        {
            if (IsToggleFeature(feature) && !IsFeatureDisabled(feature))
            {
                _toggleValues[feature.Name] = value;
            }
        }

        // Reset search to show all groups
        _searchText = string.Empty;
        _groups = _allGroups.ToList();
        NormalizeFeatureGroups(checkDisabledFeatures: false);
        }
        catch (Exception ex)
        {
            await CloseOnUnhandledModalErrorAsync(ex);
        }
    }

    private async Task OnGroupEnableAllChanged(bool value, FeatureGroupDto group)
    {
        try
        {
        foreach (var feature in group.Features)
        {
            if (IsToggleFeature(feature) && !IsFeatureDisabled(feature))
            {
                _toggleValues[feature.Name] = value;
            }
        }

        StateHasChanged();
        }
        catch (Exception ex)
        {
            await CloseOnUnhandledModalErrorAsync(ex);
        }
    }

    private async Task OnToggleFeatureChanged(bool value, FeatureGroupDto group, FeatureDto feature)
    {
        try
        {
        _toggleValues[feature.Name] = value;

        if (value)
        {
            // Enable parent features recursively
            EnableParentFeatures(group, feature, new HashSet<string>());
        }
        else
        {
            // Disable child features recursively
            DisableChildFeatures(group, feature, new HashSet<string>());
        }

        StateHasChanged();
        }
        catch (Exception ex)
        {
            await CloseOnUnhandledModalErrorAsync(ex);
        }
    }

    private void EnableParentFeatures(FeatureGroupDto group, FeatureDto feature, HashSet<string> visited)
    {
        if (!visited.Add(feature.Name) || string.IsNullOrEmpty(feature.ParentName))
        {
            return;
        }

        var parentFeature = group.Features.FirstOrDefault(x => x.Name == feature.ParentName);
        if (parentFeature != null &&
            IsToggleFeature(parentFeature) &&
            !_toggleValues.GetValueOrDefault(parentFeature.Name))
        {
            _toggleValues[parentFeature.Name] = true;
            EnableParentFeatures(group, parentFeature, visited);
        }
    }

    private void DisableChildFeatures(FeatureGroupDto group, FeatureDto feature, HashSet<string> visited)
    {
        if (!visited.Add(feature.Name))
        {
            return;
        }

        var childFeatures = group.Features.Where(x => x.ParentName == feature.Name).ToList();

        foreach (var child in childFeatures)
        {
            if (IsToggleFeature(child) &&
                _toggleValues.GetValueOrDefault(child.Name) &&
                !IsFeatureDisabled(child))
            {
                _toggleValues[child.Name] = false;
                DisableChildFeatures(group, child, visited);
            }
        }
    }

    private async Task OnDialogOpenChanged(bool open)
    {
        if (!open)
        {
            await HideAsync();
        }
    }

    private Task HideAsync()
    {
        _selectedTabId = null;
        _isOpen = false;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task CloseOnUnhandledModalErrorAsync(Exception ex)
    {
        if (_isClosing)
        {
            return;
        }

        _isClosing = true;
        await HideAsync();
        await HandleErrorAsync(ex);
    }

    private Task SaveAsync() => ExecuteWithLoadingAsync(async () =>
    {
        var features = new List<UpdateFeatureDto>();

        foreach (var group in _allGroups)
        {
            foreach (var feature in group.Features)
            {
                string? value;

                if (IsToggleFeature(feature))
                {
                    value = _toggleValues.GetValueOrDefault(feature.Name) ? "true" : "false";
                }
                else if (IsSelectionFeature(feature))
                {
                    value = _selectValues.GetValueOrDefault(feature.Name);
                }
                else
                {
                    value = _textValues.GetValueOrDefault(feature.Name);
                }

                features.Add(new UpdateFeatureDto
                {
                    Name = feature.Name,
                    Value = value
                });
            }
        }

        await FeatureAppService.UpdateAsync(_providerName, _providerKey, new UpdateFeaturesDto
        {
            Features = features
        });

        await Notify.SuccessAsync(L["FeaturesSavedSuccessfully"]);
        await OnFeaturesSaved.InvokeAsync();
        await HideAsync();
    }, LoadingKeys.SaveFeatures);

    private async Task ResetToDefaultAsync()
    {
        if (!await Message.ConfirmAsync(L["ResetFeaturesConfirmation"]))
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await FeatureAppService.DeleteAsync(_providerName, _providerKey);
            await Notify.SuccessAsync(L["FeaturesResetSuccessfully"]);

            // Reload features after reset
            var result = await FeatureAppService.GetAsync(_providerName, _providerKey);
            _allGroups = result.Groups.OrderBy(x => x.DisplayName).ToList();
            _groups = _allGroups.ToList();

            // Re-initialize value dictionaries
            _toggleValues.Clear();
            _textValues.Clear();
            _selectValues.Clear();

            foreach (var group in _allGroups)
            {
                foreach (var feature in group.Features)
                {
                    if (IsToggleFeature(feature))
                    {
                        _toggleValues[feature.Name] = feature.Value?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
                    }
                    else if (IsSelectionFeature(feature))
                    {
                        _selectValues[feature.Name] = feature.Value;
                    }
                    else
                    {
                        _textValues[feature.Name] = feature.Value;
                    }
                }
            }

            NormalizeFeatureGroups();
        }, LoadingKeys.ResetFeatures);
    }

    private static bool IsToggleFeature(FeatureDto feature)
    {
        return string.Equals(feature.ValueType?.Name, ToggleValueTypeName, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(feature.ValueType?.Name, ToggleStringValueTypeName, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(feature.Value, "true", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(feature.Value, "false", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSelectionFeature(FeatureDto feature)
    {
        return string.Equals(feature.ValueType?.Name, SelectionValueTypeName, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(feature.ValueType?.Name, SelectionStringValueTypeName, StringComparison.OrdinalIgnoreCase);
    }
}
