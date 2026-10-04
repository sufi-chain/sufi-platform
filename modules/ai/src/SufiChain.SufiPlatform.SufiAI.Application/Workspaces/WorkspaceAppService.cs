using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.SufiAI.Catalog;
using SufiChain.SufiPlatform.SufiAI.Configuration;
using SufiChain.SufiPlatform.SufiAI.Data;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using SufiChain.SufiPlatform.Tenants;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Encryption;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

[RequiresFeature(SufiAIFeatures.Enable)]
[Authorize(AIPermissions.Workspaces.Default)]
public class WorkspaceAppService : SufiApplicationService, IWorkspaceAppService
{
    private const string DefaultOpenAIBaseUrl = "https://api.openai.com/v1";

    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IAIModelConfigurationRepository _modelConfigurationRepository;
    private readonly IWorkspaceAssignmentRepository _assignmentRepository;
    private readonly IWorkspaceGuardrailService _workspaceGuardrailService;
    private readonly IInheritedWorkspaceProjectionSynchronizer _inheritedWorkspaceProjectionSynchronizer;
    private readonly ITenantStore _tenantStore;
    private readonly WorkspaceManager _workspaceManager;
    private readonly IStringEncryptionService _stringEncryptor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWorkspaceRuntimeConfigurationResolver _runtimeConfigurationResolver;
    private readonly WorkspaceSyncService _workspaceSyncService;
    private readonly IDistributedCache<ProviderModelListCacheItem> _providerModelListCache;
    private readonly IDistributedCache<OpenRouterConnectionCatalogCacheItem> _openRouterCatalogCache;
    private readonly IModelCatalogSource _modelCatalog;
    private readonly IEnumerable<IAiProviderProfile> _profiles;
    private readonly IModelEndpointLookup _endpointLookup;
    private readonly DistributedRefreshGate _refresh;
    private readonly HostPriceMarkup _hostPriceMarkup;
    private readonly AIOptions _aiOptions;
    private readonly IEnumerable<IAiWorkspaceUsageChecker> _workspaceUsageCheckers;
    private readonly IDefaultAiWorkspaceSeeder _defaultAiWorkspaceSeeder;

    public WorkspaceAppService(
        IWorkspaceRepository workspaceRepository,
        IAIModelConfigurationRepository modelConfigurationRepository,
        IWorkspaceAssignmentRepository assignmentRepository,
        IWorkspaceGuardrailService workspaceGuardrailService,
        IInheritedWorkspaceProjectionSynchronizer inheritedWorkspaceProjectionSynchronizer,
        ITenantStore tenantStore,
        WorkspaceManager workspaceManager,
        IStringEncryptionService stringEncryptor,
        IHttpClientFactory httpClientFactory,
        IWorkspaceRuntimeConfigurationResolver runtimeConfigurationResolver,
        WorkspaceSyncService workspaceSyncService,
        IDistributedCache<ProviderModelListCacheItem> providerModelListCache,
        IDistributedCache<OpenRouterConnectionCatalogCacheItem> openRouterCatalogCache,
        IModelCatalogSource modelCatalog,
        IEnumerable<IAiProviderProfile> profiles,
        IModelEndpointLookup endpointLookup,
        DistributedRefreshGate refresh,
        HostPriceMarkup hostPriceMarkup,
        IOptions<AIOptions> aiOptions,
        IEnumerable<IAiWorkspaceUsageChecker> workspaceUsageCheckers,
        IDefaultAiWorkspaceSeeder defaultAiWorkspaceSeeder)
    {
        _workspaceRepository = workspaceRepository;
        _modelConfigurationRepository = modelConfigurationRepository;
        _assignmentRepository = assignmentRepository;
        _workspaceGuardrailService = workspaceGuardrailService;
        _inheritedWorkspaceProjectionSynchronizer = inheritedWorkspaceProjectionSynchronizer;
        _tenantStore = tenantStore;
        _workspaceManager = workspaceManager;
        _stringEncryptor = stringEncryptor;
        _httpClientFactory = httpClientFactory;
        _runtimeConfigurationResolver = runtimeConfigurationResolver;
        _workspaceSyncService = workspaceSyncService;
        _providerModelListCache = providerModelListCache;
        _openRouterCatalogCache = openRouterCatalogCache;
        _modelCatalog = modelCatalog;
        _profiles = profiles;
        _endpointLookup = endpointLookup;
        _refresh = refresh;
        _hostPriceMarkup = hostPriceMarkup;
        _aiOptions = aiOptions.Value;
        _workspaceUsageCheckers = workspaceUsageCheckers;
        _defaultAiWorkspaceSeeder = defaultAiWorkspaceSeeder;
    }

