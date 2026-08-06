using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Menus.Menus;

namespace SufiChain.SufiPlatform.Menus.Blazor.Components;

public partial class MenuSidebarNavigation
{
    [Parameter] public List<MenuItemTreeDto> Items { get; set; } = [];
    [Parameter] public EventCallback<MenuItemTreeDto> OnItemSelected { get; set; }
}
