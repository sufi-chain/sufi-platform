using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.MCP;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Cache;
using SufiChain.SufiPlatform.SufiAI.MCP.Entities;
using SufiChain.SufiPlatform.SufiAI.MCP.Tools;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Features;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI.Application.MCP.Tools;

[RequiresFeature(SufiAIFeatures.Enable, SufiAIFeatures.MCP)]
[Authorize(AIPermissions.MCPTools.Default)]
public class MCPToolAppService : SufiApplicationService, IMCPToolAppService
{
    private readonly IMCPToolRegistry _toolRegistry;
    private readonly IMCPToolExecutor _toolExecutor;
    private readonly IMCPServerRepository _serverRepository;
    private readonly IMCPCatalogCache _catalogCache;
    
    public MCPToolAppService(
        IMCPToolRegistry toolRegistry,
        IMCPToolExecutor toolExecutor,
        IMCPServerRepository serverRepository,
        IMCPCatalogCache catalogCache)
    {
        _toolRegistry = toolRegistry;
        _toolExecutor = toolExecutor;
        _serverRepository = serverRepository;
        _catalogCache = catalogCache;
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
            Source = tool.Source,
            Effect = tool is IMCPToolEffect declared
                && declared.Effect is AgentActionEffect.Read or AgentActionEffect.Write
                ? declared.Effect.ToString()
                : AgentActionEffect.Write.ToString(),
            ReadOnlyHint = tool is IMCPToolCatalogAnnotation annotation ? annotation.ReadOnlyHint : null,
            EffectIsAdminSet = tool is IMCPToolCatalogAnnotation marked && marked.EffectIsAdminSet
        };
    }

    [Authorize(AIPermissions.MCPTools.Manage)]
    public async Task SetEffectAsync(SetMcpToolEffectDto input)
    {
        if (!McpExternalToolName.TrySplit(input.ToolName, out var serverKey, out var remoteName))
        {
            throw new BusinessException(AIErrorCodes.InvalidProviderConfiguration)
                .WithData("Error", "Only an external tool can have an admin effect. Use external.<serverKey>.<toolName>.");
        }

        if (!Enum.TryParse<AgentActionEffect>(input.Effect, ignoreCase: true, out var effect)
            || effect is not (AgentActionEffect.Read or AgentActionEffect.Write))
        {
            throw new BusinessException(AIErrorCodes.InvalidProviderConfiguration)
                .WithData("Error", "Tool effect must be Read or Write.");
        }

        var server = await _serverRepository.FindByKeyAsync(serverKey);
        if (server is null)
        {
            throw new BusinessException(AIErrorCodes.InvalidProviderConfiguration)
                .WithData("Error", $"MCP server '{serverKey}' is not visible to the current tenant.");
        }

        server.SetAdminToolEffect(remoteName, effect);
        await _serverRepository.UpdateAsync(server, autoSave: true);
        await _catalogCache.InvalidateAsync();
    }
    
    public async Task<MCPToolExecutionResultDto> ExecuteToolAsync(MCPToolExecutionRequestDto request)
    {
        var result = await _toolExecutor.ExecuteAsync(
            request.WorkspaceName,
            request.ToolName,
            request.Parameters,
            sessionId: request.SessionId
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
