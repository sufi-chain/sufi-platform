using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Menus.Menus;

namespace SufiChain.SufiPlatform.Menus.Blazor.Components;

public partial class MenuItemSelector
{
    [Inject] protected IPublicMenuAppService PublicMenuAppService { get; set; } = null!;
    [Parameter] public string ContextType { get; set; } = string.Empty;
    [Parameter] public Guid? ContextId { get; set; }
    [Parameter] public string MenuName { get; set; } = string.Empty;
    [Parameter] public Guid? SelectedItemId { get; set; }
    [Parameter] public EventCallback<Guid?> SelectedItemIdChanged { get; set; }
    [Parameter] public bool AllowEmpty { get; set; } = true;
    [Parameter] public string Placeholder { get; set; } = "Select...";
    protected List<PublicMenuItemTreeDto> Items { get; set; } = [];
    protected List<PublicMenuItemTreeDto> FlattenedItems { get; set; } = [];
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        if (!string.IsNullOrWhiteSpace(ContextType) && !string.IsNullOrWhiteSpace(MenuName))
        {
            Items = await PublicMenuAppService.GetTreeAsync(ContextType, ContextId, MenuName);
            FlattenedItems = Flatten(Items).ToList();
        }
    }
    protected virtual Task OnChangedAsync(Guid? value)
        => SelectedItemIdChanged.InvokeAsync(value);
    protected virtual IEnumerable<PublicMenuItemTreeDto> Flatten(IEnumerable<PublicMenuItemTreeDto> items)
    {
        foreach (var item in items)
        {
            yield return item;
            foreach (var child in Flatten(item.Children)) yield return child;
        }
    }
}
