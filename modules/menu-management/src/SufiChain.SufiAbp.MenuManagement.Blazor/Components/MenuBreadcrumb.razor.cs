using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.MenuManagement.Menus;

namespace SufiChain.SufiAbp.MenuManagement.Blazor.Components;

public partial class MenuBreadcrumb
{
    [Parameter] public List<MenuItemTreeDto> Tree { get; set; } = [];
    [Parameter] public Guid? SelectedItemId { get; set; }
    protected List<MenuItemTreeDto> Trail { get; set; } = [];
    protected override Task OnParametersSetAsync()
    {
        Trail = [];
        if (SelectedItemId.HasValue) FindTrail(Tree, SelectedItemId.Value, []);
        return base.OnParametersSetAsync();
    }
    protected virtual bool FindTrail(List<MenuItemTreeDto> items, Guid id, List<MenuItemTreeDto> trail)
    {
        foreach (var item in items)
        {
            var next = trail.Concat([item]).ToList();
            if (item.Id == id) { Trail = next; return true; }
            if (FindTrail(item.Children, id, next)) return true;
        }
        return false;
    }
}
