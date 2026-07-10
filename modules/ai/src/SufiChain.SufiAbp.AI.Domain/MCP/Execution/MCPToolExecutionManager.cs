using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SufiChain.SufiAbp.AI.Features;
using SufiChain.SufiAbp.AI.MCP.Abstractions;
using SufiChain.SufiAbp.AI.Workspaces;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using SufiChain.SufiAbp.Features;
using Volo.Abp.Users;

namespace SufiChain.SufiAbp.AI.MCP.Execution;

/// <summary>
/// Manages MCP tool execution with context, validation, and auditing.
/// Uses Semantic Kernel for function calling and tool execution.
/// </summary>
public class MCPToolExecutionManager : IMCPToolExecutor, ITransientDependency
{
    private readonly IMCPToolRegistry _toolRegistry;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly WorkspaceSyncService _syncService;
    private readonly ICurrentUser _currentUser;
    private readonly IFeatureChecker _featureChecker;
    private readonly IMCPKernelToolRegistrar _toolRegistrar;
    private readonly ILogger<MCPToolExecutionManager> _logger;
    
    public MCPToolExecutionManager(
        IMCPToolRegistry toolRegistry,
        IWorkspaceRepository workspaceRepository,
        WorkspaceSyncService syncService,
        ICurrentUser currentUser,
        IFeatureChecker featureChecker,
        IMCPKernelToolRegistrar toolRegistrar,
        ILogger<MCPToolExecutionManager> logger)
    {
        _toolRegistry = toolRegistry;
        _workspaceRepository = workspaceRepository;
        _syncService = syncService;
        _currentUser = currentUser;
        _featureChecker = featureChecker;
        _toolRegistrar = toolRegistrar;
        _logger = logger;
    }
    
    public async Task<MCPToolExecutionResult> ExecuteAsync(
        string workspaceName,
        string toolName,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        await CheckFeatureAsync();
        var tool = await _toolRegistry.GetToolAsync(workspaceName, toolName, cancellationToken);
        
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
            // Get kernel for the workspace from sync service
            var kernel = await _syncService.GetOrCreateKernelAsync(context.WorkspaceName, cancellationToken);
            
            await _toolRegistrar.RegisterToolsAsync(kernel, context.WorkspaceName, context);
            
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
