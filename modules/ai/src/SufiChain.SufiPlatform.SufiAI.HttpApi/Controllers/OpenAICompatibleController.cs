using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;
using SufiChain.SufiPlatform.SufiAI.OpenAI;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI.Controllers;

[Area("ai")]
[Route("v1")]
[RemoteService(IsEnabled = true)]
public class OpenAICompatibleController : SufiControllerBase
{
    private readonly IAIService _aiService;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IWorkspaceRuntimeConfigurationResolver _runtimeConfigurationResolver;
    private readonly IWorkspaceGuardrailService _workspaceGuardrailService;

    public OpenAICompatibleController(
        IAIService aiService,
        IWorkspaceRepository workspaceRepository,
        IWorkspaceRuntimeConfigurationResolver runtimeConfigurationResolver,
        IWorkspaceGuardrailService workspaceGuardrailService)
    {
        _aiService = aiService;
        _workspaceRepository = workspaceRepository;
        _runtimeConfigurationResolver = runtimeConfigurationResolver;
        _workspaceGuardrailService = workspaceGuardrailService;
    }

    [HttpPost("search")]
    [Authorize(AIPermissions.AI.WebSearch)]
    public async Task<WebSearchDto> SearchAsync(
        [FromBody] WebSearchInput request,
        CancellationToken cancellationToken = default)
    {
        var response = await _aiService.SearchWebAsync(new WebSearchRequest
        {
            WorkspaceName = request.WorkspaceName,
            Query = request.Query,
            Culture = request.Culture,
            SafeSearch = request.SafeSearch,
            MaxResults = request.MaxResults,
            TimeRange = request.TimeRange
        }, cancellationToken);

        return new WebSearchDto
        {
            Model = response.ModelId,
            Results = response.Results.Select(item => new WebSearchResultDto
            {
                Title = item.Title,
                Url = item.Url,
                Snippet = item.Snippet,
                PublishedAt = item.PublishedAt,
                Source = item.Source,
                Rank = item.Rank
            }).ToList()
        };
    }

    [HttpPost("web/fetch")]
    [Authorize(AIPermissions.AI.WebFetch)]
    public async Task<WebFetchDto> FetchAsync(
        [FromBody] WebFetchInput request,
        CancellationToken cancellationToken = default)
    {
        var response = await _aiService.FetchWebAsync(new WebFetchRequest
        {
            WorkspaceName = request.WorkspaceName,
            Url = request.Url,
            TimeoutSeconds = request.TimeoutSeconds,
            MaxBytes = request.MaxBytes
        }, cancellationToken);

        return new WebFetchDto
        {
            Url = response.Url,
            CanonicalUrl = response.CanonicalUrl,
            Title = response.Title,
            Content = response.Content,
            ContentType = response.ContentType,
            Truncated = response.Truncated,
            RetrievedAt = response.RetrievedAt,
            StatusCode = response.StatusCode
        };
    }

    [HttpPost("chat/completions")]
    [Authorize(AIPermissions.AI.Chat)]
    public async Task<IActionResult> CreateChatCompletionAsync(
        [FromBody] OpenAIChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        var workspace = await LoadWorkspaceAsync(request.WorkspaceName, cancellationToken);
        if (workspace == null)
        {
            return OpenAIError(404, $"Unknown workspace '{request.WorkspaceName}'.", "workspace_name", "workspace_not_found");
        }

        await _workspaceGuardrailService.EnsureCanExecuteAsync(workspace.Id, cancellationToken);

        var resolved = ResolveReadyRoute(workspace, AICapabilityType.ChatCompletion, request.Model);
        if (resolved.Error != null)
        {
            return resolved.Error;
        }

        var chatRequest = new ChatCompletionRequest
        {
            WorkspaceName = workspace.Name,
            ModelConfigurationId = resolved.ConfigurationId,
            Messages = request.Messages.Select(message => new ChatMessage
            {
                Role = message.Role,
                Content = message.Content
            }).ToList(),
            Temperature = request.Temperature.HasValue ? (float)request.Temperature.Value : null,
            MaxTokens = request.MaxTokens,
            Stream = request.Stream
        };

        if (request.Stream)
        {
            return new OpenAIStreamingChatCompletionResult(
                _aiService.StreamChatMessageAsync(chatRequest, cancellationToken),
                request.Model,
                cancellationToken);
        }

        var executed = await _aiService.SendChatMessageAsync(chatRequest, cancellationToken);
        return Ok(new OpenAIChatCompletionResponse
        {
            Id = $"chatcmpl-{Guid.NewGuid():N}",
            Object = "chat.completion",
            Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Model = request.Model,
            Choices =
            [
                new OpenAIChatCompletionChoice
                {
                    Index = 0,
                    Message = new OpenAIChatMessage
                    {
                        Role = "assistant",
                        Content = executed.Content ?? string.Empty
                    },
                    FinishReason = executed.FinishReason ?? "stop"
                }
            ],
            Usage = new OpenAIUsageInfo
            {
                PromptTokens = executed.InputTokens,
                CompletionTokens = executed.OutputTokens,
                TotalTokens = executed.TotalTokens
            }
        });
    }

