using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiAbp.AIManagement.MCP.Abstractions;
using SufiChain.SufiAbp.AIManagement.MCP.Tools;
using SufiChain.SufiAbp.AIManagement.Permissions;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.AIManagement.Application.MCP.Tools;

[Authorize(AIManagementPermissions.Workspaces.Default)]
public class MCPToolAppService : ApplicationService, IMCPToolAppService
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
    
    public async Task<List<MCPToolDto>> GetToolsForWorkspaceAsync(string workspaceName)
    {
        var tools = await _toolRegistry.GetToolsForWorkspaceAsync(workspaceName);
        
        return tools.Select(t => new MCPToolDto
        {
            Name = t.Name,
            Description = t.Description,
            ParameterSchema = t.ParameterSchema,
            ToolType = t.ToolType.ToString(),
            Source = t.Source
        }).ToList();
    }
    
    public async Task<MCPToolDto> GetToolAsync(string workspaceName, string toolName)
    {
        var tool = await _toolRegistry.GetToolAsync(workspaceName, toolName);
        
        if (tool == null)
        {
            throw new Volo.Abp.BusinessException(AIManagementErrorCodes.MCPToolNotFound)
                .WithData("ToolName", toolName)
                .WithData("WorkspaceName", workspaceName);
        }
        
        return new MCPToolDto
        {
            Name = tool.Name,
            Description = tool.Description,
            ParameterSchema = tool.ParameterSchema,
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
