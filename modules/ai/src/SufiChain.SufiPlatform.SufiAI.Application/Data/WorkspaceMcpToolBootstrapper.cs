using System.Text.Json;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Data;

/// <summary>
/// Owns default-workspace MCP tool enablement inside the AI bounded context.
/// </summary>
public class WorkspaceMcpToolBootstrapper : IWorkspaceMcpToolBootstrapper, ITransientDependency
{
    protected IDefaultAiWorkspaceSeeder DefaultAiWorkspaceSeeder { get; }
    protected IWorkspaceRepository WorkspaceRepository { get; }
    protected ILogger<WorkspaceMcpToolBootstrapper> Logger { get; }

    public WorkspaceMcpToolBootstrapper(
        IDefaultAiWorkspaceSeeder defaultAiWorkspaceSeeder,
        IWorkspaceRepository workspaceRepository,
        ILogger<WorkspaceMcpToolBootstrapper> logger)
    {
        DefaultAiWorkspaceSeeder = defaultAiWorkspaceSeeder;
        WorkspaceRepository = workspaceRepository;
        Logger = logger;
    }

    public virtual async Task EnableToolsOnDefaultWorkspaceAsync(
        IReadOnlyList<string> toolNames,
        CancellationToken cancellationToken = default)
    {
        if (toolNames.Count == 0)
        {
            return;
        }

        var workspaceId = await DefaultAiWorkspaceSeeder.EnsureDefaultWorkspaceAsync(cancellationToken);
        if (!workspaceId.HasValue)
        {
            return;
        }

        var workspace = await WorkspaceRepository.GetAsync(workspaceId.Value, cancellationToken: cancellationToken);
        var enabledTools = ReadEnabledTools(workspace.EnabledMCPToolsJson);
        var changed = false;

        foreach (var toolName in toolNames)
        {
            if (string.IsNullOrWhiteSpace(toolName) ||
                enabledTools.Contains(toolName, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            enabledTools.Add(toolName);
            changed = true;
        }

        if (!changed)
        {
            return;
        }

        workspace.SetEnabledMCPTools(JsonSerializer.Serialize(enabledTools));
        await WorkspaceRepository.UpdateAsync(workspace, autoSave: true, cancellationToken: cancellationToken);

        Logger.LogInformation(
            "Enabled MCP tools on workspace '{WorkspaceName}' ({WorkspaceId}).",
            workspace.Name,
            workspace.Id);
    }

    protected virtual List<string> ReadEnabledTools(string? enabledToolsJson)
    {
        if (string.IsNullOrWhiteSpace(enabledToolsJson))
        {
            return new List<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(enabledToolsJson) ?? new List<string>();
        }
        catch (JsonException)
        {
            return new List<string>();
        }
    }
}
