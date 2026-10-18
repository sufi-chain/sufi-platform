using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using SufiChain.SufiBlazor.Components;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Components;

public partial class WorkspaceSelector : AIComponentBase
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

    private bool _appliedSingleWorkspace;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        if (_appliedSingleWorkspace || Value.HasValue || Loading)
        {
            return;
        }

        var active = FilteredWorkspaceList.Where(workspace => workspace.IsActive).ToList();
        if (active.Count != 1)
        {
            return;
        }

        _appliedSingleWorkspace = true;
        await ValueChanged.InvokeAsync(active[0].Id);
    }

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
        return $"{GetWorkspaceDisplayName(workspace)} · {GetProviderDisplayName(workspace.Provider)} · {workspace.Model}";
    }

    /// <summary>
    /// sufi-chain#413/#414: a tenant must not see the host's own name for a workspace it inherits
    /// (for example «زیرساخت صوفی»), so inherited workspaces show a neutral, localized label.
    /// </summary>
    private string GetWorkspaceDisplayName(WorkspaceDto workspace) =>
        workspace.IsInherited ? L["SharedPlatformWorkspace"] : workspace.Name;

    private async Task OnSelectChangedAsync(Guid? workspaceId)
    {
        if (Value != workspaceId)
        {
            Value = workspaceId;
            await ValueChanged.InvokeAsync(Value);
        }
    }
}
