using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using Volo.Abp.Security.Encryption;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

[RequiresFeature(SufiAIFeatures.Enable)]
[Authorize(AIPermissions.Workspaces.Default)]
public class WorkspaceAppService : SufiApplicationService, IWorkspaceAppService
{
    private const string DefaultOpenAIBaseUrl = "https://api.openai.com/v1";

    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IAIModelConfigurationRepository _modelConfigurationRepository;
    private readonly WorkspaceManager _workspaceManager;
    private readonly IStringEncryptionService _stringEncryptor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWorkspaceRuntimeConfigurationResolver _runtimeConfigurationResolver;
    private readonly WorkspaceSyncService _workspaceSyncService;

    public WorkspaceAppService(
        IWorkspaceRepository workspaceRepository,
        IAIModelConfigurationRepository modelConfigurationRepository,
        WorkspaceManager workspaceManager,
        IStringEncryptionService stringEncryptor,
        IHttpClientFactory httpClientFactory,
        IWorkspaceRuntimeConfigurationResolver runtimeConfigurationResolver,
        WorkspaceSyncService workspaceSyncService)
    {
        _workspaceRepository = workspaceRepository;
        _modelConfigurationRepository = modelConfigurationRepository;
        _workspaceManager = workspaceManager;
        _stringEncryptor = stringEncryptor;
        _httpClientFactory = httpClientFactory;
        _runtimeConfigurationResolver = runtimeConfigurationResolver;
        _workspaceSyncService = workspaceSyncService;
    }

    public async Task<PagedResultDto<WorkspaceDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var totalCount = await _workspaceRepository.GetCountAsync();
        var workspaces = await _workspaceRepository.GetListAsync(
            skipCount: input.SkipCount,
            maxResultCount: input.MaxResultCount,
            sorting: input.Sorting ?? "Name"
        );

