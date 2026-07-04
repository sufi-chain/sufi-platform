using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.MenuManagement.Menus;
using System.Text.Json;

namespace SufiChain.SufiAbp.MenuManagement.Blazor.Components;

public partial class MenuItemEditModal : MenuManagementComponentBase
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
    [Parameter] public IReadOnlyList<MenuItemOption> ParentOptions { get; set; } = new List<MenuItemOption>();
    [Parameter] public EventCallback OnItemUpdated { get; set; }

    private UpdateMenuItemDto _model = new();
    private Guid _itemId;
    private string _displayOrderText = "0";
    private string _targetIdText = string.Empty;
    private string _parentIdText = string.Empty;

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
        }
    }

    private Task Hide() => SetOpenAsync(false);

    private async Task SetOpenAsync(bool open)
    {
        Open = open;
        await OpenChanged.InvokeAsync(open);
    }

    private Task OnValidSubmitAsync() => ExecuteWithLoadingAsync(async () =>
    {
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

        await MenuItemAppService.UpdateAsync(_itemId, _model);
        await OnItemUpdated.InvokeAsync();
        await Hide();
    }, LoadingKeys.Save);
}