    public Task<List<AiProviderProfileDto>> GetProviderProfilesAsync()
    {
        var profiles = _profiles
            .OrderBy(profile => profile.DisplayName ?? string.Empty, StringComparer.Ordinal)
            .Select(profile => new AiProviderProfileDto
            {
                ProviderType = profile.ProviderType,
                CapabilityKind = profile.CapabilityKind,
                DisplayName = profile.DisplayName,
                DefaultBaseUrl = profile.DefaultBaseUrl,
                RequiresExplicitBaseUrl = profile.RequiresExplicitBaseUrl,
                SupportsDecisions = profile.SupportsDecisions
            })
            .ToList();
        return Task.FromResult(profiles);
    }

    public async Task<PagedResultDto<WorkspaceDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        await _inheritedWorkspaceProjectionSynchronizer.EnsureCurrentTenantAsync();
        await EnsureHostDefaultWorkspaceWhenNoneExistAsync();
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

    public async Task<List<WorkspaceDto>> GetLookupAsync()
    {
        await _inheritedWorkspaceProjectionSynchronizer.EnsureCurrentTenantAsync();
        await EnsureHostDefaultWorkspaceWhenNoneExistAsync();
        var workspaces = await _workspaceRepository.GetListAsync(
            skipCount: 0,
            maxResultCount: int.MaxValue,
            sorting: "Name");
        return ObjectMapper.Map<List<Workspace>, List<WorkspaceDto>>(workspaces);
    }

    public async Task<WorkspaceDto> GetAsync(Guid id)
    {
        await _inheritedWorkspaceProjectionSynchronizer.EnsureCurrentTenantAsync();
        var workspace = await _workspaceRepository.GetAsync(id, includeDetails: true);
        return ObjectMapper.Map<Workspace, WorkspaceDto>(workspace);
    }

