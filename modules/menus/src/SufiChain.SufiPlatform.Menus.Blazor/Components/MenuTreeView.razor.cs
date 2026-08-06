using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Menus.Menus;

namespace SufiChain.SufiPlatform.Menus.Blazor.Components;

public partial class MenuTreeView : MenusComponentBase
{
    [Parameter] public List<MenuItemTreeDto> Items { get; set; } = [];
    [Parameter] public string? ContextType { get; set; }
    [Parameter] public EventCallback<MenuItemTreeDto> OnItemSelected { get; set; }
    protected virtual Task SelectAsync(MenuItemTreeDto item) => OnItemSelected.InvokeAsync(item);
}
