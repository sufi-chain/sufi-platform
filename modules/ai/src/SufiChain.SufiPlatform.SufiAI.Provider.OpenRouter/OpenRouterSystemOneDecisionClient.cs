using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Provider.OpenRouter;

public class OpenRouterSystemOneDecisionClient : ISystemOneDecisionClient, ITransientDependency
{
    private readonly IWorkspaceRepository _workspaces;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAICredentialResolver _credentials;
    private readonly IWorkspaceRuntimeConfigurationResolver _runtime;
    private readonly IAIUsageRecorder _usage;
    private readonly ILogger<OpenRouterSystemOneDecisionClient> _logger;

    public OpenRouterSystemOneDecisionClient(
        IWorkspaceRepository workspaces,
        IHttpClientFactory httpClientFactory,
        IAICredentialResolver credentials,
        IWorkspaceRuntimeConfigurationResolver runtime,
        IAIUsageRecorder usage,
        ILogger<OpenRouterSystemOneDecisionClient> logger)
    {
        _workspaces = workspaces;
        _httpClientFactory = httpClientFactory;
        _credentials = credentials;
        _runtime = runtime;
        _usage = usage;
        _logger = logger;
    }

    public async Task<SystemOneDecisionResult?> TryDecideAsync(
        SystemOneDecisionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Questions.Count == 0)
        {
            return null;
        }

        WorkspaceRuntimeConfiguration? resolved = null;
        var sent = false;
        var stopwatch = new Stopwatch();
        try
        {
            var workspace = await _workspaces.FindAsync(request.WorkspaceId, includeDetails: true, cancellationToken: cancellationToken);
            if (workspace == null || workspace.Provider != AIProviderType.OpenRouter)
            {
                return null;
            }

            var route = workspace.ModelConfigurations
                .Where(item => item.CapabilityType == AICapabilityType.Decisions && item.IsEnabled)
                .OrderBy(item => item.Priority)
                .FirstOrDefault();
            if (route == null || string.IsNullOrWhiteSpace(route.ModelId))
            {
                return null;
            }

            var apiKey = _credentials.DecryptApiKey(route.ApiKey) ?? _credentials.DecryptApiKey(workspace.ApiKey);
            var baseUrl = (route.ApiEndpoint ?? workspace.ApiBaseUrl ?? "https://openrouter.ai/api/v1").TrimEnd('/');
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return null;
            }

            resolved = _runtime.Resolve(workspace, AICapabilityType.Decisions, route);
            if (route.OutputPrice is null)
            {
                resolved = resolved.WithOutputPrice(null);
            }

