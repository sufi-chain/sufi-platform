using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.MenuManagement.Menus;

namespace SufiChain.SufiAbp.MenuManagement.Blazor.Components;

public partial class MenuItemEditDialog
{
    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public CreateMenuItemDto Model { get; set; } = new();
    [Parameter] public EventCallback<CreateMenuItemDto> OnSave { get; set; }
    protected virtual Task SaveAsync() => OnSave.InvokeAsync(Model);
    protected virtual Task CloseAsync() => OpenChanged.InvokeAsync(false);
}
