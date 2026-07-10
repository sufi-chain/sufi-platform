using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.Data;
using SufiChain.SufiAbp.LocalizationManagement.Blazor.Public.Components;
using SufiChain.SufiAbp.LocalizationManagement.Blazor.Public.Models;
using SufiChain.SufiAbp.MenuManagement.Menus;
using System.Text.Json;

namespace SufiChain.SufiAbp.MenuManagement.Blazor.Components;

public partial class MenuItemCreateModal : MenuManagementComponentBase
{
    private static class LoadingKeys
    {
        public const string Create = "create";
    }

    private IMenuItemAppService MenuItemAppService => LazyGetRequiredService(ref _menuItemAppService);
    private IMenuItemAppService? _menuItemAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public Guid MenuId { get; set; }
    [Parameter] public Guid? ParentId { get; set; }
    [Parameter] public string? MenuKey { get; set; }
    [Parameter] public string? ResourceName { get; set; }
    [Parameter] public string? ContextType { get; set; }
    [Parameter] public IReadOnlyList<MenuItemOption> ParentOptions { get; set; } = new List<MenuItemOption>();
    [Parameter] public EventCallback OnItemCreated { get; set; }

    private CreateMenuItemDto _model = new();
    private string _displayOrderText = "0";
    private string _targetIdText = string.Empty;
    private string _parentIdText = string.Empty;
    private BusinessTextEditorMode _displayNameMode = BusinessTextEditorMode.Literal;
    private string? _localizationResourceName;
    private string? _localizationKey;
    private string? _literalDisplayName;
    private BusinessTextEditor? _displayNameEditor;

    protected override void OnParametersSet()
    {
        if (Open)
        {
            _model = new CreateMenuItemDto
            {
                MenuId = MenuId,
                ParentId = ParentId,
                IsActive = true,
                IsVisible = true
            };
            _displayOrderText = "0";
            _targetIdText = string.Empty;
            _parentIdText = ParentId?.ToString() ?? string.Empty;
            _displayNameMode = string.Equals(ContextType, "Public", StringComparison.OrdinalIgnoreCase)
                ? BusinessTextEditorMode.Localized
                : BusinessTextEditorMode.Literal;
            _literalDisplayName = string.Empty;
            UpdateLocalizationBinding();
        }
    }

    private Task Hide() => SetOpenAsync(false);

    private async Task SetOpenAsync(bool open)
    {
        Open = open;
        await OpenChanged.InvokeAsync(open);
    }

    private Task OnNameChangedAsync(string value)
    {
        _model.Name = value;
        UpdateLocalizationBinding();
        return Task.CompletedTask;
    }

    private Task OnSlugChangedAsync(string value)
    {
        _model.Slug = value;
        UpdateLocalizationBinding();
        return Task.CompletedTask;
    }

    private Task OnDisplayNameModeChangedAsync(BusinessTextEditorMode mode)
    {
        _displayNameMode = mode;
        UpdateLocalizationBinding();
        return Task.CompletedTask;
    }

    private Task OnLiteralDisplayNameChangedAsync(string? value)
    {
        _literalDisplayName = value;
        return Task.CompletedTask;
    }

    private void UpdateLocalizationBinding()
    {
        _localizationResourceName = ResourceName;

        if (string.IsNullOrWhiteSpace(MenuKey))
        {
            _localizationKey = null;
            return;
        }

        var itemSlug = MenuLocalizationKeyHelper.NormalizeItemSlug(_model.Slug, _model.Name);
        if (string.IsNullOrWhiteSpace(itemSlug))
        {
            _localizationKey = null;
            return;
        }

        _localizationKey = BusinessLocalizationKeys.SeededMenuItemDisplayName(MenuKey, itemSlug);
    }

    private Task OnValidSubmitAsync() => ExecuteWithLoadingAsync(async () =>
    {
        if (_displayNameEditor == null || !await _displayNameEditor.ValidateAsync())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_model.Name))
        {
            await Message.ErrorAsync(L["NameIsRequired"]);
            return;
        }

        if (!int.TryParse(_displayOrderText, out var displayOrder))
        {
            await Message.ErrorAsync(L["DisplayOrderMustBeNumber"]);
            return;
        }
        _model.DisplayOrder = displayOrder;

        if (string.IsNullOrWhiteSpace(_parentIdText))
        {
            _model.ParentId = null;
        }
        else if (Guid.TryParse(_parentIdText, out var parentId))
        {
            _model.ParentId = parentId;
        }

        if (!string.IsNullOrWhiteSpace(_targetIdText))
        {
            if (Guid.TryParse(_targetIdText, out var targetId))
            {
                _model.TargetId = targetId;
            }
        }
        else
        {
            _model.TargetId = null;
        }

        if (!string.IsNullOrWhiteSpace(_model.MetadataJson))
        {
            try
            {
                JsonDocument.Parse(_model.MetadataJson);
            }
            catch
            {
                await Message.ErrorAsync(L["InvalidJsonFormat"]);
                return;
            }
        }

        _model.DisplayName = _displayNameEditor.GetStoredValue();
        await MenuItemAppService.CreateAsync(_model);
        await _displayNameEditor.SaveAsync();
        await OnItemCreated.InvokeAsync();
        await Hide();
    }, LoadingKeys.Create);
}
