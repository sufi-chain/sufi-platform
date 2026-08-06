using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Menus.Menus;

namespace SufiChain.SufiPlatform.Menus.Blazor.Components;

public partial class MenuNavigationCards : MenusComponentBase
{
    [Parameter] public List<MenuItemTreeDto> Items { get; set; } = [];
    [Parameter] public string? ContextType { get; set; }
}
