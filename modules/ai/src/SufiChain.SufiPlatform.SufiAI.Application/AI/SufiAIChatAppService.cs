using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
    protected IAIToolChatExecutor ToolChatExecutor { get; }
    protected IMCPKernelToolRegistrar ToolRegistrar { get; }
    protected IWorkspaceRepository WorkspaceRepository { get; }
    protected IWorkspaceRuntimeConfigurationResolver RuntimeConfigurationResolver { get; }
    protected IWorkspaceGuardrailService WorkspaceGuardrailService { get; }

    public SufiAIChatAppService(
        ISufiAIChatService chatService,
        IAIToolChatExecutor toolChatExecutor,
        IMCPKernelToolRegistrar toolRegistrar,
        IWorkspaceRepository workspaceRepository,
        IWorkspaceRuntimeConfigurationResolver runtimeConfigurationResolver,
        IWorkspaceGuardrailService workspaceGuardrailService)
    {
        ChatService = chatService;
        ToolChatExecutor = toolChatExecutor;
        ToolRegistrar = toolRegistrar;
        WorkspaceRepository = workspaceRepository;
        RuntimeConfigurationResolver = runtimeConfigurationResolver;
        WorkspaceGuardrailService = workspaceGuardrailService;
    }

    [RequiresFeature(SufiAIFeatures.Chat)]
    public virtual async Task<SufiAIChatResponseDto> SendMessageAsync(SufiAISendChatMessageInput input)
    {
        await EnsureGuardrailAsync(input.WorkspaceName);
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
        await EnsureGuardrailAsync(input.WorkspaceName);
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
        var configuration = await RuntimeConfigurationResolver.ResolveAsync(
            input.WorkspaceName,
            AICapabilityType.ChatCompletion,
            new AIModelRouteSelection
            {
                ModelConfigurationId = input.ModelConfigurationId,
                RequiresToolCalling = true
            });

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

        var executed = await ToolChatExecutor.ExecuteAsync(new AIToolChatExecutionRequest
        {
            Configuration = configuration,
            History = chatHistory,
            ExecutionSettings = executionSettings,
            RequiresToolCalling = true,
            ConfigureKernelAsync = (kernel, cancellationToken) => ToolRegistrar.RegisterToolsAsync(
                kernel,
                CreateWorkspaceContext(input.WorkspaceName),
                input.AllowedMcpToolNames,
                cancellationToken)
        });

        var usage = executed.Usage;
        return new SufiAIChatResponseDto
        {
            Message = executed.Response.Content ?? string.Empty,
            Model = executed.Response.ModelId ?? configuration.ModelId,
            TokensUsed = usage.TotalTokens ?? (usage.HasUsage
                ? (usage.InputTokens ?? 0) + (usage.OutputTokens ?? 0)
                : null),
            InputTokens = usage.InputTokens,
            OutputTokens = usage.OutputTokens
        };
    }

    protected virtual SufiAIChatRequest MapRequest(SufiAISendChatMessageInput input)
    {
        var request = new SufiAIChatRequest
        {
            WorkspaceName = input.WorkspaceName,
            ModelConfigurationId = input.ModelConfigurationId,
            Temperature = input.Temperature,
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

    protected virtual async Task EnsureGuardrailAsync(string workspaceName)
    {
        var workspace = await WorkspaceRepository.FindByNameAsync(workspaceName);
        if (workspace == null)
        {
            throw new BusinessException(AIErrorCodes.WorkspaceNotFound)
                .WithData("WorkspaceName", workspaceName);
        }

        await WorkspaceGuardrailService.EnsureCanExecuteAsync(workspace.Id);
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
}
