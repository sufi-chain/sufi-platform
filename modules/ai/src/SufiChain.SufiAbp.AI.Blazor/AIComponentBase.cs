using SufiChain.SufiAbp.AI.Localization;
using SufiChain.SufiAbp.AI.MCP.Servers;
using SufiChain.SufiAbp.AI.MCP.Tools;
using SufiChain.SufiAbp.AI.RAG;
using SufiChain.SufiAbp.AI.Workspaces;
using SufiChain.SufiAbp.Data;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.SufiAbp.AI.Blazor;

public abstract class AIComponentBase : SufiAbpComponentBase
{
    protected AIComponentBase()
    {
        LocalizationResource = typeof(AIResource);
    }

    protected string ResolveMcpToolDisplayName(string toolName) =>
        McpToolLocalizationHelper.ResolveToolDisplayName(StringLocalizerFactory, toolName);

    protected string ResolveMcpToolDescription(string toolName, string fallbackDescription) =>
        McpToolLocalizationHelper.ResolveToolDescription(StringLocalizerFactory, toolName, fallbackDescription);

    protected string ResolveMcpToolSource(string toolName, string fallbackSource) =>
        McpToolLocalizationHelper.ResolveToolSource(StringLocalizerFactory, toolName, fallbackSource);

    protected string ResolveMcpToolType(string toolType) =>
        McpToolLocalizationHelper.ResolveToolType(StringLocalizerFactory, toolType);

    protected IWorkspaceAppService WorkspaceAppService =>
        LazyGetRequiredService(ref _workspaceAppService);
    private IWorkspaceAppService? _workspaceAppService;

    protected IRAGAppService RAGAppService =>
        LazyGetRequiredService(ref _ragAppService);
    private IRAGAppService? _ragAppService;
    
    protected IMCPToolAppService MCPToolAppService =>
        LazyGetRequiredService(ref _mcpToolAppService);
    private IMCPToolAppService? _mcpToolAppService;
    
    protected IMCPServerAppService MCPServerAppService =>
        LazyGetRequiredService(ref _mcpServerAppService);
    private IMCPServerAppService? _mcpServerAppService;
}