            using var message = new HttpRequestMessage(HttpMethod.Post, baseUrl + "/systemone")
            {
                Content = new StringContent(
                    SerializeBody(route.ModelId, request.State, request.Questions),
                    Encoding.UTF8,
                    "application/json")
            };
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            stopwatch.Restart();
            sent = true;
            using var response = await _httpClientFactory.CreateClient().SendAsync(message, cancellationToken);
            stopwatch.Stop();
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "System One request failed. Model={Model}, Status={StatusCode}, Body={Body}",
                    route.ModelId,
                    (int)response.StatusCode,
                    Trim(body));
                await RecordAsync(resolved, stopwatch.ElapsedMilliseconds, false, "SystemOneRequestFailed", null, null, cancellationToken);
                return null;
            }

            var usage = ReadUsage(body);
            var parsed = TryParse(body);
            if (parsed == null && usage.InputTokens == null && usage.OutputTokens == null)
            {
                await RecordAsync(resolved, stopwatch.ElapsedMilliseconds, false, "SystemOneResponseUnreadable", null, null, cancellationToken);
                return null;
            }

            await RecordAsync(
                resolved,
                stopwatch.ElapsedMilliseconds,
                true,
                null,
                usage.InputTokens,
                usage.OutputTokens,
                cancellationToken);
            return parsed;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (sent && resolved != null)
            {
                stopwatch.Stop();
                await RecordAsync(resolved, stopwatch.ElapsedMilliseconds, false, "SystemOneRequestFailed", null, null, cancellationToken);
            }

            _logger.LogWarning(ex, "System One request is unavailable.");
            return null;
        }
    }

    private Task RecordAsync(
        WorkspaceRuntimeConfiguration resolved,
        long latencyMs,
        bool isSuccess,
        string? errorMessage,
        int? inputTokens,
        int? outputTokens,
        CancellationToken cancellationToken)
    {
        return _usage.RecordAsync(
            new AIUsageRecord
            {
                Configuration = resolved,
                CapabilityType = AICapabilityType.Decisions,
                InputTokens = inputTokens,
                OutputTokens = outputTokens,
                TotalTokens = inputTokens.HasValue || outputTokens.HasValue
                    ? (inputTokens ?? 0) + (outputTokens ?? 0)
                    : null,
                UsageUnavailableReason = inputTokens.HasValue || outputTokens.HasValue
                    ? null
                    : AIUsageRecorder.UsageUnavailable,
                LatencyMs = latencyMs,
                IsSuccess = isSuccess,
                ErrorMessage = errorMessage
            },
            cancellationToken);
    }

    /// <summary>
    /// OpenRouter rejects a JSON null <c>criteria</c> field. Omit it unless the question has one.
    /// </summary>
    public static string SerializeBody(
        string modelId,
        object? state,
        IReadOnlyDictionary<string, SystemOneQuestion> questions)
    {
        var payloadQuestions = new Dictionary<string, Dictionary<string, object?>>(StringComparer.Ordinal);
        foreach (var item in questions)
        {
            var question = new Dictionary<string, object?>
            {
                ["type"] = item.Value.Type,
                ["instructions"] = item.Value.Instructions
            };
            if (item.Value.Criteria != null)
            {
                question["criteria"] = item.Value.Criteria;
            }

            payloadQuestions[item.Key] = question;
        }

        return JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["model"] = modelId,
            ["state"] = state,
            ["questions"] = payloadQuestions
        });
    }

    public static SystemOneDecisionResult? Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        if (!root.TryGetProperty("answers", out var answers) || answers.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var parsed = new Dictionary<string, SystemOneAnswer>(StringComparer.Ordinal);
        foreach (var answer in answers.EnumerateObject())
        {
            var type = answer.Value.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : null;
            if (string.IsNullOrWhiteSpace(type))
            {
                return null;
            }

            double? noul = answer.Value.TryGetProperty("noul", out var noulElement) && noulElement.TryGetDouble(out var noulValue)
                ? noulValue
                : null;
            if (type == "noul" && noul is < 0 or > 1)
            {
                return null;
            }

            parsed[answer.Name] = new SystemOneAnswer
            {
                Type = type,
                Noul = noul,
                Choice = answer.Value.TryGetProperty("choice", out var choiceElement) ? choiceElement.GetString() : null,
                Confidence = answer.Value.TryGetProperty("confidence", out var confidenceElement) && confidenceElement.TryGetDouble(out var confidence)
                    ? confidence
                    : null
            };
        }

        foreach (var question in parsed)
        {
            if (question.Value.Type == "noul" && question.Value.Noul == null)
            {
                return null;
            }
        }

        int? inputTokens = null;
        int? outputTokens = null;
        decimal? cost = null;
        if (root.TryGetProperty("usage", out var usage))
        {
            inputTokens = ReadInt(usage, "input_tokens");
            outputTokens = ReadInt(usage, "output_tokens");
            if (usage.TryGetProperty("cost", out var costElement) && costElement.TryGetDecimal(out var costValue))
            {
                cost = costValue;
            }
        }

        return new SystemOneDecisionResult
        {
            Answers = parsed,
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
            Cost = cost
        };
    }

    private static SystemOneDecisionResult? TryParse(string json)
    {
        try
        {
            return Parse(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static (int? InputTokens, int? OutputTokens) ReadUsage(string json)
    {
        try
        {
            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("usage", out var usage))
            {
                return (null, null);
            }

            return (ReadInt(usage, "input_tokens"), ReadInt(usage, "output_tokens"));
        }
        catch (JsonException)
        {
            return (null, null);
        }
    }

    private static string Trim(string value)
    {
        const int limit = 400;
        var compact = value.Replace("\r", " ").Replace("\n", " ").Trim();
        return compact.Length <= limit ? compact : compact[..limit];
    }

    private static int? ReadInt(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var property) && property.TryGetInt32(out var value)
            ? value
            : null;
    }
}
