using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using SufiChain.SufiAbp.AIManagement.AI;
using SufiChain.SufiAbp.AIManagement.Permissions;
using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;
using Volo.Abp;

namespace SufiChain.SufiAbp.AIManagement.Controllers;

/// <summary>
/// OpenAI-compatible API endpoints for AI Management.
/// Provides /v1/chat/completions, /v1/embeddings, /v1/models endpoints.
/// </summary>
[Area("ai-management")]
[Route("v1")]
[RemoteService(IsEnabled = true)]
public class OpenAICompatibleController : SufiAbpControllerBase
{
    private readonly IAIKernelAppService _kernelAppService;
    private readonly ILogger<OpenAICompatibleController> _logger;

    public OpenAICompatibleController(
        IAIKernelAppService kernelAppService,
        ILogger<OpenAICompatibleController> logger)
    {
        _kernelAppService = kernelAppService;
        _logger = logger;
    }

    /// <summary>
    /// OpenAI-compatible chat completions endpoint with streaming support.
    /// POST /v1/chat/completions
    /// </summary>
    [HttpPost("chat/completions")]
    [Authorize(AIManagementPermissions.Workspaces.Default)]
    public async Task<IActionResult> CreateChatCompletionAsync(
        [FromBody] ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var kernel = (Kernel)await _kernelAppService.GetKernelAsync(request.WorkspaceName, cancellationToken);
            var chatService = kernel.GetRequiredService<IChatCompletionService>();

            var chatHistory = new ChatHistory();
            foreach (var message in request.Messages)
            {
                chatHistory.AddMessage(
                    new AuthorRole(message.Role),
                    message.Content
                );
            }

            var executionSettings = new PromptExecutionSettings
            {
                ExtensionData = new Dictionary<string, object>
                {
                    ["temperature"] = request.Temperature ?? 0.7,
                    ["max_tokens"] = request.MaxTokens ?? 1000,
                    ["top_p"] = request.TopP ?? 1.0
                }
            };

            if (request.Stream)
            {
                return new StreamingChatCompletionResult(
                    chatService,
                    chatHistory,
                    executionSettings,
                    request.Model,
                    cancellationToken
                );
            }

            var response = await chatService.GetChatMessageContentAsync(
                chatHistory,
                executionSettings,
                kernel,
                cancellationToken
            );

            return Ok(new ChatCompletionResponse
            {
                Id = $"chatcmpl-{Guid.NewGuid():N}",
                Object = "chat.completion",
                Created = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Model = request.Model,
                Choices = new List<ChatCompletionChoice>
                {
                    new()
                    {
                        Index = 0,
                        Message = new ChatMessage
                        {
                            Role = "assistant",
                            Content = response.Content ?? string.Empty
                        },
                        FinishReason = "stop"
                    }
                },
                Usage = new UsageInfo
                {
                    PromptTokens = null,
                    CompletionTokens = null,
                    TotalTokens = null
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in chat completion for workspace {WorkspaceName}", request.WorkspaceName);
            throw new UserFriendlyException($"Chat completion failed: {ex.Message}");
        }
    }

    /// <summary>
    /// OpenAI-compatible embeddings endpoint.
    /// POST /v1/embeddings
    /// </summary>
    [HttpPost("embeddings")]
    [Authorize(AIManagementPermissions.Workspaces.Default)]
    public async Task<IActionResult> CreateEmbeddingsAsync(
        [FromBody] EmbeddingRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var kernel = (Kernel)await _kernelAppService.GetKernelAsync(request.WorkspaceName, cancellationToken);
            var embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

            var inputs = request.Input is string singleInput
                ? new[] { singleInput }
                : ((JsonElement)request.Input).EnumerateArray().Select(e => e.GetString()!).ToArray();

            var embeddings = new List<EmbeddingData>();
            for (int i = 0; i < inputs.Length; i++)
            {
                var embedding = await embeddingService.GenerateEmbeddingAsync(inputs[i], cancellationToken: cancellationToken);
                embeddings.Add(new EmbeddingData
                {
                    Index = i,
                    Embedding = embedding.ToArray(),
                    Object = "embedding"
                });
            }

            return Ok(new EmbeddingResponse
            {
                Object = "list",
                Data = embeddings,
                Model = request.Model,
                Usage = new UsageInfo
                {
                    PromptTokens = null,
                    TotalTokens = null
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embeddings for workspace {WorkspaceName}", request.WorkspaceName);
            throw new UserFriendlyException($"Embedding generation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// OpenAI-compatible models list endpoint.
    /// GET /v1/models
    /// </summary>
    [HttpGet("models")]
    [Authorize(AIManagementPermissions.Workspaces.Default)]
    public IActionResult ListModels([FromQuery] string workspaceName)
    {
        // Return a static list - in production, this could query available models from providers
        return Ok(new ModelsResponse
        {
            Object = "list",
            Data = new List<ModelInfo>
            {
                new() { Id = "gpt-4", Object = "model", OwnedBy = "openai" },
                new() { Id = "gpt-5", Object = "model", OwnedBy = "openai" },
                new() { Id = "llama3.2", Object = "model", OwnedBy = "ollama" },
                new() { Id = "mistral", Object = "model", OwnedBy = "ollama" }
            }
        });
    }
}

#region DTOs

public class ChatCompletionRequest
{
    public required string WorkspaceName { get; set; }
    public required string Model { get; set; }
    public required List<ChatMessage> Messages { get; set; }
    public double? Temperature { get; set; }
    public int? MaxTokens { get; set; }
    public double? TopP { get; set; }
    public bool Stream { get; set; }
}

public class ChatMessage
{
    public required string Role { get; set; }
    public required string Content { get; set; }
}

public class ChatCompletionResponse
{
    public required string Id { get; set; }
    public required string Object { get; set; }
    public required long Created { get; set; }
    public required string Model { get; set; }
    public required List<ChatCompletionChoice> Choices { get; set; }
    public required UsageInfo Usage { get; set; }
}

public class ChatCompletionChoice
{
    public required int Index { get; set; }
    public required ChatMessage Message { get; set; }
    public required string FinishReason { get; set; }
}

public class EmbeddingRequest
{
    public required string WorkspaceName { get; set; }
    public required string Model { get; set; }
    public required object Input { get; set; } // string or string[]
}

public class EmbeddingResponse
{
    public required string Object { get; set; }
    public required List<EmbeddingData> Data { get; set; }
    public required string Model { get; set; }
    public required UsageInfo Usage { get; set; }
}

public class EmbeddingData
{
    public required int Index { get; set; }
    public required string Object { get; set; }
    public required float[] Embedding { get; set; }
}

public class UsageInfo
{
    public int? PromptTokens { get; set; }
    public int? CompletionTokens { get; set; }
    public int? TotalTokens { get; set; }
}

public class ModelsResponse
{
    public required string Object { get; set; }
    public required List<ModelInfo> Data { get; set; }
}

public class ModelInfo
{
    public required string Id { get; set; }
    public required string Object { get; set; }
    public required string OwnedBy { get; set; }
}

#endregion

#region Streaming Result

/// <summary>
/// Custom IActionResult for Server-Sent Events (SSE) streaming.
/// </summary>
public class StreamingChatCompletionResult : IActionResult
{
    private readonly IChatCompletionService _chatService;
    private readonly ChatHistory _chatHistory;
    private readonly PromptExecutionSettings _settings;
    private readonly string _model;
    private readonly CancellationToken _cancellationToken;

    public StreamingChatCompletionResult(
        IChatCompletionService chatService,
        ChatHistory chatHistory,
        PromptExecutionSettings settings,
        string model,
        CancellationToken cancellationToken)
    {
        _chatService = chatService;
        _chatHistory = chatHistory;
        _settings = settings;
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

        await foreach (var chunk in _chatService.GetStreamingChatMessageContentsAsync(
            _chatHistory,
            _settings,
            cancellationToken: _cancellationToken))
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

            var json = JsonSerializer.Serialize(streamChunk);
            var data = Encoding.UTF8.GetBytes($"data: {json}\n\n");
            await response.Body.WriteAsync(data, _cancellationToken);
            await response.Body.FlushAsync(_cancellationToken);
        }

        // Send final chunk with finish_reason
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

        var finalJson = JsonSerializer.Serialize(finalChunk);
        var finalData = Encoding.UTF8.GetBytes($"data: {finalJson}\n\ndata: [DONE]\n\n");
        await response.Body.WriteAsync(finalData, _cancellationToken);
        await response.Body.FlushAsync(_cancellationToken);
    }
}

#endregion