    public async Task<WorkspaceReadinessDto> GetReadinessAsync(Guid id)
    {
        await _inheritedWorkspaceProjectionSynchronizer.EnsureCurrentTenantAsync();
        var workspace = await _workspaceRepository.GetAsync(id, includeDetails: true);
        var capabilityResults = new List<WorkspaceRuntimeConfiguration>();
        foreach (var capabilityType in Enum.GetValues<AICapabilityType>())
        {
            capabilityResults.Add(_runtimeConfigurationResolver.Resolve(workspace, capabilityType));
        }

        var chat = capabilityResults.Single(
            result => result.CapabilityType == AICapabilityType.ChatCompletion);
        var mcpFailureCode = chat.FailureCode;
        if (mcpFailureCode == null && chat.Provider is not (
                AIProviderType.OpenAI
                or AIProviderType.OpenAICompatible
                or AIProviderType.OpenRouter
                or AIProviderType.HuggingFace
                or AIProviderType.AvalAI
                or AIProviderType.Liara))
        {
            mcpFailureCode = WorkspaceRuntimeFailureCodes.McpProviderNotSupported;
        }
        return new WorkspaceReadinessDto
        {
            WorkspaceId = chat.Workspace.Id,
            WorkspaceName = chat.Workspace.Name,
            IsActive = chat.Workspace.IsActive,
            IsConfigured = chat.IsConfigured,
            IsReady = chat.IsReady,
            Capabilities = capabilityResults.Select(MapCapabilityReadiness).ToList(),
            ToolCapability = new WorkspaceToolCapabilityDto
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

    /// <summary>
    /// Host admins have no tenant workspace. Create the configured host default
    /// only when the host scope is empty, so an existing host row is not duplicated.
    /// </summary>
    protected virtual async Task EnsureHostDefaultWorkspaceWhenNoneExistAsync()
    {
        if (CurrentTenant.Id != null)
        {
            return;
        }

        if (await _workspaceRepository.GetCountAsync() > 0)
        {
            return;
        }

        await _defaultAiWorkspaceSeeder.EnsureDefaultWorkspaceAsync();
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
            input.InputCostPer1MTokens,
            input.OutputCostPer1MTokens
        );

        workspace.UpdatePrimaryChatConfiguration(input.Model, input.ApiBaseUrl, input.ModelDisplayName);
        ApplyDecisionsModel(workspace, input.Provider, input.DecisionsModelId);
        ApplyGuardrails(workspace, input.Guardrails);
        await _workspaceRepository.InsertAsync(workspace, autoSave: true);

        return ObjectMapper.Map<Workspace, WorkspaceDto>(workspace);
    }

    [Authorize(AIPermissions.Workspaces.Create)]
    public async Task<WorkspaceDto> CloneAsync(Guid id, CloneWorkspaceDto input)
    {
        var source = await _workspaceRepository.GetAsync(id, includeDetails: true);
        await _workspaceManager.ValidateNameAsync(input.Name);

        var clone = _workspaceManager.CreateCopy(source, input.Name, CurrentTenant.Id);
        await _workspaceRepository.InsertAsync(clone, autoSave: true);
        return ObjectMapper.Map<Workspace, WorkspaceDto>(clone);
    }

    [Authorize(AIPermissions.Workspaces.Edit)]
    public async Task<WorkspaceDto> ConvertToCustomAsync(Guid id)
    {
        var workspace = await _workspaceRepository.GetAsync(id, includeDetails: true);
        if (!workspace.IsInherited)
        {
            throw new BusinessException(AIErrorCodes.WorkspaceNotInherited)
                .WithData("WorkspaceId", id);
        }

        var assignmentId = workspace.AssignmentId;
        workspace.ClearInheritance();
        await _workspaceRepository.UpdateAsync(workspace, autoSave: true);
        await _workspaceSyncService.ClearWorkspaceCache(workspace.Name);

        if (assignmentId.HasValue)
        {
            await _inheritedWorkspaceProjectionSynchronizer.DeactivateHostAssignmentAsync(assignmentId.Value);
        }

        return ObjectMapper.Map<Workspace, WorkspaceDto>(workspace);
    }

    [Authorize(AIPermissions.Workspaces.Create)]
    public async Task<WorkspaceDto> AssignToTenantAsync(Guid id, AssignWorkspaceToTenantDto input)
    {
        EnsureHost();

        var source = await _workspaceRepository.GetAsync(id, includeDetails: true);
        if (source.IsInherited)
        {
            throw new BusinessException(AIErrorCodes.CannotAssignInheritedWorkspace)
                .WithData("WorkspaceId", id);
        }

        if (source.TenantId != null)
        {
            throw new BusinessException(AIErrorCodes.CannotAssignTenantWorkspace)
                .WithData("WorkspaceId", id);
        }

        var tenant = await _tenantStore.FindAsync(input.TenantId);
        if (tenant == null)
        {
            throw new BusinessException(AIErrorCodes.TenantNotFound)
                .WithData("TenantId", input.TenantId);
        }

        WorkspaceAssignment? existing;
        using (DataFilter.Disable<IMultiTenant>())
        {
            existing = await _assignmentRepository.FindAsync(input.TenantId, source.Id);
        }

        var assignmentId = existing?.Id ?? GuidGenerator.Create();
        var targetId = existing is { IsActive: true }
            ? existing.TargetWorkspaceId
            : GuidGenerator.Create();
        var targetName = string.IsNullOrWhiteSpace(input.Name) ? source.Name : input.Name.Trim();

        if (existing == null)
        {
            await _assignmentRepository.InsertAsync(
                new WorkspaceAssignment(assignmentId, input.TenantId, source.Id, targetId),
                autoSave: true);
        }
        else if (!existing.IsActive || existing.TargetWorkspaceId != targetId)
        {
            existing.ReplaceTarget(targetId);
            existing.Activate();
            using (DataFilter.Disable<IMultiTenant>())
            {
                await _assignmentRepository.UpdateAsync(existing, autoSave: true);
            }
        }

        var target = await _inheritedWorkspaceProjectionSynchronizer.CreateTenantProjectionAsync(
            source,
            input.TenantId,
            assignmentId,
            targetId,
            source.Id,
            targetName);

        return ObjectMapper.Map<Workspace, WorkspaceDto>(target);
    }

    [Authorize(AIPermissions.Workspaces.Default)]
    public async Task<List<WorkspaceAssignmentDto>> GetAssignmentsAsync(Guid id)
    {
        EnsureHost();
        await _workspaceRepository.GetAsync(id);

        List<WorkspaceAssignment> assignments;
        using (DataFilter.Disable<IMultiTenant>())
        {
            assignments = await _assignmentRepository.GetListBySourceWorkspaceAsync(id);
        }

        var result = new List<WorkspaceAssignmentDto>(assignments.Count);
        foreach (var assignment in assignments.OrderByDescending(x => x.CreationTime))
        {
            string? tenantName = null;
            if (assignment.TenantId.HasValue)
            {
                var tenant = await _tenantStore.FindAsync(assignment.TenantId.Value);
                tenantName = tenant?.Name;
            }

            result.Add(new WorkspaceAssignmentDto
            {
                Id = assignment.Id,
                TenantId = assignment.TenantId,
                TenantName = tenantName,
                SourceWorkspaceId = assignment.SourceWorkspaceId,
                TargetWorkspaceId = assignment.TargetWorkspaceId,
                Version = assignment.Version,
                IsActive = assignment.IsActive,
                CreationTime = assignment.CreationTime,
                CreatorId = assignment.CreatorId,
                LastModificationTime = assignment.LastModificationTime,
                LastModifierId = assignment.LastModifierId
            });
        }

        return result;
    }

    [Authorize(AIPermissions.Workspaces.Edit)]
    public async Task DeactivateAssignmentAsync(Guid assignmentId)
    {
        EnsureHost();

        WorkspaceAssignment assignment;
        using (DataFilter.Disable<IMultiTenant>())
        {
            assignment = await _assignmentRepository.GetAsync(assignmentId);
            if (!assignment.IsActive)
            {
                return;
            }

            assignment.Deactivate();
            await _assignmentRepository.UpdateAsync(assignment, autoSave: true);
        }

        if (assignment.TenantId.HasValue)
        {
            await _inheritedWorkspaceProjectionSynchronizer.DeactivateTenantProjectionAsync(
                assignment.TenantId.Value,
                assignment.TargetWorkspaceId);
        }
    }

    [Authorize(AIPermissions.Workspaces.Default)]
    public async Task<List<AssignableTenantDto>> GetAssignableTenantsAsync()
    {
        EnsureHost();

        var tenantRepository = LazyServiceProvider.LazyGetService<ITenantRepository>();
        if (tenantRepository == null)
        {
            return [];
        }

        var tenants = await tenantRepository.GetListAsync(
            sorting: nameof(Tenant.Name),
            maxResultCount: int.MaxValue,
            skipCount: 0,
            filter: null,
            includeDetails: false);
        return tenants
            .OrderBy(tenant => tenant.Name, StringComparer.OrdinalIgnoreCase)
            .Select(tenant => new AssignableTenantDto
            {
                Id = tenant.Id,
                Name = tenant.Name
            })
            .ToList();
    }

    [Authorize(AIPermissions.Workspaces.Edit)]
    public async Task<WorkspaceDto> UpdateAsync(Guid id, UpdateWorkspaceDto input)
    {
        var workspace = await _workspaceRepository.GetAsync(id, includeDetails: true);
        EnsureEditable(workspace);

        await _workspaceManager.ValidateNameAsync(input.Name, id);
        workspace.SetName(input.Name);
        workspace.SetProvider(input.Provider);

        // Only update API key if a new one is provided
        var apiKeyToUpdate = string.IsNullOrWhiteSpace(input.ApiKey) 
            ? workspace.ApiKey  // Keep existing
            : EncryptApiKey(input.ApiKey);  // Encrypt new one

        workspace.UpdateConfiguration(
            input.Model,
            apiKeyToUpdate,
            input.ApiBaseUrl,
            input.InputCostPer1MTokens,
            input.OutputCostPer1MTokens
        );
        workspace.UpdatePrimaryChatConfiguration(
            input.Model,
            input.ApiBaseUrl,
            input.ModelDisplayName);
        ApplyDecisionsModel(workspace, input.Provider, input.DecisionsModelId);

        if (input.IsActive)
            workspace.Activate();
        else
            workspace.Deactivate();

        await _workspaceRepository.UpdateAsync(workspace, autoSave: true);
        await _workspaceSyncService.ClearWorkspaceCache(workspace.Name);

        return ObjectMapper.Map<Workspace, WorkspaceDto>(workspace);
    }

    [Authorize(AIPermissions.Workspaces.Edit)]
    public async Task<WorkspaceDto> UpdateGuardrailsAsync(Guid id, UpdateWorkspaceGuardrailsDto input)
    {
        var workspace = await _workspaceRepository.GetAsync(id, includeDetails: true);
        EnsureEditable(workspace);
        ApplyGuardrails(workspace, input.Items);
        await _workspaceRepository.UpdateAsync(workspace, autoSave: true);
        await _workspaceSyncService.ClearWorkspaceCache(workspace.Name);
        return ObjectMapper.Map<Workspace, WorkspaceDto>(workspace);
    }

    [Authorize(AIPermissions.Workspaces.Default)]
    public async Task<List<WorkspaceGuardrailStatusDto>> GetGuardrailStatusAsync(Guid id)
    {
        await _inheritedWorkspaceProjectionSynchronizer.EnsureCurrentTenantAsync();
        return await _workspaceGuardrailService.GetStatusAsync(id);
    }

    [Authorize(AIPermissions.Workspaces.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var workspace = await _workspaceRepository.FindAsync(id);
        if (workspace != null)
        {
            EnsureEditable(workspace);
        }

        await EnsureNoActiveUsageAsync(id);
        await _workspaceRepository.DeleteAsync(id, autoSave: true);
        if (workspace != null)
        {
            await _workspaceSyncService.ClearWorkspaceCache(workspace.Name);
        }
    }

    private async Task EnsureNoActiveUsageAsync(Guid workspaceId)
    {
        var blocking = new List<AiWorkspaceUsage>();
        foreach (var checker in _workspaceUsageCheckers)
        {
            var usages = await checker.FindActiveUsagesAsync(workspaceId);
            blocking.AddRange(usages.Where(usage => usage.ActiveCount > 0));
        }

        if (blocking.Count == 0)
        {
            return;
        }

        var details = string.Join(
            ", ",
            blocking.Select(usage => L[usage.RelationKey, usage.ActiveCount].Value));
        throw new BusinessException(AIErrorCodes.WorkspaceInUse)
            .WithData("WorkspaceId", workspaceId)
            .WithData("Details", details);
    }

    private static void EnsureEditable(Workspace workspace)
    {
        if (workspace.IsInherited)
        {
            throw new BusinessException(AIErrorCodes.InheritedWorkspaceReadOnly)
                .WithData("WorkspaceId", workspace.Id);
        }
    }

    private void EnsureHost()
    {
        if (CurrentTenant.Id != null)
        {
            throw new BusinessException(AIErrorCodes.WorkspaceAssignmentHostOnly);
        }
    }

    private static void ApplyGuardrails(Workspace workspace, IReadOnlyList<WorkspaceGuardrailDto>? items)
    {
        var requested = (items ?? Array.Empty<WorkspaceGuardrailDto>())
            .Where(item => item.AmountUsd > 0)
            .GroupBy(item => item.Period)
            .ToDictionary(group => group.Key, group => group.Last().AmountUsd);

        foreach (var period in Enum.GetValues<WorkspaceGuardrailPeriod>())
        {
            if (requested.TryGetValue(period, out var amount))
            {
                workspace.SetGuardrail(period, amount);
            }
            else
            {
                workspace.RemoveGuardrail(period);
            }
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

        var provider = input.Provider ?? credentials.Provider;
        var cacheKey = BuildProviderModelListCacheKey(
            credentials.BaseUrl,
            provider,
            input.CapabilityType);
        var ttlSeconds = ProviderModelListCacheSeconds();
        if (ttlSeconds > 0 && !string.IsNullOrWhiteSpace(cacheKey))
        {
            var loaded = await _refresh.ReadOrRefreshAsync(
                _providerModelListCache,
                cacheKey,
                "SufiAI:AccountModels:" + cacheKey,
                ttlSeconds,
                () => LoadAccountModelsAsync(credentials, provider, input.CapabilityType),
                item => item.Unavailable);
            if (loaded?.Unavailable == true)
            {
                throw new Volo.Abp.UserFriendlyException(L["LoadModelsFailed"]);
            }

            if (loaded != null)
            {
                var catalog = ConnectionCatalog(loaded, provider);
                if (provider == AIProviderType.OpenRouter && !loaded.CatalogLoaded)
                {
                    catalog = await LoadOpenRouterConnectionCatalogAsync(credentials.BaseUrl, credentials.ApiKey!);
                }

                return await ComposeAvailableModelsAsync(
                    loaded.Models,
                    input.CapabilityType,
                    provider,
                    catalog);
            }
        }

        var models = await LoadAccountModelsAsync(credentials, provider, input.CapabilityType);
        if (models.Unavailable)
        {
            throw new Volo.Abp.UserFriendlyException(L["LoadModelsFailed"]);
        }

        return await ComposeAvailableModelsAsync(
            models.Models,
            input.CapabilityType,
            provider,
            ConnectionCatalog(models, provider));
    }

    private async Task<ProviderModelListCacheItem> LoadAccountModelsAsync(
        (string? ApiKey, string BaseUrl, AIProviderType Provider) credentials,
        AIProviderType provider,
        AICapabilityType capabilityType)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            AccountModelsUri(credentials.BaseUrl, provider, capabilityType));
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
            return new ProviderModelListCacheItem { Unavailable = true };
        }

        var json = await response.Content.ReadAsStringAsync();
        var item = new ProviderModelListCacheItem { Models = ParseModels(json) };
        if (provider == AIProviderType.OpenRouter)
        {
            var catalog = await LoadOpenRouterConnectionCatalogAsync(credentials.BaseUrl, credentials.ApiKey!);
            if (catalog != null)
            {
                item.Catalog = catalog.ToList();
                item.CatalogLoaded = true;
            }
        }

        return item;
    }

