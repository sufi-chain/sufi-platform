using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.MenuManagement.Menus;

namespace SufiChain.SufiAbp.MenuManagement.Blazor.Components;

public partial class MenuNavigationCards : MenuManagementComponentBase
{
    [Parameter] public List<MenuItemTreeDto> Items { get; set; } = [];
    [Parameter] public string? ContextType { get; set; }
}
