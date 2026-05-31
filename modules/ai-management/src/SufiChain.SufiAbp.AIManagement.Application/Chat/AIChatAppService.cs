using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SufiChain.SufiAbp.AI.Features;
using SufiChain.SufiAbp.AIManagement.Workspaces;
using SufiChain.SufiAbp.Application.Services;
using Volo.Abp;
using SufiChain.SufiAbp.Features;

namespace SufiChain.SufiAbp.AIManagement.Chat;

[RequiresFeature(SufiAbpAIFeatures.Enable)]
public class AIChatAppService : SufiAbpApplicationService, IAIChatAppService
{
    private readonly IWorkspaceRepository _workspaceRepository;

    public AIChatAppService(IWorkspaceRepository workspaceRepository)
    {
        _workspaceRepository = workspaceRepository;
    }

    public async Task<ChatResponseDto> SendMessageAsync(SendChatMessageInput input)
    {
        await CheckFeatureAsync(SufiAbpAIFeatures.Chat);

        // Find workspace by name
        var workspace = await _workspaceRepository.FindByNameAsync(input.WorkspaceName);
        if (workspace == null)
        {
            throw new Volo.Abp.BusinessException("AIManagement:WorkspaceNotFound")
                .WithData("WorkspaceName", input.WorkspaceName);
        }

        if (!workspace.IsActive)
        {
            throw new Volo.Abp.BusinessException("AIManagement:WorkspaceNotActive")
                .WithData("WorkspaceName", input.WorkspaceName);
        }

        // Get chat completion service for this workspace
        var chatService = GetChatCompletionServiceForWorkspace(workspace);

        // Build conversation history
        var chatHistory = new Microsoft.SemanticKernel.ChatCompletion.ChatHistory();

        // Add SystemPrompt if configured
        if (!string.IsNullOrEmpty(workspace.SystemPrompt))
        {
            chatHistory.AddSystemMessage(workspace.SystemPrompt);
        }

        // Add conversation history
        foreach (var msg in input.ConversationHistory)
        {
            if (msg.Role.ToLowerInvariant() == "user")
            {
                chatHistory.AddUserMessage(msg.Content);
            }
            else
            {
                chatHistory.AddAssistantMessage(msg.Content);
            }
        }

        // Add current message
        chatHistory.AddUserMessage(input.Message);

        // Configure execution settings
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            Temperature = workspace.Temperature,
            MaxTokens = workspace.MaxTokens
        };

        // Send to AI
        var response = await chatService.GetChatMessageContentAsync(
            chatHistory,
            executionSettings);

        return new ChatResponseDto
        {
            Message = response.Content ?? string.Empty,
            TokensUsed = response.Metadata?.TryGetValue("Usage", out var usage) == true 
                ? GetTotalTokens(usage) 
                : null,
            Model = workspace.Model
        };
    }

    private int? GetTotalTokens(object? usage)
    {
        if (usage == null) return null;
        
        // Try to get TotalTokens from the usage object
        var usageType = usage.GetType();
        var totalTokensProp = usageType.GetProperty("TotalTokens");
        if (totalTokensProp != null)
        {
            return (int?)totalTokensProp.GetValue(usage);
        }
        
        return null;
    }

    private async Task CheckFeatureAsync(string featureName)
    {
        if (!await FeatureChecker.IsEnabledAsync(featureName))
        {
            throw new BusinessException($"Feature is disabled: {featureName}");
        }
    }

    private IChatCompletionService GetChatCompletionServiceForWorkspace(Workspace workspace)
    {
        return workspace.Provider switch
        {
            AIProviderType.OpenAI => CreateOpenAIChatService(workspace),
            _ => throw new Volo.Abp.BusinessException("AIManagement:UnsupportedProvider")
                .WithData("Provider", workspace.Provider.ToString())
        };
    }

    private IChatCompletionService CreateOpenAIChatService(Workspace workspace)
    {
        if (string.IsNullOrEmpty(workspace.ApiKey))
        {
            throw new Volo.Abp.BusinessException("AIManagement:ApiKeyRequired")
                .WithData("WorkspaceName", workspace.Name)
                .WithData("Provider", "OpenAI");
        }

        // If custom API base URL is provided, use HttpClient with custom endpoint
        if (!string.IsNullOrEmpty(workspace.ApiBaseUrl))
        {
            var httpClient = new System.Net.Http.HttpClient
            {
                BaseAddress = new Uri(workspace.ApiBaseUrl)
            };
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {workspace.ApiKey}");

            return new OpenAIChatCompletionService(
                modelId: workspace.Model,
                apiKey: workspace.ApiKey,
                httpClient: httpClient);
        }
        else
        {
            // Use default OpenAI endpoint
            return new OpenAIChatCompletionService(
                modelId: workspace.Model,
                apiKey: workspace.ApiKey);
        }
    }

}
