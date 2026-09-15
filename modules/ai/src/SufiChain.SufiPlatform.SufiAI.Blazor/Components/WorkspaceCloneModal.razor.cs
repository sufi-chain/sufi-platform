using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.SufiAI.Workspaces;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Components;

public partial class WorkspaceCloneModal : AIComponentBase
{
    private static class LoadingKeys
    {
        public const string Clone = "clone-workspace";
    }

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public Guid? WorkspaceId { get; set; }
    [Parameter] public string? SourceName { get; set; }
    [Parameter] public EventCallback OnCloned { get; set; }

    private IWorkspaceAppService WorkspaceAppService => LazyGetRequiredService(ref _workspaceAppService);
    private IWorkspaceAppService? _workspaceAppService;

    private string _name = string.Empty;
    private bool _wasOpen;

    protected override void OnParametersSet()
    {
        if (Open && !_wasOpen)
        {
            _name = string.IsNullOrWhiteSpace(SourceName)
                ? string.Empty
                : L["CloneWorkspaceName", SourceName];
        }

        _wasOpen = Open;
    }

    private async Task CloneAsync()
    {
        if (!WorkspaceId.HasValue)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_name))
        {
            await Message.ErrorAsync(L["WorkspaceNameRequired"]);
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await WorkspaceAppService.CloneAsync(WorkspaceId.Value, new CloneWorkspaceDto { Name = _name.Trim() });
            await CloseModal();
            await OnCloned.InvokeAsync();
        }, LoadingKeys.Clone);
    }

    private Task CloseModal() => SetOpenAsync(false);

    private async Task SetOpenAsync(bool open)
    {
        Open = open;
        _wasOpen = open;
        await OpenChanged.InvokeAsync(open);
    }
}
