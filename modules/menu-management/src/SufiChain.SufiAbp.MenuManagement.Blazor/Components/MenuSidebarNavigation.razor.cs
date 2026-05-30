using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.MenuManagement.Menus;

namespace SufiChain.SufiAbp.MenuManagement.Blazor.Components;

public partial class MenuSidebarNavigation
{
    [Parameter] public List<MenuItemTreeDto> Items { get; set; } = [];
    [Parameter] public EventCallback<MenuItemTreeDto> OnItemSelected { get; set; }
}
