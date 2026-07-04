using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.MenuManagement.Menus;

namespace SufiChain.SufiAbp.MenuManagement.Blazor.Components;

public partial class MenuCreateModal : MenuManagementComponentBase
{
    private static class LoadingKeys
    {
        public const string Create = "create";
    }

    private IMenuAppService MenuAppService => LazyGetRequiredService(ref _menuAppService);
    private IMenuAppService? _menuAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public EventCallback OnMenuCreated { get; set; }

    private CreateMenuDto _model = new();
    private string _contextIdText = string.Empty;

    protected override void OnParametersSet()
    {
        if (Open)
        {
            _model = new CreateMenuDto();
            _contextIdText = string.Empty;
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
        if (!string.IsNullOrWhiteSpace(_contextIdText))
        {
            if (!Guid.TryParse(_contextIdText, out var contextId))
            {
                await Message.ErrorAsync(L["InvalidEntityId"]);
                return;
            }
            _model.ContextId = contextId;
        }
        else
        {
            _model.ContextId = null;
        }

        await MenuAppService.CreateAsync(_model);
        await OnMenuCreated.InvokeAsync();
        await Hide();
    }, LoadingKeys.Create);
}