        return new PagedResultDto<WorkspaceDto>(
            totalCount,
            ObjectMapper.Map<List<Workspace>, List<WorkspaceDto>>(workspaces)
        );
    }

    public async Task<WorkspaceDto> GetAsync(Guid id)
    {
        var workspace = await _workspaceRepository.GetAsync(id, includeDetails: true);
        return ObjectMapper.Map<Workspace, WorkspaceDto>(workspace);
    }

    public async Task<WorkspaceReadinessDto> GetReadinessAsync(Guid id)
    {
        var workspace = await _workspaceRepository.GetAsync(id, includeDetails: true);
        var capabilityResults = new List<WorkspaceRuntimeConfiguration>();
        foreach (var capabilityType in Enum.GetValues<AICapabilityType>())
        {
            capabilityResults.Add(_runtimeConfigurationResolver.Resolve(workspace, capabilityType));
        }

        var chat = capabilityResults.Single(
            result => result.CapabilityType == AICapabilityType.ChatCompletion);
        var mcpFailureCode = chat.FailureCode;
        if (mcpFailureCode == null && chat.Provider != AIProviderType.OpenAI)
        {
            mcpFailureCode = WorkspaceRuntimeFailureCodes.McpProviderNotSupported;
        }
        else if (mcpFailureCode == null && chat.OpenAIApiMode != OpenAIApiMode.ChatCompletions)
        {
            mcpFailureCode = WorkspaceRuntimeFailureCodes.McpApiModeNotSupported;
        }

        return new WorkspaceReadinessDto
        {
            WorkspaceId = chat.Workspace.Id,
            WorkspaceName = chat.Workspace.Name,
            IsActive = chat.Workspace.IsActive,
            IsConfigured = chat.IsConfigured,
            IsReady = chat.IsReady,
            Capabilities = capabilityResults.Select(MapCapabilityReadiness).ToList(),
            Mcp = new WorkspaceMcpReadinessDto
            {
                IsConfigured = chat.IsConfigured,
                IsReady = mcpFailureCode == null,
                Provider = chat.Provider,
                ModelId = NullIfWhiteSpace(chat.ModelId),
                OpenAIApiMode = chat.OpenAIApiMode,
                FailureCode = mcpFailureCode
            }
        };
    }

    [Authorize(AIPermissions.Workspaces.Create)]
    public async Task<WorkspaceDto> CreateAsync(CreateWorkspaceDto input)
    {
        await _workspaceManager.ValidateNameAsync(input.Name);

        var workspace = new Workspace(
            GuidGenerator.Create(),
            input.Name,
            input.Provider,
            input.Model,
            CurrentTenant.Id
        );

        workspace.UpdateConfiguration(
            input.Model,
            EncryptApiKey(input.ApiKey),
            input.ApiBaseUrl,
            input.SystemPrompt,
            input.Temperature,
            input.MaxContextTokens,
            input.OpenAIApiMode,
            input.InputCostPer1MTokens,
            input.OutputCostPer1MTokens
        );

        await _workspaceRepository.InsertAsync(workspace, autoSave: true);

        return ObjectMapper.Map<Workspace, WorkspaceDto>(workspace);
    }

    [Authorize(AIPermissions.Workspaces.Edit)]
    public async Task<WorkspaceDto> UpdateAsync(Guid id, UpdateWorkspaceDto input)
    {
        var workspace = await _workspaceRepository.GetAsync(id, includeDetails: true);

        await _workspaceManager.ValidateNameAsync(input.Name, id);
        workspace.SetName(input.Name);

        // Only update API key if a new one is provided
        var apiKeyToUpdate = string.IsNullOrWhiteSpace(input.ApiKey) 
            ? workspace.ApiKey  // Keep existing
            : EncryptApiKey(input.ApiKey);  // Encrypt new one

        workspace.UpdateConfiguration(
            input.Model,
            apiKeyToUpdate,
            input.ApiBaseUrl,
            input.SystemPrompt,
            input.Temperature,
            input.MaxContextTokens,
            input.OpenAIApiMode,
            input.InputCostPer1MTokens,
            input.OutputCostPer1MTokens
        );
        workspace.UpdatePrimaryChatConfiguration(
            input.Model,
            input.ApiBaseUrl,
            input.OpenAIApiMode,
            input.InputCostPer1MTokens,
            input.OutputCostPer1MTokens);

        if (input.IsActive)
            workspace.Activate();
        else
            workspace.Deactivate();

        await _workspaceRepository.UpdateAsync(workspace, autoSave: true);
        _workspaceSyncService.ClearWorkspaceCache(workspace.Name);

        return ObjectMapper.Map<Workspace, WorkspaceDto>(workspace);
    }

    [Authorize(AIPermissions.Workspaces.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var workspace = await _workspaceRepository.FindAsync(id);
        await _workspaceRepository.DeleteAsync(id, autoSave: true);
        if (workspace != null)
        {
            _workspaceSyncService.ClearWorkspaceCache(workspace.Name);
        }
    }

    public async Task<List<OpenAIModelDto>> GetAvailableModelsAsync(GetOpenAIModelsInput input)
    {
        var credentials = await ResolveConnectionCredentialsAsync(
            input.WorkspaceId,
            input.ModelConfigurationId,
            input.ApiKey,
            input.ApiBaseUrl);

        if (string.IsNullOrWhiteSpace(credentials.ApiKey))
        {
            throw new Volo.Abp.UserFriendlyException(L["ApiKeyRequiredForModelList"]);
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{credentials.BaseUrl}/models");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credentials.ApiKey);
        request.Headers.Add("X-Client-Request-Id", GuidGenerator.Create().ToString("D"));

        using var response = await _httpClientFactory.CreateClient().SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var error = ParseProviderError(responseBody);
            var requestId = ReadRequestId(response, error);
            Logger.LogWarning(
                "OpenAI-compatible model list request failed. BaseUrl: {BaseUrl}, Status: {StatusCode}, ErrorCode: {ErrorCode}, Param: {Param}, RequestId: {RequestId}, Body: {Body}",
                SanitizeBaseUrlForLog(credentials.BaseUrl),
                (int)response.StatusCode,
                error.Code,
                error.Param,
                requestId,
                TrimError(responseBody));

            throw new Volo.Abp.UserFriendlyException(
                BuildModelListErrorMessage(response, error, requestId)
            );
        }

        var json = await response.Content.ReadAsStringAsync();
        return ParseModels(json);
    }

    public async Task TestConnectionAsync(TestWorkspaceConnectionInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Model))
        {
            throw new Volo.Abp.UserFriendlyException(L["ModelIdRequired"]);
        }

        var credentials = await ResolveConnectionCredentialsAsync(
            input.WorkspaceId,
            input.ModelConfigurationId,
            input.ApiKey,
            input.ApiBaseUrl);

        if (string.IsNullOrWhiteSpace(credentials.ApiKey))
        {
            throw new Volo.Abp.UserFriendlyException(L["ApiKeyRequiredForConnectionTest"]);
        }

        using var request = CreateConnectionTestRequest(
            credentials.BaseUrl,
            credentials.ApiKey,
            input.Model,
            input.CapabilityType,
            input.OpenAIApiMode);

        using var response = await _httpClientFactory.CreateClient().SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        var error = ParseProviderError(responseBody);
        var requestId = ReadRequestId(response, error);
        Logger.LogWarning(
            "OpenAI-compatible connection test failed. Capability: {CapabilityType}, Mode: {OpenAIApiMode}, Model: {Model}, BaseUrl: {BaseUrl}, Status: {StatusCode}, ErrorCode: {ErrorCode}, Param: {Param}, RequestId: {RequestId}, Body: {Body}",
            input.CapabilityType,
            input.OpenAIApiMode,
            input.Model,
            SanitizeBaseUrlForLog(credentials.BaseUrl),
            (int)response.StatusCode,
            error.Code,
            error.Param,
            requestId,
            TrimError(responseBody));

        throw new Volo.Abp.UserFriendlyException(
            BuildConnectionTestErrorMessage(response, error, requestId)
        );
    }

    private async Task<(string? ApiKey, string BaseUrl)> ResolveConnectionCredentialsAsync(
        Guid? workspaceId,
        Guid? modelConfigurationId,
        string? apiKey,
        string? apiBaseUrl)
    {
        SufiChain.SufiPlatform.SufiAI.AIModelConfiguration? modelConfiguration = null;
        if (modelConfigurationId.HasValue)
        {
            modelConfiguration = await _modelConfigurationRepository.GetAsync(modelConfigurationId.Value);
            workspaceId ??= modelConfiguration.WorkspaceId;
        }

        Workspace? workspace = null;
        if (workspaceId.HasValue)
        {
            workspace = await _workspaceRepository.GetAsync(workspaceId.Value);
        }

        var resolvedKey = apiKey;
        if (string.IsNullOrWhiteSpace(resolvedKey) && modelConfiguration != null)
        {
            resolvedKey = DecryptApiKey(modelConfiguration.ApiKey);
        }

        if (string.IsNullOrWhiteSpace(resolvedKey) && workspace != null)
        {
            resolvedKey = DecryptApiKey(workspace.ApiKey);
        }

        var resolvedBaseUrl = apiBaseUrl;
        if (string.IsNullOrWhiteSpace(resolvedBaseUrl) && modelConfiguration != null)
        {
            resolvedBaseUrl = modelConfiguration.ApiEndpoint;
        }

        if (string.IsNullOrWhiteSpace(resolvedBaseUrl) && workspace != null)
        {
            resolvedBaseUrl = workspace.ApiBaseUrl;
        }

        return (resolvedKey, NormalizeBaseUrl(resolvedBaseUrl));
    }

    private HttpRequestMessage CreateConnectionTestRequest(
        string baseUrl,
        string apiKey,
        string model,
        AICapabilityType capabilityType,
        OpenAIApiMode openAIApiMode)
    {
        HttpRequestMessage request;
        if (capabilityType == AICapabilityType.Embeddings)
        {
            request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/embeddings")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(CreateEmbeddingsTestPayload(model)),
                    Encoding.UTF8,
                    "application/json")
            };
        }
        else if (capabilityType is AICapabilityType.AudioTranscription
                 or AICapabilityType.TextToSpeech
                 or AICapabilityType.ImageGeneration)
        {
            // Auth + endpoint smoke test; these capabilities need binary/multipart probes.
            request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/models");
        }
        else
        {
            var requestUri = openAIApiMode == OpenAIApiMode.Responses
                ? $"{baseUrl}/responses"
                : $"{baseUrl}/chat/completions";
            request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(CreateTestPayload(model, openAIApiMode)),
                    Encoding.UTF8,
                    "application/json")
            };
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Headers.Add("X-Client-Request-Id", GuidGenerator.Create().ToString("D"));
        return request;
    }

    private static object CreateEmbeddingsTestPayload(string model)
    {
        return new Dictionary<string, object?>
        {
            ["model"] = model,
            ["input"] = "ping"
        };
    }

    private string? EncryptApiKey(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return apiKey;
        }

        return _stringEncryptor.Encrypt(apiKey);
    }

    private static WorkspaceCapabilityReadinessDto MapCapabilityReadiness(
        WorkspaceRuntimeConfiguration result)
    {
        return new WorkspaceCapabilityReadinessDto
        {
            CapabilityType = result.CapabilityType,
            IsConfigured = result.IsConfigured,
            IsReady = result.IsReady,
            Provider = result.Provider,
            ModelId = NullIfWhiteSpace(result.ModelId),
            OpenAIApiMode = result.OpenAIApiMode,
            HasApiEndpoint = !string.IsNullOrWhiteSpace(result.ApiEndpoint),
            HasApiKey = !string.IsNullOrWhiteSpace(result.ApiKey),
            UsesWorkspaceFallback = result.IsFallback,
            FailureCode = result.FailureCode
        };
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
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
            // If decryption fails, assume it's already plain text (backward compatibility)
            return encryptedApiKey;
        }
    }

    private static string NormalizeBaseUrl(string? apiBaseUrl)
    {
        return (string.IsNullOrWhiteSpace(apiBaseUrl) ? DefaultOpenAIBaseUrl : apiBaseUrl.Trim()).TrimEnd('/');
    }

    private static object CreateTestPayload(string model, OpenAIApiMode openAIApiMode)
    {
        if (openAIApiMode == OpenAIApiMode.Responses)
        {
            return new Dictionary<string, object?>
            {
                ["model"] = model,
                ["input"] = "ping",
                ["max_output_tokens"] = 16
            };
        }

        return new Dictionary<string, object?>
        {
            ["model"] = model,
            ["messages"] = new[]
            {
                new Dictionary<string, string>
                {
                    ["role"] = "user",
                    ["content"] = "ping"
                }
            }
        };
    }

    private string BuildConnectionTestErrorMessage(HttpResponseMessage response, ProviderError error, string? requestId)
    {
        var message = StripProviderPrefix(error.Message);
        if (string.IsNullOrWhiteSpace(message))
        {
            message = $"{(int)response.StatusCode} {response.ReasonPhrase}";
        }

        var details = new List<string>
        {
            L["ConnectionTestFailed"].Value,
            message
        };

        if (!string.IsNullOrWhiteSpace(error.Param))
        {
            details.Add($"{L["Parameter"].Value}: {error.Param}");
        }

        if (!string.IsNullOrWhiteSpace(error.Code))
        {
            details.Add($"{L["Code"].Value}: {error.Code}");
        }

        if (!string.IsNullOrWhiteSpace(requestId))
        {
            details.Add($"{L["RequestId"].Value}: {requestId}");
        }

        return string.Join(" | ", details);
    }

    private string BuildModelListErrorMessage(HttpResponseMessage response, ProviderError error, string? requestId)
    {
        var message = StripProviderPrefix(error.Message);
        if (string.IsNullOrWhiteSpace(message))
        {
            message = $"{(int)response.StatusCode} {response.ReasonPhrase}";
        }

        var details = new List<string>
        {
            L["LoadModelsFailed"].Value,
            message
        };

        if (!string.IsNullOrWhiteSpace(error.Code))
        {
            details.Add($"{L["Code"].Value}: {error.Code}");
        }

        if (!string.IsNullOrWhiteSpace(requestId))
        {
            details.Add($"{L["RequestId"].Value}: {requestId}");
        }

        return string.Join(" | ", details);
    }

    private static List<OpenAIModelDto> ParseModels(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array)
        {
            return new List<OpenAIModelDto>();
        }

        return data
            .EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.Object)
            .Select(item => new OpenAIModelDto
            {
                Id = TryGetString(item, "id") ?? string.Empty,
                OwnedBy = TryGetString(item, "owned_by"),
                Created = TryGetInt64(item, "created")
            })
            .Where(model => !string.IsNullOrWhiteSpace(model.Id))
            .OrderBy(model => model.Id)
            .ToList();
    }

    private static long? TryGetInt64(JsonElement element, string propertyName)
    {
        return element.ValueKind == JsonValueKind.Object &&
               element.TryGetProperty(propertyName, out var property) &&
               property.ValueKind == JsonValueKind.Number &&
               property.TryGetInt64(out var value)
            ? value
            : null;
    }

    private static string SanitizeBaseUrlForLog(string baseUrl)
    {
        return Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri)
            ? uri.GetLeftPart(UriPartial.Authority)
            : "custom provider endpoint";
    }

    private static ProviderError ParseProviderError(string? responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return ProviderError.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return new ProviderError(TrimError(root.ToString()), null, null, null, null);
            }

            var errorElement = root.TryGetProperty("error", out var error) ? error : root;
            if (errorElement.ValueKind != JsonValueKind.Object)
            {
                return new ProviderError(
                    TrimError(errorElement.ValueKind == JsonValueKind.String ? errorElement.GetString() : errorElement.ToString()),
                    null,
                    null,
                    null,
                    TryGetString(root, "request_id") ?? TryGetString(root, "_request_id"));
            }

            return new ProviderError(
                TryGetString(errorElement, "message") ?? TryGetString(root, "message"),
                TryGetString(errorElement, "type"),
                TryGetString(errorElement, "param"),
                TryGetString(errorElement, "code"),
                TryGetString(root, "request_id") ?? TryGetString(root, "_request_id"));
        }
        catch (JsonException)
        {
            return new ProviderError(TrimError(responseBody), null, null, null, null);
        }
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

    private static string? StripProviderPrefix(string? message)
    {
        const string providerPrefix = "Provider API error:";
        if (string.IsNullOrWhiteSpace(message))
        {
            return message;
        }

        return message.StartsWith(providerPrefix, StringComparison.OrdinalIgnoreCase)
            ? message[providerPrefix.Length..].Trim()
            : message.Trim();
    }

    private static string TrimError(string? responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return string.Empty;
        }

        return responseBody.Length <= 500 ? responseBody : responseBody[..500];
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
