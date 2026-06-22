using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiAbp.AI.Features;
using Volo.Abp.Application.Services;
using SufiChain.SufiAbp.Features;
using Volo.Abp.Security.Encryption;
using SufiChain.SufiAbp.AI.Permissions;
using SufiChain.SufiAbp.Application.Dtos;
using Microsoft.Extensions.Logging;
using SufiChain.SufiAbp.AI.MCP.Abstractions;
using SufiChain.SufiAbp.AI.MCP.Tools;

namespace SufiChain.SufiAbp.AI.Workspaces;

[RequiresFeature(SufiAIFeatures.Enable)]
[Authorize(AIPermissions.Workspaces.Default)]
public class WorkspaceAppService : ApplicationService, IWorkspaceAppService
{
    private const string DefaultOpenAIBaseUrl = "https://api.openai.com/v1";

    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly WorkspaceManager _workspaceManager;
    private readonly IStringEncryptionService _stringEncryptor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMCPToolRegistry _toolRegistry;

    public WorkspaceAppService(
        IWorkspaceRepository workspaceRepository,
        WorkspaceManager workspaceManager,
        IStringEncryptionService stringEncryptor,
        IHttpClientFactory httpClientFactory,
        IMCPToolRegistry toolRegistry)
    {
        _workspaceRepository = workspaceRepository;
        _workspaceManager = workspaceManager;
        _stringEncryptor = stringEncryptor;
        _httpClientFactory = httpClientFactory;
        _toolRegistry = toolRegistry;
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
        var workspace = await _workspaceRepository.GetAsync(id);
        return ObjectMapper.Map<Workspace, WorkspaceDto>(workspace);
    }

    [Authorize(AIPermissions.Workspaces.Create)]
    public async Task<WorkspaceDto> CreateAsync(CreateWorkspaceDto input)
    {
        await _workspaceManager.ValidateNameAsync(input.Name);

        var workspace = new Workspace(
            GuidGenerator.Create(),
            input.Name,
            AIProviderType.OpenAI,
            input.Model,
            CurrentTenant.Id
        );

        workspace.UpdateConfiguration(
            input.Model,
            EncryptApiKey(input.ApiKey),
            input.ApiBaseUrl,
            input.SystemPrompt,
            input.Temperature,
            input.MaxTokens,
            input.OpenAIApiMode,
            input.InputCostPer1MTokens,
            input.OutputCostPer1MTokens
        );

        if (input.EmbedderConfig != null)
        {
            var embedderConfig = input.EmbedderConfig;
            embedderConfig.ApiKey = EncryptApiKey(embedderConfig.ApiKey);
            workspace.SetEmbedderConfig(JsonSerializer.Serialize(embedderConfig));
        }

        if (input.VectorStoreConfig != null)
        {
            workspace.SetVectorStoreConfig(JsonSerializer.Serialize(input.VectorStoreConfig));
        }

        await _workspaceRepository.InsertAsync(workspace, autoSave: true);

        return ObjectMapper.Map<Workspace, WorkspaceDto>(workspace);
    }

    [Authorize(AIPermissions.Workspaces.Edit)]
    public async Task<WorkspaceDto> UpdateAsync(Guid id, UpdateWorkspaceDto input)
    {
        var workspace = await _workspaceRepository.GetAsync(id);

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
            input.MaxTokens,
            input.OpenAIApiMode,
            input.InputCostPer1MTokens,
            input.OutputCostPer1MTokens
        );

        if (input.IsActive)
            workspace.Activate();
        else
            workspace.Deactivate();

        if (input.EmbedderConfig != null)
        {
            var embedderConfig = input.EmbedderConfig;
            
            // Only update embedder API key if a new one is provided
            if (!string.IsNullOrWhiteSpace(embedderConfig.ApiKey))
            {
                embedderConfig.ApiKey = EncryptApiKey(embedderConfig.ApiKey);
            }
            else if (!string.IsNullOrWhiteSpace(workspace.EmbedderConfigJson))
            {
                // Keep existing API key from current config
                var existingConfig = JsonSerializer.Deserialize<EmbedderConfigDto>(workspace.EmbedderConfigJson);
                if (existingConfig != null)
                {
                    embedderConfig.ApiKey = existingConfig.ApiKey;
                }
            }
            
            workspace.SetEmbedderConfig(JsonSerializer.Serialize(embedderConfig));
        }

        if (input.VectorStoreConfig != null)
        {
            workspace.SetVectorStoreConfig(JsonSerializer.Serialize(input.VectorStoreConfig));
        }

        await _workspaceRepository.UpdateAsync(workspace, autoSave: true);

