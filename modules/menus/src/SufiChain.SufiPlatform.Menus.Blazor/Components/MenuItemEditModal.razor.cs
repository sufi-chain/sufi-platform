using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Localization.Blazor.Public.Components;
using SufiChain.SufiPlatform.Localization.Blazor.Public.Models;
using SufiChain.SufiPlatform.Menus.Menus;
using System.Text.Json;

namespace SufiChain.SufiPlatform.Menus.Blazor.Components;

public partial class MenuItemEditModal : MenusComponentBase
{
    private static class LoadingKeys
    {
        public const string Save = "save";
    }

    private IMenuItemAppService MenuItemAppService => LazyGetRequiredService(ref _menuItemAppService);
    private IMenuItemAppService? _menuItemAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public MenuItemTreeDto? Item { get; set; }
    [Parameter] public string? MenuKey { get; set; }
    [Parameter] public string? ResourceName { get; set; }
    [Parameter] public string? ContextType { get; set; }
    [Parameter] public IReadOnlyList<MenuItemOption> ParentOptions { get; set; } = new List<MenuItemOption>();
    [Parameter] public EventCallback OnItemUpdated { get; set; }

    private UpdateMenuItemDto _model = new();
    private Guid _itemId;
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
        if (Open && Item != null && Item.Id != _itemId)
        {
            _itemId = Item.Id;
            _model = new UpdateMenuItemDto
            {
                ParentId = Item.ParentId,
                Name = Item.Name,
                DisplayName = Item.DisplayName,
                Slug = Item.Slug,
                Description = Item.Description,
                DisplayOrder = Item.DisplayOrder,
                Kind = Item.Kind,
                DisplayType = Item.DisplayType,
                Url = Item.Url,
                LinkTarget = Item.LinkTarget,
                TargetType = Item.TargetType,
                TargetId = Item.TargetId,
                Icon = Item.Icon,
                CssClass = Item.CssClass,
                PermissionName = Item.PermissionName,
                ComponentName = Item.ComponentName,
                MetadataJson = Item.MetadataJson,
                IsActive = Item.IsActive,
                IsVisible = Item.IsVisible
            };
            _displayOrderText = Item.DisplayOrder.ToString();
            _targetIdText = Item.TargetId?.ToString() ?? string.Empty;
            _parentIdText = Item.ParentId?.ToString() ?? string.Empty;

            InitializeDisplayNameEditor(Item);
        }
    }

    private void InitializeDisplayNameEditor(MenuItemTreeDto item)
    {
        _localizationResourceName = ResourceName;

        if (BusinessLocalizationHelper.IsBusinessLocalizationKey(item.DisplayName))
        {
            _displayNameMode = BusinessTextEditorMode.Localized;
            _localizationKey = item.DisplayName;
            _literalDisplayName = string.Empty;
            return;
        }

        _displayNameMode = BusinessTextEditorMode.Literal;
        _literalDisplayName = item.DisplayName;
        _localizationKey = null;
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
        if (_displayNameMode != BusinessTextEditorMode.Localized
            || string.IsNullOrWhiteSpace(MenuKey))
        {
            return;
        }

        _localizationResourceName = ResourceName;

        if (BusinessLocalizationHelper.IsBusinessLocalizationKey(_localizationKey))
        {
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
        await MenuItemAppService.UpdateAsync(_itemId, _model);
        await _displayNameEditor.SaveAsync();
        await OnItemUpdated.InvokeAsync();
        await Hide();
    }, LoadingKeys.Save);
}
