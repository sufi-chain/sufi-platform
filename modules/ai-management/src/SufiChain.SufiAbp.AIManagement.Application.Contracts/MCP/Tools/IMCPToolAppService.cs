using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.AIManagement.MCP.Tools;

public interface IMCPToolAppService : IApplicationService
{
    Task<List<MCPToolDto>> GetToolsForWorkspaceAsync(string workspaceName);
    
    Task<MCPToolDto> GetToolAsync(string workspaceName, string toolName);
    
    Task<MCPToolExecutionResultDto> ExecuteToolAsync(MCPToolExecutionRequestDto request);
    
    Task RefreshToolRegistryAsync();
}
