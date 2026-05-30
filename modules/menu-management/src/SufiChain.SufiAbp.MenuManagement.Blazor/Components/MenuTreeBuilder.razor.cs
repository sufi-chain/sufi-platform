using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.MenuManagement.Menus;

namespace SufiChain.SufiAbp.MenuManagement.Blazor.Components;

public partial class MenuTreeBuilder
{
    [Inject] protected IMenuItemAppService MenuItemAppService { get; set; } = null!;
    [Parameter] public Guid MenuId { get; set; }
    protected List<MenuItemTreeDto> Items { get; set; } = [];
    protected bool DialogOpen { get; set; }
    protected CreateMenuItemDto EditModel { get; set; } = new();
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        if (MenuId != Guid.Empty) Items = await MenuItemAppService.GetTreeAsync(new GetMenuTreeInput { MenuId = MenuId });
    }
    protected virtual Task SelectAsync(MenuItemTreeDto item) => Task.CompletedTask;
    protected virtual Task SetDialogOpenAsync(bool open) { DialogOpen = open; return Task.CompletedTask; }
    protected virtual Task NewRootAsync() { EditModel = new CreateMenuItemDto { MenuId = MenuId, Kind = MenuItemKind.Container, DisplayType = MenuItemDisplayType.Default }; DialogOpen = true; return Task.CompletedTask; }
    protected virtual async Task SaveAsync(CreateMenuItemDto model)
    {
        await MenuItemAppService.CreateAsync(model);
        Items = await MenuItemAppService.GetTreeAsync(new GetMenuTreeInput { MenuId = MenuId });
        DialogOpen = false;
    }
}
