using SufiChain.SufiAbp.AIManagement.Localization;
using SufiChain.SufiAbp.AIManagement.MCP.Servers;
using SufiChain.SufiAbp.AIManagement.MCP.Tools;
using SufiChain.SufiAbp.AIManagement.RAG;
using SufiChain.SufiAbp.AIManagement.Workspaces;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.SufiAbp.AIManagement.Blazor;

public abstract class AIManagementComponentBase : SufiAbpComponentBase
{
    protected AIManagementComponentBase()
    {
        LocalizationResource = typeof(AIManagementResource);
    }

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
