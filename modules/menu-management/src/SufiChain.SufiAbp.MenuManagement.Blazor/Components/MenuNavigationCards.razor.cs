using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.MenuManagement.Menus;

namespace SufiChain.SufiAbp.MenuManagement.Blazor.Components;

public partial class MenuNavigationCards
{
    [Parameter] public List<MenuItemTreeDto> Items { get; set; } = [];
}
