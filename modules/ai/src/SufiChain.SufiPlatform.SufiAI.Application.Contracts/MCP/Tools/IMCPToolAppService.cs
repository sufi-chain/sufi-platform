using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Tools;

public interface IMCPToolAppService : IApplicationService
{
    /// <summary>
    /// Returns the current tenant's visible internal and enabled external tool catalog.
    /// List payloads omit <see cref="MCPToolDto.ParameterSchema"/> (use <see cref="GetAsync"/> for schema).
    /// </summary>
    Task<List<MCPToolDto>> GetCatalogAsync();

    /// <summary>
    /// Returns a single catalog tool including its parameter schema, or null when not found.
    /// </summary>
    Task<MCPToolDto?> GetAsync(string toolName);
    
    Task<MCPToolResolutionResultDto> ResolveAsync(MCPToolResolutionRequestDto request);
    
    Task<MCPToolExecutionResultDto> ExecuteToolAsync(MCPToolExecutionRequestDto request);
    
    Task RefreshToolRegistryAsync();
}
