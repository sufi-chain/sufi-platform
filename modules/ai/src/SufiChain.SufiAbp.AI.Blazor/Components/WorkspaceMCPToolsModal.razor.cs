using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.AI.MCP.Tools;
using SufiChain.SufiAbp.AI.Workspaces;

namespace SufiChain.SufiAbp.AI.Blazor.Components;

public partial class WorkspaceMCPToolsModal
{
    [Inject] private IWorkspaceAppService WorkspaceAppService { get; set; } = default!;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public Guid? WorkspaceId { get; set; }
    [Parameter] public EventCallback OnSaved { get; set; }

    private static class LoadingKeys
    {
        public const string Load = nameof(Load);
        public const string Save = nameof(Save);
    }

    private Guid? _loadedWorkspaceId;
    private string? _filter;
    private List<MCPToolDto> _availableTools = new();
    private HashSet<string> _enabledToolNames = new();

    private IReadOnlyList<MCPToolDto> FilteredTools
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_filter))
            {
                return _availableTools;
            }

            return _availableTools
                .Where(tool =>
                    tool.Name.Contains(_filter, StringComparison.OrdinalIgnoreCase) ||
                    ResolveMcpToolDisplayName(tool.Name).Contains(_filter, StringComparison.OrdinalIgnoreCase) ||
                    ResolveMcpToolDescription(tool.Name, tool.Description).Contains(_filter, StringComparison.OrdinalIgnoreCase) ||
                    tool.Source.Contains(_filter, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!Open || !WorkspaceId.HasValue || _loadedWorkspaceId == WorkspaceId)
        {
            return;
        }

        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (!WorkspaceId.HasValue)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            var configuration = await WorkspaceAppService.GetMCPToolConfigurationAsync(WorkspaceId.Value);
            _availableTools = configuration.AvailableTools
                .OrderBy(tool => tool.Source)
                .ThenBy(tool => tool.Name)
                .ToList();
            _enabledToolNames = configuration.EnabledToolNames.ToHashSet();
            _loadedWorkspaceId = WorkspaceId;
        }, LoadingKeys.Load);
    }

    private bool IsSelected(string toolName)
    {
        return _enabledToolNames.Contains(toolName);
    }

    private Task ToggleToolAsync(string toolName, bool enabled)
    {
        if (enabled)
        {
            _enabledToolNames.Add(toolName);
        }
        else
        {
            _enabledToolNames.Remove(toolName);
        }

        return InvokeAsync(StateHasChanged);
    }

    private Task SelectAllAsync()
    {
        _enabledToolNames = _availableTools.Select(tool => tool.Name).ToHashSet();
        return InvokeAsync(StateHasChanged);
    }

    private Task ClearAllAsync()
    {
        _enabledToolNames.Clear();
        return InvokeAsync(StateHasChanged);
    }

    private async Task SaveAsync()
    {
        if (!WorkspaceId.HasValue)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await WorkspaceAppService.UpdateMCPToolConfigurationAsync(
                WorkspaceId.Value,
                new UpdateWorkspaceMCPToolConfigurationDto
                {
                    EnabledToolNames = _enabledToolNames.OrderBy(toolName => toolName).ToList()
                });

            await OnSaved.InvokeAsync();
            await CloseAsync();
        }, LoadingKeys.Save);
    }

    private async Task CancelAsync()
    {
        await CloseAsync();
    }

    private async Task CloseAsync()
    {
        await SetOpenAsync(false);
        _loadedWorkspaceId = null;
        _filter = null;
        _availableTools.Clear();
        _enabledToolNames.Clear();
    }

    private async Task SetOpenAsync(bool open)
    {
        Open = open;
        await OpenChanged.InvokeAsync(open);
    }
}
