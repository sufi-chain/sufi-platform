using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Tools;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Features;

namespace SufiChain.SufiPlatform.SufiAI.Application.MCP.Tools;

[RequiresFeature(SufiAIFeatures.Enable)]
[Authorize(AIPermissions.MCPTools.Default)]
public class MCPToolAppService : SufiApplicationService, IMCPToolAppService
{
    private readonly IMCPToolRegistry _toolRegistry;
    private readonly IMCPToolExecutor _toolExecutor;
    
    public MCPToolAppService(
        IMCPToolRegistry toolRegistry,
        IMCPToolExecutor toolExecutor)
    {
        _toolRegistry = toolRegistry;
        _toolExecutor = toolExecutor;
    }
    
    public async Task<List<MCPToolDto>> GetCatalogAsync()
    {
        var tools = await _toolRegistry.GetCatalogAsync();
        // Listing pages / test-chat pickers do not need JSON schemas — keep the Blazor
        // circuit payload small so render cannot stall the dispatcher.
        return MapTools(tools, includeParameterSchema: false);
    }

    public async Task<MCPToolDto?> GetAsync(string toolName)
    {
        if (string.IsNullOrWhiteSpace(toolName))
        {
            return null;
        }

        var tools = await _toolRegistry.GetCatalogAsync();
        var match = tools.FirstOrDefault(tool =>
            string.Equals(tool.Name, toolName.Trim(), StringComparison.OrdinalIgnoreCase));
        return match == null ? null : MapTool(match, includeParameterSchema: true);
    }

    public async Task<MCPToolResolutionResultDto> ResolveAsync(MCPToolResolutionRequestDto request)
    {
        var result = await _toolRegistry.ResolveAsync(request.ToolNames);
        return new MCPToolResolutionResultDto
        {
            Tools = MapTools(result.Tools, includeParameterSchema: true),
            Diagnostics = result.Diagnostics.Select(diagnostic => new MCPToolResolutionDiagnosticDto
            {
                ToolName = diagnostic.ToolName,
                Code = diagnostic.Code,
                Message = diagnostic.Message
            }).ToList()
        };
    }

    private static List<MCPToolDto> MapTools(IEnumerable<IMCPTool> tools, bool includeParameterSchema)
    {
        return tools.Select(tool => MapTool(tool, includeParameterSchema)).ToList();
    }

    private static MCPToolDto MapTool(IMCPTool tool, bool includeParameterSchema)
    {
        return new MCPToolDto
        {
            Name = tool.Name,
            Description = tool.Description,
            ParameterSchema = includeParameterSchema ? tool.ParameterSchema : string.Empty,
            ToolType = tool.ToolType.ToString(),
            Source = tool.Source
        };
    }
    
    public async Task<MCPToolExecutionResultDto> ExecuteToolAsync(MCPToolExecutionRequestDto request)
    {
        var result = await _toolExecutor.ExecuteAsync(
            request.WorkspaceName,
            request.ToolName,
            request.Parameters
        );
        
        return new MCPToolExecutionResultDto
        {
            Success = result.Success,
            Result = result.Result,
            ErrorMessage = result.ErrorMessage,
            ExceptionDetails = result.ExceptionDetails,
            ExecutionTimeMs = result.ExecutionTimeMs,
            ExecutedAt = result.ExecutedAt
        };
    }
    
    public async Task RefreshToolRegistryAsync()
    {
        await _toolRegistry.RefreshAsync();
    }
}