        return ObjectMapper.Map<Workspace, WorkspaceDto>(workspace);
    }

    [Authorize(AIPermissions.Workspaces.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        await _workspaceRepository.DeleteAsync(id, autoSave: true);
    }

    public async Task<List<OpenAIModelDto>> GetAvailableModelsAsync(GetOpenAIModelsInput input)
    {
        var apiKey = input.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey) && input.WorkspaceId.HasValue)
        {
            var workspace = await _workspaceRepository.GetAsync(input.WorkspaceId.Value);
            apiKey = DecryptApiKey(workspace.ApiKey);
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new Volo.Abp.UserFriendlyException(L["ApiKeyRequiredForModelList"]);
        }

        var baseUrl = NormalizeBaseUrl(input.ApiBaseUrl);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/models");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Headers.Add("X-Client-Request-Id", GuidGenerator.Create().ToString("D"));

        using var response = await _httpClientFactory.CreateClient().SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            var error = ParseProviderError(responseBody);
            var requestId = ReadRequestId(response, error);
            Logger.LogWarning(
                "OpenAI-compatible model list request failed. BaseUrl: {BaseUrl}, Status: {StatusCode}, ErrorCode: {ErrorCode}, Param: {Param}, RequestId: {RequestId}, Body: {Body}",
                SanitizeBaseUrlForLog(baseUrl),
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

        var apiKey = input.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey) && input.WorkspaceId.HasValue)
        {
            var workspace = await _workspaceRepository.GetAsync(input.WorkspaceId.Value);
            apiKey = DecryptApiKey(workspace.ApiKey);
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new Volo.Abp.UserFriendlyException(L["ApiKeyRequiredForConnectionTest"]);
        }

        var baseUrl = NormalizeBaseUrl(input.ApiBaseUrl);
        var requestUri = input.OpenAIApiMode == OpenAIApiMode.Responses
            ? $"{baseUrl}/responses"
            : $"{baseUrl}/chat/completions";

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Headers.Add("X-Client-Request-Id", GuidGenerator.Create().ToString("D"));
        request.Content = new StringContent(
            JsonSerializer.Serialize(CreateTestPayload(input.Model, input.OpenAIApiMode)),
            Encoding.UTF8,
            "application/json"
        );

        using var response = await _httpClientFactory.CreateClient().SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        var error = ParseProviderError(responseBody);
        var requestId = ReadRequestId(response, error);
        Logger.LogWarning(
            "OpenAI-compatible workspace connection test failed. Mode: {OpenAIApiMode}, Model: {Model}, BaseUrl: {BaseUrl}, Status: {StatusCode}, ErrorCode: {ErrorCode}, Param: {Param}, RequestId: {RequestId}, Body: {Body}",
            input.OpenAIApiMode,
            input.Model,
            SanitizeBaseUrlForLog(baseUrl),
            (int)response.StatusCode,
            error.Code,
            error.Param,
            requestId,
            TrimError(responseBody));

        throw new Volo.Abp.UserFriendlyException(
            BuildConnectionTestErrorMessage(response, error, requestId)
        );
    }

    [Authorize(AIPermissions.Workspaces.Edit)]
    public async Task<WorkspaceMCPToolConfigurationDto> GetMCPToolConfigurationAsync(Guid id)
    {
        var workspace = await _workspaceRepository.GetAsync(id);
        var enabledToolNames = ReadEnabledMCPToolNames(workspace);
        var tools = await _toolRegistry.GetAllToolsForWorkspaceAsync(workspace.Name);

        return new WorkspaceMCPToolConfigurationDto
        {
            AvailableTools = tools.Select(tool => new MCPToolDto
            {
                Name = tool.Name,
                Description = tool.Description,
                ParameterSchema = tool.ParameterSchema,
                ToolType = tool.ToolType.ToString(),
                Source = tool.Source
            }).ToList(),
            EnabledToolNames = enabledToolNames
        };
    }

    [Authorize(AIPermissions.Workspaces.Edit)]
    public async Task UpdateMCPToolConfigurationAsync(Guid id, UpdateWorkspaceMCPToolConfigurationDto input)
    {
        var workspace = await _workspaceRepository.GetAsync(id);
        var tools = await _toolRegistry.GetAllToolsForWorkspaceAsync(workspace.Name);
        var availableToolNames = tools.Select(tool => tool.Name).ToHashSet();
        var enabledToolNames = input.EnabledToolNames
            .Where(availableToolNames.Contains)
            .Distinct()
            .OrderBy(toolName => toolName)
            .ToList();

        workspace.SetEnabledMCPTools(JsonSerializer.Serialize(enabledToolNames));

        await _workspaceRepository.UpdateAsync(workspace, autoSave: true);
    }

    private string? EncryptApiKey(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return apiKey;
        }

        return _stringEncryptor.Encrypt(apiKey);
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

    private static List<string> ReadEnabledMCPToolNames(Workspace workspace)
    {
        if (string.IsNullOrWhiteSpace(workspace.EnabledMCPToolsJson))
        {
            return new List<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(workspace.EnabledMCPToolsJson) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
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
