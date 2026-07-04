using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.MenuManagement.Menus;

namespace SufiChain.SufiAbp.MenuManagement.Blazor.Components;

public partial class MenuEditModal : MenuManagementComponentBase
{
    private static class LoadingKeys
    {
        public const string Save = "save";
    }

    private IMenuAppService MenuAppService => LazyGetRequiredService(ref _menuAppService);
    private IMenuAppService? _menuAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public MenuDto? Menu { get; set; }
    [Parameter] public EventCallback OnMenuUpdated { get; set; }

    private UpdateMenuDto _model = new();
    private Guid _menuId;

    protected override void OnParametersSet()
    {
        if (Open && Menu != null && Menu.Id != _menuId)
        {
            _menuId = Menu.Id;
            _model = new UpdateMenuDto
            {
                DisplayName = Menu.DisplayName,
                Description = Menu.Description,
                IsActive = Menu.IsActive
            };
        }
    }

    private Task Hide() => SetOpenAsync(false);

    private async Task SetOpenAsync(bool open)
    {
        Open = open;
        await OpenChanged.InvokeAsync(open);
    }

    private Task OnValidSubmitAsync() => ExecuteWithLoadingAsync(async () =>
    {
        await MenuAppService.UpdateAsync(_menuId, _model);
        await OnMenuUpdated.InvokeAsync();
        await Hide();
    }, LoadingKeys.Save);
}
