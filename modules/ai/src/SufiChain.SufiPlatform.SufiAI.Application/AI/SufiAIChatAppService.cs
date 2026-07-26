using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Features;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI;

[RequiresFeature(SufiAIFeatures.Enable)]
[Authorize(AIPermissions.AI.Chat)]
public class SufiAIChatAppService : SufiApplicationService, ISufiAIChatAppService
{
    protected ISufiAIChatService ChatService { get; }
    protected IWorkspaceAccessor WorkspaceAccessor { get; }
    protected WorkspaceSyncService WorkspaceSyncService { get; }
    protected IMCPKernelToolRegistrar ToolRegistrar { get; }
    protected IWorkspaceRepository WorkspaceRepository { get; }
    protected IAIUsageLogRepository UsageLogRepository { get; }

    public SufiAIChatAppService(
        ISufiAIChatService chatService,
        IWorkspaceAccessor workspaceAccessor,
        WorkspaceSyncService workspaceSyncService,
        IMCPKernelToolRegistrar toolRegistrar,
        IWorkspaceRepository workspaceRepository,
        IAIUsageLogRepository usageLogRepository)
    {
        ChatService = chatService;
        WorkspaceAccessor = workspaceAccessor;
        WorkspaceSyncService = workspaceSyncService;
        ToolRegistrar = toolRegistrar;
        WorkspaceRepository = workspaceRepository;
        UsageLogRepository = usageLogRepository;
    }

    [RequiresFeature(SufiAIFeatures.Chat)]
    public virtual async Task<SufiAIChatResponseDto> SendMessageAsync(SufiAISendChatMessageInput input)
    {
        var request = MapRequest(input);
        var response = await ChatService.CompleteAsync(request);

        return new SufiAIChatResponseDto
        {
            Message = response.Content,
            Model = response.ModelId,
            TokensUsed = response.Usage.TotalTokens,
            InputTokens = response.Usage.InputTokens,
            OutputTokens = response.Usage.OutputTokens
        };
    }

    [RequiresFeature(SufiAIFeatures.Chat)]
    public virtual async IAsyncEnumerable<SufiAIChatResponseDto> StreamMessageAsync(SufiAISendChatMessageInput input)
    {
        await foreach (var chunk in ChatService.StreamAsync(MapRequest(input)))
        {
            yield return new SufiAIChatResponseDto
            {
                Message = chunk.Content,
                Model = chunk.ModelId,
                TokensUsed = chunk.Usage?.TotalTokens,
                InputTokens = chunk.Usage?.InputTokens,
                OutputTokens = chunk.Usage?.OutputTokens
            };
        }
    }

    [RequiresFeature(SufiAIFeatures.Chat, SufiAIFeatures.MCP)]
    [Authorize(AIPermissions.MCPTools.Execute)]
    public virtual async Task<SufiAIChatResponseDto> SendMessageWithToolsAsync(SufiAISendChatMessageInput input)
    {
        var workspace = await WorkspaceRepository.FindByNameAsync(input.WorkspaceName);
        if (workspace == null)
        {
            throw new BusinessException(AIErrorCodes.WorkspaceNotFound)
                .WithData("WorkspaceName", input.WorkspaceName);
        }

        var kernel = await WorkspaceSyncService.CreateRequestKernelAsync(input.WorkspaceName);
        await ToolRegistrar.RegisterToolsAsync(
            kernel,
            CreateWorkspaceContext(input.WorkspaceName),
            input.AllowedMcpToolNames);

        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(BuildToolUseSystemMessage());

        foreach (var message in input.ConversationHistory)
        {
            chatHistory.AddMessage(new AuthorRole(message.Role), message.Content);
        }

        chatHistory.AddUserMessage(input.Message);

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            Temperature = input.Temperature,
            MaxTokens = input.MaxTokens,
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
        };

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await chatService.GetChatMessageContentAsync(
                chatHistory,
                executionSettings,
                kernel);

            stopwatch.Stop();
            var usage = SemanticKernelChatTokenUsageExtractor.Extract(response);
            await LogMcpChatUsageAsync(workspace, usage, stopwatch.ElapsedMilliseconds, isSuccess: true);