    private static IReadOnlyList<ModelCatalogEntry>? ConnectionCatalog(
        ProviderModelListCacheItem item,
        AIProviderType provider)
    {
        return provider == AIProviderType.OpenRouter && item.CatalogLoaded
            ? item.Catalog
            : null;
    }

    private async Task<IReadOnlyList<ModelCatalogEntry>?> LoadOpenRouterConnectionCatalogAsync(
        string baseUrl,
        string apiKey)
    {
        var ttlSeconds = ProviderModelListCacheSeconds();
        var cacheKey = OpenRouterCatalogCacheKey(baseUrl);
        if (ttlSeconds > 0 && !string.IsNullOrWhiteSpace(cacheKey))
        {
            var cached = await _refresh.ReadOrRefreshAsync(
                _openRouterCatalogCache,
                cacheKey,
                "SufiAI:OpenRouterConnectionCatalog:" + cacheKey,
                ttlSeconds,
                () => ReadOpenRouterConnectionCatalogAsync(baseUrl, apiKey),
                item => item.Unavailable);
            return cached is { Unavailable: false, Models.Count: > 0 } ? cached.Models : null;
        }

        var fresh = await ReadOpenRouterConnectionCatalogAsync(baseUrl, apiKey);
        return fresh is { Unavailable: false, Models.Count: > 0 } ? fresh.Models : null;
    }

