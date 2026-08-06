using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using SufiChain.SufiPlatform.Features;
using Volo.Abp.Users;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Execution;

/// <summary>
/// Manages MCP tool execution with context, validation, and auditing.
/// Uses Semantic Kernel for function calling and tool execution.
/// </summary>
public class MCPToolExecutionManager : IMCPToolExecutor, ITransientDependency
{
    private readonly IMCPToolRegistry _toolRegistry;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IFeatureChecker _featureChecker;
    private readonly ILogger<MCPToolExecutionManager> _logger;
    
    public MCPToolExecutionManager(
        IMCPToolRegistry toolRegistry,
        IWorkspaceRepository workspaceRepository,
        ICurrentUser currentUser,
        IFeatureChecker featureChecker,
        ILogger<MCPToolExecutionManager> logger)
    {
        _toolRegistry = toolRegistry;
        _workspaceRepository = workspaceRepository;
        _currentUser = currentUser;
        _featureChecker = featureChecker;
        _logger = logger;
    }
    
    public async Task<MCPToolExecutionResult> ExecuteAsync(
        string workspaceName,
        string toolName,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync();
        var resolution = await _toolRegistry.ResolveAsync(new[] { toolName }, cancellationToken);
        var tool = resolution.Tools.SingleOrDefault();
        
        if (tool == null)
        {
            throw new BusinessException(AIErrorCodes.MCPToolNotFound)
                .WithData("ToolName", toolName)
                .WithData("WorkspaceName", workspaceName);
        }
        
        var workspace = await _workspaceRepository.FindByNameAsync(workspaceName, cancellationToken);
        
        if (workspace == null)
        {
            throw new BusinessException(AIErrorCodes.WorkspaceNotFound)
                .WithData("WorkspaceName", workspaceName);
        }
        
        var context = new WorkspaceContext
        {
            WorkspaceName = workspaceName,
            TenantId = workspace.TenantId,
            UserId = _currentUser.Id
        };
        
        return await ExecuteAsync(tool, context, parameters, cancellationToken);
    }
    
    public async Task<MCPToolExecutionResult> ExecuteAsync(
        IMCPTool tool,
        WorkspaceContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync();
        _logger.LogInformation(
            "Executing MCP tool {ToolName} (Type: {ToolType}, Source: {Source}) in workspace {WorkspaceName}",
            tool.Name,
            tool.ToolType,
            tool.Source,
            context.WorkspaceName
        );
        
        try
        {
            // Execute the tool
            var result = await tool.ExecuteAsync(context, parameters, cancellationToken);
            
            if (result.Success)
            {
                _logger.LogInformation(
                    "MCP tool {ToolName} executed successfully in {ExecutionTimeMs}ms",
                    tool.Name,
                    result.ExecutionTimeMs
                );
            }
            else
            {
                _logger.LogWarning(
                    "MCP tool {ToolName} execution failed: {ErrorMessage}",
                    tool.Name,
                    result.ErrorMessage
                );
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error executing MCP tool {ToolName}",
                tool.Name
            );
            
            throw new BusinessException(AIErrorCodes.MCPToolExecutionFailed)
                .WithData("ToolName", tool.Name)
                .WithData("Error", ex.Message);
        }
    }
    
    private async Task CheckFeatureAsync()
    {
        if (!await _featureChecker.IsEnabledAsync(SufiAIFeatures.Enable))
        {
            throw new BusinessException($"Feature is disabled: {SufiAIFeatures.Enable}");
        }

        if (!await _featureChecker.IsEnabledAsync(SufiAIFeatures.MCP))
        {
            throw new BusinessException($"Feature is disabled: {SufiAIFeatures.MCP}");
        }
    }
}
