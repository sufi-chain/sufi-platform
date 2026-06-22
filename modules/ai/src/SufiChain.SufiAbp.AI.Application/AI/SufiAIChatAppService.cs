using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AI.Features;
using SufiChain.SufiAbp.AI.MCP.Abstractions;
using SufiChain.SufiAbp.AI.Permissions;
using SufiChain.SufiAbp.AI.Workspaces;
using SufiChain.SufiAbp.Application.Services;
using SufiChain.SufiAbp.Features;
using Volo.Abp;

namespace SufiChain.SufiAbp.AI;

[RequiresFeature(SufiAIFeatures.Enable)]
[Authorize(AIPermissions.AI.Chat)]
public class SufiAIChatAppService : SufiAbpApplicationService, ISufiAIChatAppService
{
    protected ISufiAIChatService ChatService { get; }
    protected IWorkspaceAccessor WorkspaceAccessor { get; }
    protected IMCPKernelToolRegistrar ToolRegistrar { get; }
    protected IWorkspaceRepository WorkspaceRepository { get; }
    protected IAIUsageLogRepository UsageLogRepository { get; }

    public SufiAIChatAppService(
        ISufiAIChatService chatService,
        IWorkspaceAccessor workspaceAccessor,
        IMCPKernelToolRegistrar toolRegistrar,
        IWorkspaceRepository workspaceRepository,
        IAIUsageLogRepository usageLogRepository)
    {
        ChatService = chatService;
        WorkspaceAccessor = workspaceAccessor;
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

        var kernel = await WorkspaceAccessor.GetKernelAsync(input.WorkspaceName);
        await ToolRegistrar.RegisterToolsAsync(kernel, input.WorkspaceName, CreateWorkspaceContext(input.WorkspaceName));

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
            var usage = ExtractTokenUsage(response);
            await LogMcpChatUsageAsync(workspace, usage, stopwatch.ElapsedMilliseconds, isSuccess: true);

            return new SufiAIChatResponseDto
            {
                Message = response.Content ?? string.Empty,
                Model = response.ModelId ?? string.Empty,
                TokensUsed = usage.EffectiveTotalTokens,
                InputTokens = usage.InputTokens,
                OutputTokens = usage.OutputTokens
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await LogMcpChatUsageAsync(
                workspace,
                TokenUsage.Empty,
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
        return $"""
            You are connected to the workspace's enabled MCP tools.
            Current UTC time is {DateTime.UtcNow:O}. Use this only to interpret explicit relative dates like today, tomorrow, or next week.
            When the user asks for information that may be available through a tool, use the enabled tools automatically.
            If a target tool requires an identifier such as an id, calendarId, eventId, documentId, userId, or similar, do not ask the user for that identifier first when there is an enabled list, lookup, search, get, or browse tool that can discover it.
            First call the appropriate discovery tool, inspect names, titles, kinds, defaults, ownership, and descriptions, then call the target tool with the best matching identifier.
            For create/update/delete tools, never invent required business data. If date, time, duration/end time, timezone, title, target calendar, participant, or any required field is missing or ambiguous, ask a concise follow-up question before calling the tool.
            For scheduling, a time like "2pm" is not enough by itself unless the date, timezone, and duration or end time are already clear from the conversation.
            Ask a follow-up question only when enabled tools cannot discover a suitable target or required create/update/delete data is missing or ambiguous.
            Do not expose raw JSON unless the user asks for it; summarize tool results naturally in the user's language.
            """;
    }

    protected virtual async Task LogMcpChatUsageAsync(
        Workspace workspace,
        TokenUsage usage,
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
                cost.CostCalculationNote,
                requestMetadataJson: "{\"mcp\":true}");
        }
        else
        {
            log.RecordFailure(errorMessage ?? "Unknown error", latencyMs, "{\"mcp\":true}");
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

    protected virtual TokenUsage ExtractTokenUsage(ChatMessageContent response)
    {
        var metadataUsage = ExtractTokenUsage(response.Metadata);
        if (metadataUsage.HasUsage)
        {
            return metadataUsage;
        }

        return response.InnerContent == null
            ? TokenUsage.Empty
            : TokenUsage.Create(
                ReadIntProperty(response.InnerContent, "InputTokens", "PromptTokens", "InputTokenCount"),
                ReadIntProperty(response.InnerContent, "OutputTokens", "CompletionTokens", "OutputTokenCount"),
                ReadIntProperty(response.InnerContent, "TotalTokens", "TotalTokenCount"));
    }

    protected virtual TokenUsage ExtractTokenUsage(IReadOnlyDictionary<string, object?>? metadata)
    {
        if (metadata == null)
        {
            return TokenUsage.Empty;
        }

        foreach (var key in new[] { "Usage", "usage", "TokenUsage", "token_usage" })
        {
            if (metadata.TryGetValue(key, out var usage) && usage != null)
            {
                return TokenUsage.Create(
                    ReadIntProperty(usage, "InputTokens", "PromptTokens", "InputTokenCount"),
                    ReadIntProperty(usage, "OutputTokens", "CompletionTokens", "OutputTokenCount"),
                    ReadIntProperty(usage, "TotalTokens", "TotalTokenCount"));
            }
        }

        return TokenUsage.Create(
            ReadIntMetadata(metadata, "InputTokens", "PromptTokens", "input_tokens", "prompt_tokens"),
            ReadIntMetadata(metadata, "OutputTokens", "CompletionTokens", "output_tokens", "completion_tokens"),
            ReadIntMetadata(metadata, "TotalTokens", "total_tokens"));
    }

    private static int? ReadIntMetadata(IReadOnlyDictionary<string, object?> metadata, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (metadata.TryGetValue(key, out var value) && TryConvertInt(value, out var result))
            {
                return result;
            }
        }

        return null;
    }

    private static int? ReadIntProperty(object source, params string[] propertyNames)
    {
        var sourceType = source.GetType();
        foreach (var propertyName in propertyNames)
        {
            var property = sourceType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (property != null && TryConvertInt(property.GetValue(source), out var result))
            {
                return result;
            }
        }

        return null;
    }

    private static bool TryConvertInt(object? value, out int result)
    {
        switch (value)
        {
            case int intValue:
                result = intValue;
                return true;
            case long longValue when longValue <= int.MaxValue:
                result = (int)longValue;
                return true;
            case null:
                result = 0;
                return false;
            default:
                return int.TryParse(value.ToString(), out result);
        }
    }

    protected sealed record TokenUsage(int? InputTokens, int? OutputTokens, int? TotalTokens)
    {
        public static TokenUsage Empty { get; } = new(null, null, null);

        public bool HasUsage => InputTokens.HasValue || OutputTokens.HasValue || TotalTokens.HasValue;

        public int? EffectiveTotalTokens => TotalTokens ?? (InputTokens.HasValue || OutputTokens.HasValue
            ? (InputTokens ?? 0) + (OutputTokens ?? 0)
            : null);

        public static TokenUsage Create(int? inputTokens, int? outputTokens, int? totalTokens)
        {
            return new TokenUsage(
                inputTokens,
                outputTokens,
                totalTokens ?? (inputTokens.HasValue || outputTokens.HasValue
                    ? (inputTokens ?? 0) + (outputTokens ?? 0)
                    : null));
        }
    }

    protected sealed record CostCalculationResult(
        decimal EstimatedCost,
        bool IsCostCalculated,
        string? CostCalculationNote);
}