    [HttpPost("embeddings")]
    [Authorize(AIPermissions.AI.Embeddings)]
    public async Task<IActionResult> CreateEmbeddingsAsync(
        [FromBody] OpenAIEmbeddingRequest request,
        CancellationToken cancellationToken = default)
    {
        var workspace = await LoadWorkspaceAsync(request.WorkspaceName, cancellationToken);
        if (workspace == null)
        {
            return OpenAIError(404, $"Unknown workspace '{request.WorkspaceName}'.", "workspace_name", "workspace_not_found");
        }

        await _workspaceGuardrailService.EnsureCanExecuteAsync(workspace.Id, cancellationToken);

        var resolved = ResolveReadyRoute(workspace, AICapabilityType.Embeddings, request.Model);
        if (resolved.Error != null)
        {
            return resolved.Error;
        }

        var inputs = ParseEmbeddingInputs(request.Input);
        var embeddings = new List<OpenAIEmbeddingData>();
        int? promptTokens = null;
        for (var i = 0; i < inputs.Count; i++)
        {
            var response = await _aiService.GenerateEmbeddingsAsync(new EmbeddingsRequest
            {
                WorkspaceName = workspace.Name,
                ModelConfigurationId = resolved.ConfigurationId,
                Text = inputs[i]
            }, cancellationToken);

            embeddings.Add(new OpenAIEmbeddingData
            {
                Index = i,
                Embedding = response.Embedding,
                Object = "embedding"
            });

            if (response.TotalTokens.HasValue)
            {
                promptTokens = (promptTokens ?? 0) + response.TotalTokens.Value;
            }
        }

        return Ok(new OpenAIEmbeddingResponse
        {
            Object = "list",
            Data = embeddings,
            Model = request.Model,
            Usage = new OpenAIUsageInfo
            {
                PromptTokens = promptTokens,
                TotalTokens = promptTokens
            }
        });
    }

    [HttpGet("models")]
    [Authorize(AIPermissions.AI.Chat)]
    public async Task<IActionResult> ListModels(
        [FromQuery] string workspaceName,
        CancellationToken cancellationToken = default)
    {
        var workspace = await LoadWorkspaceAsync(workspaceName, cancellationToken);
        if (workspace == null)
        {
            return OpenAIError(404, $"Unknown workspace '{workspaceName}'.", "workspace_name", "workspace_not_found");
        }

        var data = workspace.ModelConfigurations
            .Where(configuration => configuration.CapabilityType == AICapabilityType.ChatCompletion && configuration.IsEnabled)
            .Select(configuration => _runtimeConfigurationResolver.Resolve(
                workspace,
                AICapabilityType.ChatCompletion,
                configuration))
            .Where(snapshot => snapshot.IsReady)
            .Select(snapshot => new OpenAIModelInfo
            {
                Id = snapshot.ModelId,
                Object = "model",
                OwnedBy = workspace.Name
            })
            .ToList();

        return Ok(new OpenAIModelsResponse
        {
            Object = "list",
            Data = data
        });
    }

