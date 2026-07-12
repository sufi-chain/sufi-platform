using SufiChain.SufiPlatform.SufiAI.Localization;
using SufiChain.SufiPlatform.SufiAI.MCP.Servers;
using SufiChain.SufiPlatform.SufiAI.MCP.Tools;
using SufiChain.SufiPlatform.SufiAI.RAG;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.SufiAI.Blazor;

public abstract class AIComponentBase : SufiComponentBase
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
