using System.Linq;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.SufiAI.Hooshvare;
using SufiChain.SufiPlatform.SufiAI.MCP.Tools;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using SufiChain.SufiPlatform.SufiAI.RAG;
using SufiChain.SufiPlatform.SufiAI.Workspaces;

namespace SufiChain.SufiPlatform.SufiAI.Mcp;

[Authorize]
public class WorkspaceReadinessMcpAppService : SufiApplicationService
{
    private const int MaxResults = 50;

    protected IWorkspaceAppService Workspaces { get; }
    protected IAIModelCatalogAppService ModelCatalog { get; }
    protected IRAGAppService Rag { get; }
    protected IMCPToolAppService McpTools { get; }

    public WorkspaceReadinessMcpAppService(
        IWorkspaceAppService workspaces,
        IAIModelCatalogAppService modelCatalog,
        IRAGAppService rag,
        IMCPToolAppService mcpTools)
    {
        Workspaces = workspaces;
        ModelCatalog = modelCatalog;
        Rag = rag;
        McpTools = mcpTools;
    }

    [SufiAiMcpTool(WorkspaceReadinessHooshvareKeys.Tools.ListWorkspaces,
        "Lists AI workspaces without API keys or endpoint URLs.", ReadOnly = true)]
    [Authorize(AIPermissions.Workspaces.Default)]
    public virtual async Task<object> ListWorkspacesAsync()
    {
        var page = await Workspaces.GetListAsync(new PagedAndSortedResultRequestDto
        {
            MaxResultCount = MaxResults
        });
        return new
        {
            page.TotalCount,
            Items = page.Items.Select(workspace => new
            {
                workspace.Id,
                workspace.Name,
                Provider = workspace.Provider.ToString(),
                workspace.Model,
                workspace.IsActive,
                workspace.HasApiKey,
                workspace.IsInherited
            }).ToList()
        };
    }

    [SufiAiMcpTool(WorkspaceReadinessHooshvareKeys.Tools.GetWorkspaceReadiness,
        "Returns readiness flags for one workspace. HasApiKey is a boolean and never the key.", ReadOnly = true)]
    [Authorize(AIPermissions.Workspaces.Default)]
    public virtual async Task<WorkspaceReadinessDto> GetWorkspaceReadinessAsync(Guid workspaceId)
    {
        return await Workspaces.GetReadinessAsync(workspaceId);
    }

    [SufiAiMcpTool(WorkspaceReadinessHooshvareKeys.Tools.ListModelCapabilities,
        "Lists selectable chat routes for a workspace, or readiness capabilities when no workspace is given. Omits secrets.", ReadOnly = true)]
    [Authorize(AIPermissions.Workspaces.Default)]
    public virtual async Task<object> ListModelCapabilitiesAsync(Guid? workspaceId = null)
    {
        if (workspaceId.HasValue)
        {
            var routes = await ModelCatalog.GetSelectableRoutesAsync(new GetSelectableModelRoutesInput
            {
                WorkspaceId = workspaceId.Value,
                CapabilityType = AICapabilityType.ChatCompletion
            });
            return routes.Select(route => new
            {
                route.Id,
                route.ModelId,
                route.DisplayName,
                Capability = route.CapabilityType.ToString(),
                route.IsDefault,
                route.IsReady,
                route.UnavailableReason,
                route.SupportsToolCalling,
                route.AcceptsImageInput,
                route.AcceptsFileInput,
                route.SupportsReasoning
            }).ToList();
        }

        var page = await Workspaces.GetListAsync(new PagedAndSortedResultRequestDto { MaxResultCount = MaxResults });
        var items = new List<object>();
        foreach (var workspace in page.Items)
        {
            var readiness = await Workspaces.GetReadinessAsync(workspace.Id);
            items.Add(new
            {
                workspace.Id,
                workspace.Name,
                Capabilities = readiness.Capabilities.Select(capability => new
                {
                    Capability = capability.CapabilityType.ToString(),
                    capability.IsReady,
                    Provider = capability.Provider.ToString(),
                    capability.ModelId,
                    capability.HasApiKey,
                    capability.HasApiEndpoint,
                    capability.FailureCode
                }).ToList()
            });
        }

        return items;
    }

    [SufiAiMcpTool(WorkspaceReadinessHooshvareKeys.Tools.GetRagAvailability,
        "Returns whether retrieval-augmented generation is available. Does not return provider payloads.", ReadOnly = true)]
    [Authorize(AIPermissions.RAG.Default)]
    public virtual async Task<object> GetRagAvailabilityAsync()
    {
        var availability = await Rag.GetAvailabilityAsync();
        return new
        {
            availability.IsAvailable,
            Provider = availability.Provider.ToString(),
            availability.Message
        };
    }

    [SufiAiMcpTool(WorkspaceReadinessHooshvareKeys.Tools.GetMcpCatalogSummary,
        "Summarizes visible MCP tools by name, type, and source. Omits parameter schemas and argument JSON.", ReadOnly = true)]
    [Authorize(AIPermissions.MCPTools.Default)]
    public virtual async Task<object> GetMcpCatalogSummaryAsync()
    {
        var catalog = await McpTools.GetCatalogAsync();
        return new
        {
            Count = catalog.Count,
            Items = catalog.Take(MaxResults).Select(tool => new
            {
                tool.Name,
                Description = tool.Description.Length <= 240 ? tool.Description : tool.Description[..240],
                tool.ToolType,
                tool.Source
            }).ToList()
        };
    }
}
