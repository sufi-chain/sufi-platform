using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.AIManagement.Workspaces;
using SufiChain.SufiBlazor.Components;

namespace SufiChain.SufiAbp.AIManagement.Blazor.Components;

public partial class WorkspaceSelector : AIManagementComponentBase
{
    [Parameter] public Guid? Value { get; set; }
    [Parameter] public EventCallback<Guid?> ValueChanged { get; set; }
    [Parameter] public List<WorkspaceDto> Workspaces { get; set; } = new();
    [Parameter] public bool Loading { get; set; }
    [Parameter] public bool ShowInactiveWorkspaces { get; set; } = false;
    [Parameter] public string? EmptyMessage { get; set; }

    private const int CompactChipThreshold = 3;

    private IEnumerable<WorkspaceDto> FilteredWorkspaces =>
        ShowInactiveWorkspaces
            ? Workspaces
            : Workspaces.Where(w => w.IsActive);

    private List<WorkspaceDto> FilteredWorkspaceList => FilteredWorkspaces.ToList();

    private bool UseCompactChips => FilteredWorkspaceList.Count <= CompactChipThreshold;

    private async Task OnWorkspaceClickAsync(Guid workspaceId)
    {
        if (Value != workspaceId)
        {
            Value = workspaceId;
            await ValueChanged.InvokeAsync(Value);
        }
    }

    private string GetProviderIcon(AIProviderType provider)
    {
        return provider switch
        {
            AIProviderType.OpenAI => "cloud",
            _ => "box"
        };
    }

    private SbColor GetProviderColor(AIProviderType provider)
    {
        return provider switch
        {
            AIProviderType.OpenAI => SbColor.Primary,
            _ => SbColor.Default
        };
    }

    private string GetProviderDisplayName(AIProviderType provider)
    {
        return provider switch
        {
            AIProviderType.OpenAI => "OpenAI",
            _ => provider.ToString()
        };
    }

    private string GetWorkspaceOptionText(WorkspaceDto workspace)
    {
        return $"{workspace.Name} · {GetProviderDisplayName(workspace.Provider)} · {workspace.Model}";
    }

    private async Task OnSelectChangedAsync(Guid? workspaceId)
    {
        if (Value != workspaceId)
        {
            Value = workspaceId;
            await ValueChanged.InvokeAsync(Value);
        }
    }
}
