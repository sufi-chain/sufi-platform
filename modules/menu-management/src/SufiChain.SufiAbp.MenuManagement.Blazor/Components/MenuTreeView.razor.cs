using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.MenuManagement.Menus;

namespace SufiChain.SufiAbp.MenuManagement.Blazor.Components;

public partial class MenuTreeView
{
    [Parameter] public List<MenuItemTreeDto> Items { get; set; } = [];
    [Parameter] public EventCallback<MenuItemTreeDto> OnItemSelected { get; set; }
    protected virtual Task SelectAsync(MenuItemTreeDto item) => OnItemSelected.InvokeAsync(item);
}