            return new SufiAIChatResponseDto
            {
                Message = response.Content ?? string.Empty,
                Model = response.ModelId ?? string.Empty,
                TokensUsed = usage.TotalTokens ?? (usage.HasUsage
                    ? (usage.InputTokens ?? 0) + (usage.OutputTokens ?? 0)
                    : null),
                InputTokens = usage.InputTokens,
                OutputTokens = usage.OutputTokens
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await LogMcpChatUsageAsync(
                workspace,
                new SufiAITokenUsage(),
                stopwatch.ElapsedMilliseconds,
                isSuccess: false,
                errorMessage: ex.Message);

            throw;
        }
    }

    protected virtual SufiAIChatRequest MapRequest(SufiAISendChatMessageInput input)
    {
        var request = new SufiAIChatRequest
        {
            WorkspaceName = input.WorkspaceName,
            Temperature = input.Temperature,
            MaxTokens = input.MaxTokens,
            Messages = input.ConversationHistory.Select(message => new SufiAIChatMessage
            {
                Role = message.Role,
                Content = message.Content
            }).ToList()
        };

        request.Messages.Add(new SufiAIChatMessage
        {
            Role = SufiAIChatRoles.User,
            Content = input.Message
        });

        return request;
    }

    protected virtual WorkspaceContext CreateWorkspaceContext(string workspaceName)
    {
        return new WorkspaceContext
        {
            WorkspaceName = workspaceName,
            TenantId = CurrentTenant.Id,
            UserId = CurrentUser.Id
        };
    }

    protected virtual string BuildToolUseSystemMessage()
    {
        return """
            You are connected to the workspace's enabled MCP tools.
            When the user asks for information that may be available through a tool, use the enabled tools automatically.
            If a tool requires an identifier, do not ask the user for it first when an enabled list, lookup, search, get, or browse tool can discover it.
            Use the appropriate discovery tool, inspect its results, then call the target tool with the best matching identifier.
            Never invent required arguments or business data for create, update, delete, or other state-changing operations.
            Ask a concise follow-up question when required information is missing or ambiguous and cannot be discovered with the enabled tools.
            Confirm the intended target and effect before performing a destructive or irreversible operation.
            Treat tool results as the source of truth and report success only when the tool confirms it.
            Do not expose raw JSON unless the user asks for it; summarize tool results naturally in the user's language.
            """;
    }

    protected virtual async Task LogMcpChatUsageAsync(
        Workspace workspace,
        SufiAITokenUsage usage,
        long latencyMs,
        bool isSuccess,
        string? errorMessage = null)
    {
        var log = new AIUsageLog(
            GuidGenerator.Create(),
            workspace.Id,
            AICapabilityType.ChatCompletion,
            workspace.Model,
            workspace.Provider,
            workspace.TenantId);

        if (isSuccess)
        {
            var cost = CalculateCost(workspace, usage.InputTokens, usage.OutputTokens);
            log.RecordSuccess(
                usage.InputTokens,
                usage.OutputTokens,
                latencyMs,
                cost.EstimatedCost,
                usage.TotalTokens,
                cost.IsCostCalculated,
                usage.HasUsage ? null : "ProviderDidNotReturnUsage",
                cost.CostCalculationNote);
        }
        else
        {
            log.RecordFailure(errorMessage ?? "Unknown error", latencyMs);
        }

        await UsageLogRepository.InsertAsync(log);
    }

    protected virtual CostCalculationResult CalculateCost(Workspace workspace, int? inputTokens, int? outputTokens)
    {
        var hasTokenUsage = inputTokens.HasValue || outputTokens.HasValue;
        if (!hasTokenUsage)
        {
            return new CostCalculationResult(0, false, "UsageUnavailable");
        }

        var hasPricing = workspace.InputCostPer1MTokens.HasValue || workspace.OutputCostPer1MTokens.HasValue;
        if (!hasPricing)
        {
            return new CostCalculationResult(0, false, "PricingNotConfigured");
        }

        var estimatedCost = ((inputTokens ?? 0) * (workspace.InputCostPer1MTokens ?? 0) +
                             (outputTokens ?? 0) * (workspace.OutputCostPer1MTokens ?? 0)) / 1000000m;

        return new CostCalculationResult(estimatedCost, true, null);
    }

    protected sealed record CostCalculationResult(
        decimal EstimatedCost,
        bool IsCostCalculated,
        string? CostCalculationNote);
}
