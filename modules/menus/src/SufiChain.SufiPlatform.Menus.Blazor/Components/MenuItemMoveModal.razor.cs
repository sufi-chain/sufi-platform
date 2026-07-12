using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Menus.Menus;

namespace SufiChain.SufiPlatform.Menus.Blazor.Components;

public partial class MenuItemMoveModal : MenusComponentBase
{
    private static class LoadingKeys
    {
        public const string Move = "move";
    }

    private IMenuItemAppService MenuItemAppService => LazyGetRequiredService(ref _menuItemAppService);
    private IMenuItemAppService? _menuItemAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public MenuItemTreeDto? Item { get; set; }
    [Parameter] public IReadOnlyList<MenuItemOption> ParentOptions { get; set; } = new List<MenuItemOption>();
    [Parameter] public EventCallback OnItemMoved { get; set; }

    private Guid _itemId;
    private string _parentIdText = string.Empty;
    private string _displayOrderText = "0";

    protected override void OnParametersSet()
    {
        if (Open && Item != null && Item.Id != _itemId)
        {
            _itemId = Item.Id;
            _parentIdText = Item.ParentId?.ToString() ?? string.Empty;
            _displayOrderText = Item.DisplayOrder.ToString();
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
        if (!int.TryParse(_displayOrderText, out var displayOrder))
        {
            await Message.ErrorAsync(L["DisplayOrderMustBeNumber"]);
            return;
        }

        Guid? parentId = null;
        if (!string.IsNullOrWhiteSpace(_parentIdText) && Guid.TryParse(_parentIdText, out var parsed))
        {
            parentId = parsed;
        }

        await MenuItemAppService.MoveAsync(_itemId, new MoveMenuItemDto
        {
            ParentId = parentId,
            DisplayOrder = displayOrder
        });

        await OnItemMoved.InvokeAsync();
        await Hide();
    }, LoadingKeys.Move);
}
