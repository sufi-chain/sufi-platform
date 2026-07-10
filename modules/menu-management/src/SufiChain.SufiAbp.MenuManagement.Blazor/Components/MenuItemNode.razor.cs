using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.MenuManagement.Menus;

namespace SufiChain.SufiAbp.MenuManagement.Blazor.Components;

public partial class MenuItemNode : MenuManagementComponentBase
{
    [Parameter] public MenuItemTreeDto Item { get; set; } = default!;
    [Parameter] public string? ContextType { get; set; }

    [Parameter] public EventCallback<MenuItemTreeDto> OnAddChild { get; set; }
    [Parameter] public EventCallback<MenuItemTreeDto> OnEdit { get; set; }
    [Parameter] public EventCallback<MenuItemTreeDto> OnMove { get; set; }
    [Parameter] public EventCallback<MenuItemTreeDto> OnDelete { get; set; }
    [Parameter] public EventCallback<MenuItemTreeDto> OnReorderUp { get; set; }
    [Parameter] public EventCallback<MenuItemTreeDto> OnReorderDown { get; set; }
}
