using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SufiChain.SufiAbp.AI.Features;
using SufiChain.SufiAbp.AIManagement.MCP.Abstractions;
using SufiChain.SufiAbp.AIManagement.Workspaces;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using SufiChain.SufiAbp.Features;
using Volo.Abp.Users;

namespace SufiChain.SufiAbp.AIManagement.MCP.Execution;

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
    private readonly ILogger<MCPToolExecutionManager> _logger;
    
    public MCPToolExecutionManager(
        IMCPToolRegistry toolRegistry,
        IWorkspaceRepository workspaceRepository,
        WorkspaceSyncService syncService,
        ICurrentUser currentUser,
        IFeatureChecker featureChecker,
        ILogger<MCPToolExecutionManager> logger)
    {
        _toolRegistry = toolRegistry;
        _workspaceRepository = workspaceRepository;
        _syncService = syncService;
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
        var tool = await _toolRegistry.GetToolAsync(workspaceName, toolName, cancellationToken);
        
        if (tool == null)
        {
            throw new BusinessException(AIManagementErrorCodes.MCPToolNotFound)
                .WithData("ToolName", toolName)
                .WithData("WorkspaceName", workspaceName);
        }
        
        var workspace = await _workspaceRepository.FindByNameAsync(workspaceName, cancellationToken);
        
        if (workspace == null)
        {
            throw new BusinessException(AIManagementErrorCodes.WorkspaceNotFound)
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
            
            // Register MCP tool as Semantic Kernel plugin if not already registered
            // This allows SK to invoke the tool via function calling
            RegisterToolAsPlugin(kernel, tool, context);
            
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
            
            throw new BusinessException(AIManagementErrorCodes.MCPToolExecutionFailed)
                .WithData("ToolName", tool.Name)
                .WithData("Error", ex.Message);
        }
    }
    
    private void RegisterToolAsPlugin(Kernel kernel, IMCPTool tool, WorkspaceContext context)
    {
        // Check if plugin already registered
        var pluginName = $"MCP_{tool.Name}";
        if (kernel.Plugins.Any(p => p.Name == pluginName))
        {
            return;
        }
        
        // Create a KernelFunction from the MCP tool
        var function = KernelFunctionFactory.CreateFromMethod(
            async (Dictionary<string, object?> args, CancellationToken ct) =>
            {
                var result = await tool.ExecuteAsync(context, args, ct);
                return result.Result;
            },
            functionName: tool.Name,
            description: tool.Description ?? $"MCP tool: {tool.Name}"
        );
        
        // Add as plugin
        kernel.Plugins.AddFromFunctions(pluginName, new[] { function });
        
        _logger.LogDebug("Registered MCP tool {ToolName} as Semantic Kernel plugin", tool.Name);
    }

    private async Task CheckFeatureAsync()
    {
        if (!await _featureChecker.IsEnabledAsync(SufiAbpAIFeatures.Enable))
        {
            throw new BusinessException($"Feature is disabled: {SufiAbpAIFeatures.Enable}");
        }

        if (!await _featureChecker.IsEnabledAsync(SufiAbpAIFeatures.MCP))
        {
            throw new BusinessException($"Feature is disabled: {SufiAbpAIFeatures.MCP}");
        }
    }
}