    private async Task<OpenRouterConnectionCatalogCacheItem> ReadOpenRouterConnectionCatalogAsync(
        string baseUrl,
        string apiKey)
    {
        var listed = await TryReadOpenRouterCatalogAsync(baseUrl, apiKey, "output_modalities=all");
        var decisions = await TryReadOpenRouterCatalogAsync(baseUrl, apiKey, "output_modalities=decisions");
        if (listed == null && decisions == null)
        {
            return new OpenRouterConnectionCatalogCacheItem { Unavailable = true };
        }

        var merged = OpenRouterModelCatalogSource.Merge(listed, decisions);
        return new OpenRouterConnectionCatalogCacheItem
        {
            Models = merged?.ToList() ?? new List<ModelCatalogEntry>()
        };
    }

    private async Task<IReadOnlyList<ModelCatalogEntry>?> TryReadOpenRouterCatalogAsync(
        string baseUrl,
        string apiKey,
        string query)
    {
        var uri = OpenRouterModelCatalogSource.ModelsUri(baseUrl, query);
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Headers.Add("X-Client-Request-Id", GuidGenerator.Create().ToString("D"));
            using var response = await _httpClientFactory.CreateClient().SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning(
                    "OpenRouter connection catalog failed. BaseUrl: {BaseUrl}, Query: {Query}, Status: {StatusCode}.",
                    SanitizeBaseUrlForLog(baseUrl),
                    query,
                    (int)response.StatusCode);
                return null;
            }