    private async Task<Workspace?> LoadWorkspaceAsync(string? workspaceName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(workspaceName))
        {
            return null;
        }

        return await _workspaceRepository.FindByNameAsync(workspaceName.Trim(), cancellationToken);
    }

    private (Guid? ConfigurationId, IActionResult? Error) ResolveReadyRoute(
        Workspace workspace,
        AICapabilityType capabilityType,
        string modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            return (null, OpenAIError(400, "Model is required.", "model", "invalid_request_error"));
        }

        var matches = workspace.ModelConfigurations
            .Where(configuration =>
                configuration.IsEnabled &&
                configuration.CapabilityType == capabilityType &&
                string.Equals(configuration.ModelId, modelId, StringComparison.OrdinalIgnoreCase))
            .Select(configuration => _runtimeConfigurationResolver.Resolve(workspace, capabilityType, configuration))
            .Where(snapshot => snapshot.IsReady)
            .ToList();

        if (matches.Count == 0)
        {
            return (null, OpenAIError(
                404,
                $"The model '{modelId}' does not match a ready {capabilityType} route in workspace '{workspace.Name}'.",
                "model",
                "model_not_found"));
        }

        if (matches.Count > 1)
        {
            return (null, OpenAIError(
                400,
                $"The model '{modelId}' matches more than one ready route in workspace '{workspace.Name}'.",
                "model",
                "model_ambiguous"));
        }

        return (matches[0].ModelConfigurationId, null);
    }

    private static IActionResult OpenAIError(int statusCode, string message, string? param, string? code)
    {
        return new ObjectResult(new OpenAIErrorResponse
        {
            Error = new OpenAIError
            {
                Message = message,
                Type = "invalid_request_error",
                Param = param,
                Code = code
            }
        })
        {
            StatusCode = statusCode
        };
    }

    private static List<string> ParseEmbeddingInputs(object input)
    {
        if (input is string text)
        {
            return [text];
        }

        if (input is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                return [element.GetString() ?? string.Empty];
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                return element.EnumerateArray()
                    .Select(item => item.GetString() ?? string.Empty)
                    .ToList();
            }
        }

        return [input?.ToString() ?? string.Empty];
    }
}

public class OpenAIStreamingChatCompletionResult : IActionResult
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IAsyncEnumerable<ChatCompletionResponse> _stream;
    private readonly string _model;
    private readonly CancellationToken _cancellationToken;

    public OpenAIStreamingChatCompletionResult(
        IAsyncEnumerable<ChatCompletionResponse> stream,
        string model,
        CancellationToken cancellationToken)
    {
        _stream = stream;
        _model = model;
        _cancellationToken = cancellationToken;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        response.ContentType = "text/event-stream";
        response.Headers.Append("Cache-Control", "no-cache");
        response.Headers.Append("Connection", "keep-alive");

        var chatId = $"chatcmpl-{Guid.NewGuid():N}";
        var created = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await foreach (var chunk in _stream.WithCancellation(_cancellationToken))
        {
            var streamChunk = new
            {
                id = chatId,
                @object = "chat.completion.chunk",
                created,
                model = _model,
                choices = new[]
                {
                    new
                    {
                        index = 0,
                        delta = new { content = chunk.Content },
                        finish_reason = (string?)null
                    }
                }
            };

            var json = JsonSerializer.Serialize(streamChunk, JsonOptions);
            var data = Encoding.UTF8.GetBytes($"data: {json}\n\n");
            await response.Body.WriteAsync(data, _cancellationToken);
            await response.Body.FlushAsync(_cancellationToken);
        }

        var finalChunk = new
        {
            id = chatId,
            @object = "chat.completion.chunk",
            created,
            model = _model,
            choices = new[]
            {
                new
                {
                    index = 0,
                    delta = new { },
                    finish_reason = "stop"
                }
            }
        };

        var finalJson = JsonSerializer.Serialize(finalChunk, JsonOptions);
        var finalData = Encoding.UTF8.GetBytes($"data: {finalJson}\n\ndata: [DONE]\n\n");
        await response.Body.WriteAsync(finalData, _cancellationToken);
        await response.Body.FlushAsync(_cancellationToken);
    }
}
