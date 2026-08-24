using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Encryption;

namespace SufiChain.SufiPlatform.SufiAI.Providers;

/// <summary>
/// OpenAI provider implementation supporting chat, audio, vision, and embeddings
/// </summary>
public class OpenAIProvider : IAIProvider, ITransientDependency
{
    private const string DefaultBaseUrl = "https://api.openai.com/v1";
    private const string ProviderDidNotReturnUsage = "ProviderDidNotReturnUsage";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<OpenAIProvider> _logger;
    private readonly IStringEncryptionService _stringEncryptor;

    public AIProviderType ProviderType => AIProviderType.OpenAI;

    public OpenAIProvider(
        IHttpClientFactory httpClientFactory,
        ILogger<OpenAIProvider> logger,
        IStringEncryptionService stringEncryptor)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _stringEncryptor = stringEncryptor;
    }

    public bool SupportsCapability(AICapabilityType capabilityType)
    {
        return capabilityType switch
        {
            AICapabilityType.ChatCompletion => true,
            AICapabilityType.AudioTranscription => true,
            AICapabilityType.TextToSpeech => true,
            AICapabilityType.VisionAnalysis => true,
            AICapabilityType.Embeddings => true,
            AICapabilityType.ImageGeneration => true,
            _ => false
        };
    }

    public async Task<ChatCompletionResponse> SendChatMessageAsync(
        Workspace workspace,
        AIModelConfiguration configuration,
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        return ResolveApiMode(workspace, configuration) == OpenAIApiMode.Responses
            ? await SendResponsesMessageAsync(workspace, configuration, request, cancellationToken)
            : await SendChatCompletionsMessageAsync(workspace, configuration, request, cancellationToken);
    }

    public async IAsyncEnumerable<ChatCompletionResponse> StreamChatMessageAsync(
        Workspace workspace,
        AIModelConfiguration configuration,
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var stream = ResolveApiMode(workspace, configuration) == OpenAIApiMode.Responses
            ? StreamResponsesMessageAsync(workspace, configuration, request, cancellationToken)
            : StreamChatCompletionsMessageAsync(workspace, configuration, request, cancellationToken);

        await foreach (var chunk in stream.WithCancellation(cancellationToken))
        {
            yield return chunk;
        }
    }

    public async Task<AudioTranscriptionResponse> TranscribeAudioAsync(
        Workspace workspace,
        AIModelConfiguration configuration,
        AudioTranscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpClient = CreateHttpClient(workspace, configuration);
        var baseUrl = GetBaseUrl(workspace, configuration);

        using var formData = new MultipartFormDataContent();
        formData.Add(new ByteArrayContent(request.AudioData), "file", $"audio.{request.AudioFormat}");
        formData.Add(new StringContent(configuration.ModelId), "model");
        formData.Add(new StringContent("json"), "response_format");

        if (!string.IsNullOrEmpty(request.Language))
        {
            formData.Add(new StringContent(request.Language), "language");
        }

        if (!string.IsNullOrEmpty(request.Prompt))
        {
            formData.Add(new StringContent(request.Prompt), "prompt");
        }

        var response = await httpClient.PostAsync($"{baseUrl}/audio/transcriptions", formData, cancellationToken);
        await EnsureProviderSuccessAsync(response, "audio/transcriptions", configuration.ModelId, cancellationToken);

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<JsonElement>(responseJson);
        var usage = TryReadUsage(result, out var tokenUsage) ? tokenUsage : TokenUsage.Unavailable;

        return new AudioTranscriptionResponse
        {
            Text = result.GetProperty("text").GetString() ?? string.Empty,
            ModelId = configuration.ModelId,
            Language = result.TryGetProperty("language", out var lang) ? lang.GetString() : null,
            InputTokens = usage.InputTokens,
            OutputTokens = usage.OutputTokens,
            TotalTokens = usage.TotalTokens,
            UsageUnavailableReason = usage.HasUsage ? null : ProviderDidNotReturnUsage
        };
    }

    public async Task<TextToSpeechResponse> GenerateSpeechAsync(
        Workspace workspace,
        AIModelConfiguration configuration,
        TextToSpeechRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpClient = CreateHttpClient(workspace, configuration);
        var baseUrl = GetBaseUrl(workspace, configuration);

        var requestBody = new
        {
            model = configuration.ModelId,
            input = request.Text,
            voice = request.Voice ?? "alloy",
            response_format = request.AudioFormat ?? "mp3",
            speed = request.Speed ?? 1.0
        };

        var response = await httpClient.PostAsync($"{baseUrl}/audio/speech", CreateJsonContent(requestBody), cancellationToken);
        await EnsureProviderSuccessAsync(response, "audio/speech", configuration.ModelId, cancellationToken);

        var audioData = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        return new TextToSpeechResponse
        {
            AudioData = audioData,
            ModelId = configuration.ModelId,
            AudioFormat = request.AudioFormat ?? "mp3"
        };
    }

    public async Task<VisionAnalysisResponse> AnalyzeImageAsync(
        Workspace workspace,
        AIModelConfiguration configuration,
        VisionAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        if (ResolveApiMode(workspace, configuration) == OpenAIApiMode.Responses)
        {
            return await AnalyzeImageWithResponsesAsync(workspace, configuration, request, cancellationToken);
        }

        var httpClient = CreateHttpClient(workspace, configuration);
        var baseUrl = GetBaseUrl(workspace, configuration);
        var imageUrl = ToDataUrl(request.ImageData, request.ImageFormat);

        var requestBody = new
        {
            model = configuration.ModelId,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = request.Prompt },
                        new { type = "image_url", image_url = new { url = imageUrl } }
                    }
                }
            },
            max_tokens = request.MaxTokens ?? 300
        };

        var response = await httpClient.PostAsync($"{baseUrl}/chat/completions", CreateJsonContent(requestBody), cancellationToken);
        await EnsureProviderSuccessAsync(response, "chat/completions", configuration.ModelId, cancellationToken);

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<JsonElement>(responseJson);
        var choice = result.GetProperty("choices")[0];
        var message = choice.GetProperty("message");
        var usage = TryReadUsage(result, out var tokenUsage) ? tokenUsage : TokenUsage.Unavailable;

        return new VisionAnalysisResponse
        {
            Description = message.GetProperty("content").GetString() ?? string.Empty,
            ModelId = configuration.ModelId,
            InputTokens = usage.InputTokens,
            OutputTokens = usage.OutputTokens,
            TotalTokens = usage.TotalTokens,
            UsageUnavailableReason = usage.HasUsage ? null : ProviderDidNotReturnUsage
        };
    }

    public async Task<EmbeddingsResponse> GenerateEmbeddingsAsync(
        Workspace workspace,
        AIModelConfiguration configuration,
        EmbeddingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpClient = CreateHttpClient(workspace, configuration);
        var baseUrl = GetBaseUrl(workspace, configuration);

        var requestBody = new
        {
            model = configuration.ModelId,
            input = request.Text
        };

        var response = await httpClient.PostAsync($"{baseUrl}/embeddings", CreateJsonContent(requestBody), cancellationToken);
        await EnsureProviderSuccessAsync(response, "embeddings", configuration.ModelId, cancellationToken);

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<JsonElement>(responseJson);

        var embeddingArray = result.GetProperty("data")[0].GetProperty("embedding");
        var embedding = new float[embeddingArray.GetArrayLength()];
        for (var i = 0; i < embedding.Length; i++)
        {
            embedding[i] = embeddingArray[i].GetSingle();
        }

        var usage = TryReadUsage(result, out var tokenUsage) ? tokenUsage : TokenUsage.Unavailable;

        return new EmbeddingsResponse
        {
            Embedding = embedding,
            ModelId = configuration.ModelId,
            TotalTokens = usage.TotalTokens,
            UsageUnavailableReason = usage.HasUsage ? null : ProviderDidNotReturnUsage
        };
    }

    private async Task<ChatCompletionResponse> SendChatCompletionsMessageAsync(
        Workspace workspace,
        AIModelConfiguration configuration,
        ChatCompletionRequest request,
        CancellationToken cancellationToken)
    {
        var httpClient = CreateHttpClient(workspace, configuration);
        var baseUrl = GetBaseUrl(workspace, configuration);

        var requestBody = new
        {
            model = configuration.ModelId,
            messages = BuildChatCompletionsMessages(request.Messages, request.SystemPrompt ?? workspace.SystemPrompt),
            temperature = request.Temperature ?? workspace.Temperature,
            max_tokens = request.MaxTokens,
            stream = false
        };

        var response = await httpClient.PostAsync($"{baseUrl}/chat/completions", CreateJsonContent(requestBody), cancellationToken);
        await EnsureProviderSuccessAsync(response, "chat/completions", configuration.ModelId, cancellationToken);

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<JsonElement>(responseJson);
        if (!result.TryGetProperty("choices", out var choices) ||
            choices.ValueKind != JsonValueKind.Array ||
            choices.GetArrayLength() == 0)
        {
            _logger.LogError(
                "OpenAI chat completion response did not contain choices. Model={Model} ResponseKeys={ResponseKeys}",
                configuration.ModelId,
                string.Join(",", result.EnumerateObject().Select(property => property.Name)));
            throw new BusinessException("AI:InvalidProviderResponse")
                .WithData("Model", configuration.ModelId);
        }

        var choice = choices[0];
        if (!choice.TryGetProperty("message", out var message))
        {
            _logger.LogError(
                "OpenAI chat completion choice did not contain message. Model={Model} ChoiceKeys={ChoiceKeys}",
                configuration.ModelId,
                string.Join(",", choice.EnumerateObject().Select(property => property.Name)));
            throw new BusinessException("AI:InvalidProviderResponse")
                .WithData("Model", configuration.ModelId);
        }

        var usage = TryReadUsage(result, out var tokenUsage) ? tokenUsage : TokenUsage.Unavailable;
        var content = message.TryGetProperty("content", out var contentProperty) &&
                      contentProperty.ValueKind == JsonValueKind.String
            ? contentProperty.GetString() ?? string.Empty
            : string.Empty;

        return new ChatCompletionResponse
        {
            Content = content,
            ModelId = configuration.ModelId,
            InputTokens = usage.InputTokens,
            OutputTokens = usage.OutputTokens,
            TotalTokens = usage.TotalTokens,
            UsageUnavailableReason = usage.HasUsage ? null : ProviderDidNotReturnUsage,
            FinishReason = choice.TryGetProperty("finish_reason", out var finishReason) ? finishReason.GetString() : null
        };
    }

    private async IAsyncEnumerable<ChatCompletionResponse> StreamChatCompletionsMessageAsync(
        Workspace workspace,
        AIModelConfiguration configuration,
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var httpClient = CreateHttpClient(workspace, configuration);
        var baseUrl = GetBaseUrl(workspace, configuration);

        var requestBody = new
        {
            model = configuration.ModelId,
            messages = BuildChatCompletionsMessages(request.Messages, request.SystemPrompt ?? workspace.SystemPrompt),
            temperature = request.Temperature ?? workspace.Temperature,
            max_tokens = request.MaxTokens,
            stream = true,
            stream_options = new { include_usage = true }
        };

        await foreach (var chunk in StreamJsonLinesAsync(httpClient, $"{baseUrl}/chat/completions", requestBody, cancellationToken))
        {
            if (TryReadUsage(chunk, out var usage))
            {
                yield return new ChatCompletionResponse
                {
                    ModelId = configuration.ModelId,
                    IsUsageChunk = true,
                    InputTokens = usage.InputTokens,
                    OutputTokens = usage.OutputTokens,
                    TotalTokens = usage.TotalTokens
                };
                continue;
            }

            if (!chunk.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
            {
                continue;
            }

            var choice = choices[0];
            if (!choice.TryGetProperty("delta", out var delta) || !delta.TryGetProperty("content", out var contentProp))
            {
                continue;
            }

            yield return new ChatCompletionResponse
            {
                Content = contentProp.GetString() ?? string.Empty,
                ModelId = configuration.ModelId
            };
        }
    }

    private async Task<ChatCompletionResponse> SendResponsesMessageAsync(
        Workspace workspace,
        AIModelConfiguration configuration,
        ChatCompletionRequest request,
        CancellationToken cancellationToken)
    {
        var httpClient = CreateHttpClient(workspace, configuration);
        var baseUrl = GetBaseUrl(workspace, configuration);
        var requestBody = BuildResponsesRequest(workspace, configuration, request, stream: false);

        var response = await httpClient.PostAsync($"{baseUrl}/responses", CreateJsonContent(requestBody), cancellationToken);
        await EnsureProviderSuccessAsync(response, "responses", configuration.ModelId, cancellationToken);

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<JsonElement>(responseJson);
        var usage = TryReadUsage(result, out var tokenUsage) ? tokenUsage : TokenUsage.Unavailable;

        return new ChatCompletionResponse
        {
            Content = ReadResponsesOutputText(result),
            ModelId = configuration.ModelId,
            InputTokens = usage.InputTokens,
            OutputTokens = usage.OutputTokens,
            TotalTokens = usage.TotalTokens,
            UsageUnavailableReason = usage.HasUsage ? null : ProviderDidNotReturnUsage
        };
    }

    private async IAsyncEnumerable<ChatCompletionResponse> StreamResponsesMessageAsync(
        Workspace workspace,
        AIModelConfiguration configuration,
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var httpClient = CreateHttpClient(workspace, configuration);
        var baseUrl = GetBaseUrl(workspace, configuration);
        var requestBody = BuildResponsesRequest(workspace, configuration, request, stream: true);

        await foreach (var chunk in StreamJsonLinesAsync(httpClient, $"{baseUrl}/responses", requestBody, cancellationToken))
        {
            if (!chunk.TryGetProperty("type", out var typeProperty))
            {
                continue;
            }

            var type = typeProperty.GetString();
            if (type == "response.output_text.delta" && chunk.TryGetProperty("delta", out var delta))
            {
                yield return new ChatCompletionResponse
                {
                    Content = delta.GetString() ?? string.Empty,
                    ModelId = configuration.ModelId
                };
                continue;
            }

            if (type == "response.completed" && chunk.TryGetProperty("response", out var response))
            {
                var usage = TryReadUsage(response, out var tokenUsage) ? tokenUsage : TokenUsage.Unavailable;
                yield return new ChatCompletionResponse
                {
                    ModelId = configuration.ModelId,
                    IsUsageChunk = true,
                    InputTokens = usage.InputTokens,
                    OutputTokens = usage.OutputTokens,
                    TotalTokens = usage.TotalTokens,
                    UsageUnavailableReason = usage.HasUsage ? null : ProviderDidNotReturnUsage
                };
                continue;
            }

            if (type == "error")
            {
                var message = chunk.TryGetProperty("message", out var messageProperty)
                    ? messageProperty.GetString()
                    : "OpenAI Responses stream error";
                throw new BusinessException("AI:OpenAIResponsesStreamError")
                    .WithData("Message", message);
            }
        }
    }

    private async Task<VisionAnalysisResponse> AnalyzeImageWithResponsesAsync(
        Workspace workspace,
        AIModelConfiguration configuration,
        VisionAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        var httpClient = CreateHttpClient(workspace, configuration);
        var baseUrl = GetBaseUrl(workspace, configuration);
        var chatRequest = new ChatCompletionRequest
        {
            WorkspaceName = request.WorkspaceName,
            Messages = new List<ChatMessage>
            {
                new()
                {
                    Role = "user",
                    MultiModalContent = new List<MessageContent>
                    {
                        new() { Type = "text", Text = request.Prompt },
                        new() { Type = "image_url", ImageUrl = new ImageContent { Url = ToDataUrl(request.ImageData, request.ImageFormat) } }
                    }
                }
            },
            MaxTokens = request.MaxTokens
        };
        var requestBody = BuildResponsesRequest(workspace, configuration, chatRequest, stream: false);

        var response = await httpClient.PostAsync($"{baseUrl}/responses", CreateJsonContent(requestBody), cancellationToken);
        await EnsureProviderSuccessAsync(response, "responses", configuration.ModelId, cancellationToken);

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<JsonElement>(responseJson);
        var usage = TryReadUsage(result, out var tokenUsage) ? tokenUsage : TokenUsage.Unavailable;

        return new VisionAnalysisResponse
        {
            Description = ReadResponsesOutputText(result),
            ModelId = configuration.ModelId,
            InputTokens = usage.InputTokens,
            OutputTokens = usage.OutputTokens,
            TotalTokens = usage.TotalTokens,
            UsageUnavailableReason = usage.HasUsage ? null : ProviderDidNotReturnUsage
        };
    }

    private static List<object> BuildChatCompletionsMessages(IEnumerable<ChatMessage> messages, string? systemPrompt)
    {
        var result = new List<object>();
        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            result.Add(new { role = "system", content = systemPrompt });
        }

        foreach (var message in messages)
        {
            result.Add(new
            {
                role = message.Role,
                content = BuildChatCompletionsContent(message)
            });
        }

        return result;
    }

    private static object BuildChatCompletionsContent(ChatMessage message)
    {
        if (message.MultiModalContent == null || !message.MultiModalContent.Any())
        {
            return message.Content;
        }

        return message.MultiModalContent.Select(content => content.Type == "text"
            ? (object)new { type = "text", text = content.Text }
            : new { type = "image_url", image_url = new { url = content.ImageUrl?.Url, detail = content.ImageUrl?.Detail } }).ToList();
    }

    private static object BuildResponsesRequest(
        Workspace workspace,
        AIModelConfiguration configuration,
        ChatCompletionRequest request,
        bool stream)
    {
        return new
        {
            model = configuration.ModelId,
            instructions = request.SystemPrompt ?? workspace.SystemPrompt,
            input = BuildResponsesInput(request.Messages),
            temperature = request.Temperature ?? workspace.Temperature,
            max_output_tokens = request.MaxTokens,
            stream
        };
    }

    private static List<object> BuildResponsesInput(IEnumerable<ChatMessage> messages)
    {
        return messages.Select(message => new
        {
            role = message.Role,
            content = BuildResponsesContent(message)
        }).Cast<object>().ToList();
    }

    private static List<object> BuildResponsesContent(ChatMessage message)
    {
        if (message.MultiModalContent == null || !message.MultiModalContent.Any())
        {
            return new List<object> { new { type = "input_text", text = message.Content } };
        }

        return message.MultiModalContent.Select(content => content.Type == "text"
            ? (object)new { type = "input_text", text = content.Text }
            : new { type = "input_image", image_url = content.ImageUrl?.Url, detail = content.ImageUrl?.Detail }).ToList();
    }

    private async IAsyncEnumerable<JsonElement> StreamJsonLinesAsync(
        HttpClient httpClient,
        string url,
        object requestBody,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = CreateJsonContent(requestBody)
        };

        var response = await httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await EnsureProviderSuccessAsync(response, url, "streaming", cancellationToken);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new System.IO.StreamReader(stream);

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
            {
                continue;
            }

            var data = line.Substring(6);
            if (data == "[DONE]")
            {
                break;
            }

            JsonElement chunk;
            try
            {
                chunk = JsonSerializer.Deserialize<JsonElement>(data);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to parse streaming chunk: {Data}", data);
                continue;
            }

            yield return chunk;
        }
    }

    private static bool TryReadUsage(JsonElement result, out TokenUsage usage)
    {
        usage = TokenUsage.Unavailable;
        if (!result.TryGetProperty("usage", out var usageElement) || usageElement.ValueKind == JsonValueKind.Null)
        {
            return false;
        }

        var inputTokens = TryGetInt32(usageElement, "prompt_tokens") ?? TryGetInt32(usageElement, "input_tokens");
        var outputTokens = TryGetInt32(usageElement, "completion_tokens") ?? TryGetInt32(usageElement, "output_tokens");
        var totalTokens = TryGetInt32(usageElement, "total_tokens");

        usage = new TokenUsage(inputTokens, outputTokens, totalTokens);
        return usage.HasUsage;
    }

    private static int? TryGetInt32(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
            ? property.GetInt32()
            : null;
    }

    private static string ReadResponsesOutputText(JsonElement result)
    {
        if (result.TryGetProperty("output_text", out var outputText) && outputText.ValueKind == JsonValueKind.String)
        {
            return outputText.GetString() ?? string.Empty;
        }

        if (!result.TryGetProperty("output", out var output) || output.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var outputItem in output.EnumerateArray())
        {
            if (!outputItem.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var contentItem in content.EnumerateArray())
            {
                if (contentItem.TryGetProperty("type", out var type) &&
                    type.GetString() == "output_text" &&
                    contentItem.TryGetProperty("text", out var text))
                {
                    builder.Append(text.GetString());
                }
            }
        }

        return builder.ToString();
    }

    private async Task EnsureProviderSuccessAsync(
        HttpResponseMessage response,
        string operation,
        string modelId,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var error = ParseProviderError(responseBody, response);
        var requestId = ReadRequestId(response, error);
        var safeOperation = SanitizeOperation(operation);
        var statusCode = (int)response.StatusCode;

        _logger.LogWarning(
            "OpenAI-compatible provider request failed. Operation: {Operation}, Model: {ModelId}, Status: {StatusCode}, ErrorType: {ErrorType}, ErrorCode: {ErrorCode}, Param: {Param}, RequestId: {RequestId}, Body: {Body}",
            safeOperation,
            modelId,
            statusCode,
            error.Type,
            error.Code,
            error.Param,
            requestId,
            TrimForLog(responseBody));

        throw new BusinessException(AIErrorCodes.ProviderRequestFailed)
            .WithData("StatusCode", statusCode)
            .WithData("Operation", safeOperation)
            .WithData("ModelId", modelId)
            .WithData("RequestId", requestId ?? string.Empty);
    }

    private static string SanitizeOperation(string operation)
    {
        if (!Uri.TryCreate(operation, UriKind.Absolute, out var uri))
        {
            return operation;
        }

        if (uri.AbsolutePath.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
        {
            return "chat/completions";
        }

        if (uri.AbsolutePath.EndsWith("/responses", StringComparison.OrdinalIgnoreCase))
        {
            return "responses";
        }

        if (uri.AbsolutePath.EndsWith("/embeddings", StringComparison.OrdinalIgnoreCase))
        {
            return "embeddings";
        }

        if (uri.AbsolutePath.EndsWith("/audio/transcriptions", StringComparison.OrdinalIgnoreCase))
        {
            return "audio/transcriptions";
        }

        if (uri.AbsolutePath.EndsWith("/audio/speech", StringComparison.OrdinalIgnoreCase))
        {
            return "audio/speech";
        }

        return "provider endpoint";
    }

    private static ProviderError ParseProviderError(string? responseBody, HttpResponseMessage response)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return new ProviderError(
                $"{(int)response.StatusCode} {response.ReasonPhrase}",
                null,
                null,
                null,
                null);
        }

        if (LooksLikeHtmlErrorPage(responseBody))
        {
            return new ProviderError(
                ExtractHtmlErrorSummary(responseBody, response),
                null,
                null,
                null,
                null);
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return new ProviderError(NormalizeProviderErrorText(root.ToString()), null, null, null, null);
            }

            var errorElement = root.TryGetProperty("error", out var error) ? error : root;
            if (errorElement.ValueKind != JsonValueKind.Object)
            {
                return new ProviderError(
                    NormalizeProviderErrorText(
                        errorElement.ValueKind == JsonValueKind.String ? errorElement.GetString() : errorElement.ToString()),
                    null,
                    null,
                    null,
                    TryGetString(root, "request_id") ?? TryGetString(root, "_request_id"));
            }

            return new ProviderError(
                NormalizeProviderErrorText(TryGetString(errorElement, "message") ?? TryGetString(root, "message")),
                TryGetString(errorElement, "type"),
                TryGetString(errorElement, "param"),
                TryGetString(errorElement, "code"),
                TryGetString(root, "request_id") ?? TryGetString(root, "_request_id"));
        }
        catch (JsonException)
        {
            return new ProviderError(NormalizeProviderErrorText(responseBody), null, null, null, null);
        }
    }

    private static bool LooksLikeHtmlErrorPage(string responseBody)
    {
        var trimmed = responseBody.TrimStart();
        return trimmed.StartsWith("<!", StringComparison.OrdinalIgnoreCase)
               || trimmed.StartsWith("<html", StringComparison.OrdinalIgnoreCase)
               || trimmed.Contains("error-section__status", StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractHtmlErrorSummary(string responseBody, HttpResponseMessage response)
    {
        var statusMatch = System.Text.RegularExpressions.Regex.Match(
            responseBody,
            @"error-section__status[^>]*>([^<]+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        var reasonMatch = System.Text.RegularExpressions.Regex.Match(
            responseBody,
            @"error-section__reason[^>]*>([^<]+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        if (statusMatch.Success || reasonMatch.Success)
        {
            var status = statusMatch.Success ? statusMatch.Groups[1].Value.Trim() : $"{(int)response.StatusCode}";
            var reason = reasonMatch.Success ? reasonMatch.Groups[1].Value.Trim() : response.ReasonPhrase ?? "Error";
            return $"{status} {reason}".Trim();
        }

        return $"{(int)response.StatusCode} {response.ReasonPhrase}";
    }

    private static string? NormalizeProviderErrorText(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return message;
        }

        if (LooksLikeHtmlErrorPage(message))
        {
            return "Gateway error";
        }

        return message.Length <= 500 ? message : message[..500] + "...";
    }

    private static string? ReadRequestId(HttpResponseMessage response, ProviderError error)
    {
        if (!string.IsNullOrWhiteSpace(error.RequestId))
        {
            return error.RequestId;
        }

        return response.Headers.TryGetValues("x-request-id", out var values)
            ? values.FirstOrDefault()
            : ReadClientRequestId(response);
    }

    private static string? ReadClientRequestId(HttpResponseMessage response)
    {
        return response.RequestMessage?.Headers.TryGetValues("X-Client-Request-Id", out var values) == true
            ? values.FirstOrDefault()
            : null;
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    private static string TrimForLog(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Length <= 2000 ? value : value[..2000];
    }

    private static StringContent CreateJsonContent(object requestBody)
    {
        var json = JsonSerializer.Serialize(requestBody, JsonOptions);
        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    private static OpenAIApiMode ResolveApiMode(Workspace workspace, AIModelConfiguration configuration)
    {
        return configuration.OpenAIApiMode ?? workspace.OpenAIApiMode;
    }

    private static string GetBaseUrl(Workspace workspace, AIModelConfiguration configuration)
    {
        return (configuration.ApiEndpoint ?? workspace.ApiBaseUrl ?? DefaultBaseUrl).TrimEnd('/');
    }

    private static string ToDataUrl(byte[] data, string format)
    {
        return $"data:image/{format};base64,{Convert.ToBase64String(data)}";
    }

    private HttpClient CreateHttpClient(Workspace workspace, AIModelConfiguration configuration)
    {
        var httpClient = _httpClientFactory.CreateClient();

        var apiKey = DecryptApiKey(configuration.ApiKey ?? workspace.ApiKey);
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new BusinessException("AI:ApiKeyRequired")
                .WithData("WorkspaceName", workspace.Name)
                .WithData("Provider", workspace.Provider.ToString());
        }

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        httpClient.DefaultRequestHeaders.Add("X-Client-Request-Id", Guid.NewGuid().ToString("D"));
        httpClient.Timeout = TimeSpan.FromMinutes(5);

        return httpClient;
    }

    private string? DecryptApiKey(string? encryptedApiKey)
    {
        if (string.IsNullOrWhiteSpace(encryptedApiKey))
        {
            return encryptedApiKey;
        }

        try
        {
            return _stringEncryptor.Decrypt(encryptedApiKey);
        }
        catch
        {
            return encryptedApiKey;
        }
    }

    private sealed record TokenUsage(int? InputTokens, int? OutputTokens, int? TotalTokens)
    {
        public static TokenUsage Unavailable { get; } = new(null, null, null);
        public bool HasUsage => InputTokens.HasValue || OutputTokens.HasValue || TotalTokens.HasValue;
    }

    private sealed record ProviderError(
        string? Message,
        string? Type,
        string? Param,
        string? Code,
        string? RequestId)
    {
        public static ProviderError Empty { get; } = new(null, null, null, null, null);
    }
}