            return OpenRouterModelCatalogSource.ParseList(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.LogWarning(
                ex,
                "OpenRouter connection catalog is unavailable. BaseUrl: {BaseUrl}, Query: {Query}.",
                SanitizeBaseUrlForLog(baseUrl),
                query);
            return null;
        }
    }

    private async Task<List<OpenAIModelDto>> ComposeAvailableModelsAsync(
        List<OpenAIModelDto> workspaceModels,
        AICapabilityType capabilityType,
        AIProviderType provider,
        IReadOnlyList<ModelCatalogEntry>? connectionCatalog)
    {
        IReadOnlyList<ModelCatalogEntry>? catalog = connectionCatalog;
        if (provider != AIProviderType.OpenRouter)
        {
            try
            {
                var catalogName = AiProviderProfiles.Find(_profiles, provider)?.CatalogName;
                catalog = string.IsNullOrWhiteSpace(catalogName)
                    ? null
                    : await _modelCatalog.GetModelsAsync(catalogName);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Logger.LogWarning(ex, "Model catalog is unavailable. Load Models will use the workspace list only.");
            }
        }

        var markupPercent = await _hostPriceMarkup.GetPercentAsync();
        return AvailableModelListComposer.Compose(workspaceModels, catalog, capabilityType, markupPercent);
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

        var apiMode = await ResolveConnectionTestApiModeAsync(input, credentials.Provider);
        using var request = CreateConnectionTestRequest(
            credentials.BaseUrl,
            credentials.ApiKey,
            input.Model,
            input.CapabilityType,
            apiMode);

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
            apiMode,
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

    private async Task<(string? ApiKey, string BaseUrl, AIProviderType Provider)> ResolveConnectionCredentialsAsync(
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

        var provider = workspace?.Provider ?? AIProviderType.OpenAI;
        var resolvedKey = apiKey;
        var resolvedBaseUrl = apiBaseUrl;
        if (modelConfiguration != null && workspace != null)
        {
            var resolved = _runtimeConfigurationResolver.Resolve(
                workspace,
                modelConfiguration.CapabilityType,
                modelConfiguration);
            if (string.IsNullOrWhiteSpace(resolvedKey))
            {
                resolvedKey = resolved.ApiKey;
            }

            if (string.IsNullOrWhiteSpace(resolvedBaseUrl))
            {
                resolvedBaseUrl = FirstNonWhiteSpace(modelConfiguration.ApiEndpoint, workspace.ApiBaseUrl);
            }
        }
        else if (workspace != null)
        {
            if (string.IsNullOrWhiteSpace(resolvedKey))
            {
                resolvedKey = DecryptStoredApiKey(workspace.ApiKey);
            }

            if (string.IsNullOrWhiteSpace(resolvedBaseUrl))
            {
                resolvedBaseUrl = workspace.ApiBaseUrl;
            }
        }

        return (resolvedKey, NormalizeBaseUrl(resolvedBaseUrl, provider), provider);
    }

    private static string? FirstNonWhiteSpace(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private string? DecryptStoredApiKey(string? encryptedApiKey)
    {
        if (string.IsNullOrWhiteSpace(encryptedApiKey))
        {
            return null;
        }

        try
        {
            var decrypted = _stringEncryptor.Decrypt(encryptedApiKey);
            return string.IsNullOrWhiteSpace(decrypted) ? null : decrypted;
        }
        catch
        {
            return encryptedApiKey;
        }
    }

    private void ApplyDecisionsModel(Workspace workspace, AIProviderType provider, string? decisionsModelId)
    {
        var profile = AiProviderProfiles.Find(_profiles, provider);
        if (profile?.SupportsDecisions != true)
        {
            return;
        }

        workspace.SetDecisionsModel(decisionsModelId);
    }

    private async Task<OpenAIApiMode> ResolveConnectionTestApiModeAsync(
        TestWorkspaceConnectionInput input,
        AIProviderType workspaceProvider)
    {
        if (input.CapabilityType is not (AICapabilityType.ChatCompletion or AICapabilityType.VisionAnalysis))
        {
            return input.OpenAIApiMode;
        }

        var provider = input.Provider ?? workspaceProvider;
        var profile = AiProviderProfiles.Find(_profiles, provider);
        var endpoints = await _endpointLookup.GetSupportedEndpointsAsync(provider, input.Model);
        return OpenAIApiModePolicy.Select(
            profile?.SupportsApiMode(OpenAIApiMode.Responses) == true,
            endpoints,
            requiresTools: false,
            requested: null);
    }

    private HttpRequestMessage CreateConnectionTestRequest(
        string baseUrl,
        string apiKey,
        string model,
        AICapabilityType capabilityType,
        OpenAIApiMode openAIApiMode)
    {
        HttpRequestMessage request;
        if (capabilityType == AICapabilityType.Decisions)
        {
            request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/systemone")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new Dictionary<string, object?>
                    {
                        ["model"] = model,
                        ["state"] = "ping",
                        ["questions"] = new Dictionary<string, object?>
                        {
                            ["ready"] = new Dictionary<string, object?>
                            {
                                ["type"] = "noul",
                                ["instructions"] = "The state is the word ping.",
                                ["criteria"] = new Dictionary<string, string>
                                {
                                    ["true"] = "The state is exactly the word ping.",
                                    ["false"] = "The state is anything else."
                                }
                            }
                        }
                    }),
                    Encoding.UTF8,
                    "application/json")
            };
        }
        else if (capabilityType == AICapabilityType.Embeddings)
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

    private static string AccountModelsUri(
        string baseUrl,
        AIProviderType provider,
        AICapabilityType capabilityType)
    {
        var uri = $"{baseUrl.TrimEnd('/')}/models";
        if (provider == AIProviderType.OpenRouter && capabilityType == AICapabilityType.Decisions)
        {
            return uri + "?output_modalities=decisions";
        }

        return uri;
    }

    private int ProviderModelListCacheSeconds()
    {
        var configured = _aiOptions.ProviderModelDiscoveryCacheSeconds;
        if (configured <= 0)
        {
            return 0;
        }

        return Math.Max(configured, AIOptions.MinimumProviderModelDiscoveryCacheSeconds);
    }

    /// <summary>
    /// One entry per gateway base URL. OpenRouter decisions uses a separate list URL, so it has its own variant.
    /// Other capabilities share the default <c>/models</c> payload.
    /// </summary>
    private static string? BuildProviderModelListCacheKey(
        string normalizedEndpoint,
        AIProviderType provider,
        AICapabilityType capabilityType)
    {
        var endpointKey = (normalizedEndpoint ?? string.Empty).Trim().TrimEnd('/').ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(endpointKey))
        {
            return null;
        }

        var listVariant = provider == AIProviderType.OpenRouter && capabilityType == AICapabilityType.Decisions
            ? "decisions"
            : "default";
        return "ai:models:" + endpointKey + ":" + listVariant;
    }

    private static string? OpenRouterCatalogCacheKey(string baseUrl)
    {
        var endpointKey = (baseUrl ?? string.Empty).Trim().TrimEnd('/').ToLowerInvariant();
        return string.IsNullOrWhiteSpace(endpointKey)
            ? null
            : "ai:openrouter-catalog:" + endpointKey;
    }

    private string NormalizeBaseUrl(string? apiBaseUrl, AIProviderType provider)
    {
        if (!string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            return apiBaseUrl.Trim().TrimEnd('/');
        }

        var profile = AiProviderProfiles.Find(_profiles, provider);
        if (profile?.RequiresExplicitBaseUrl == true)
        {
            return string.Empty;
        }

        return (profile?.DefaultBaseUrl ?? DefaultOpenAIBaseUrl).Trim().TrimEnd('/');
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
            .Select(ParseModel)
            .Where(model => !string.IsNullOrWhiteSpace(model.Id))
            .OrderBy(model => model.Id)
            .ToList();
    }

    private static OpenAIModelDto ParseModel(JsonElement item)
    {
        JsonElement? architecture = item.TryGetProperty("architecture", out var architectureElement) &&
                                    architectureElement.ValueKind == JsonValueKind.Object
            ? architectureElement
            : null;
        JsonElement? pricing = item.TryGetProperty("pricing", out var pricingElement) &&
                               pricingElement.ValueKind == JsonValueKind.Object
            ? pricingElement
            : null;
        var reasoning = OpenRouterModelCatalogSource.ReadReasoning(item);

        return new OpenAIModelDto
        {
            Id = TryGetString(item, "id") ?? string.Empty,
            OwnedBy = TryGetString(item, "owned_by"),
            Created = TryGetInt64(item, "created"),
            Mode = DecisionsMode(item, architecture),
            Modality = architecture.HasValue ? TryGetString(architecture.Value, "modality") : null,
            InputModalities = architecture.HasValue
                ? TryGetStringList(architecture.Value, "input_modalities")
                : null,
            OutputModalities = architecture.HasValue
                ? TryGetStringList(architecture.Value, "output_modalities")
                : null,
            SupportedParameters = TryGetStringList(item, "supported_parameters"),
            ContextLength = OpenRouterModelCatalogSource.ReadContextLength(item),
            ReasoningEfforts = reasoning.Efforts.Count == 0 ? null : reasoning.Efforts,
            DefaultReasoningEffort = reasoning.DefaultEffort,
            PromptPricePerToken = pricing.HasValue ? TryGetDecimal(pricing.Value, "prompt") : null,
            CompletionPricePerToken = pricing.HasValue ? TryGetDecimal(pricing.Value, "completion") : null,
            ImagePrice = pricing.HasValue ? TryGetDecimal(pricing.Value, "image") : null,
            ImageOutputPrice = pricing.HasValue ? TryGetDecimal(pricing.Value, "image_output") : null,
            ImageTokenPrice = pricing.HasValue ? TryGetDecimal(pricing.Value, "image_token") : null,
            RequestPrice = pricing.HasValue ? TryGetDecimal(pricing.Value, "request") : null,
            WebSearchPrice = pricing.HasValue ? TryGetDecimal(pricing.Value, "web_search") : null
        };
    }

    private static string? DecisionsMode(JsonElement item, JsonElement? architecture)
    {
        var outputs = architecture.HasValue ? TryGetStringList(architecture.Value, "output_modalities") : null;
        if (outputs?.Any(value => string.Equals(value, "decisions", StringComparison.OrdinalIgnoreCase)) == true)
        {
            return "decisions";
        }

        var modality = architecture.HasValue ? TryGetString(architecture.Value, "modality") : null;
        if (!string.IsNullOrWhiteSpace(modality) &&
            modality.Contains("decisions", StringComparison.OrdinalIgnoreCase))
        {
            return "decisions";
        }

        return TryGetString(item, "mode");
    }

    private static List<string>? TryGetStringList(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(propertyName, out var property) ||
            property.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var values = new List<string>();
        foreach (var item in property.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var value = item.GetString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                values.Add(value);
            }
        }

        return values.Count == 0 ? null : values;
    }

    private static decimal? TryGetDecimal(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out var number))
        {
            return number;
        }

        if (property.ValueKind == JsonValueKind.String &&
            decimal.TryParse(property.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return null;
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
