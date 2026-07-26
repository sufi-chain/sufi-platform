using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.SufiAI.Configuration;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Encryption;

namespace SufiChain.SufiPlatform.SufiAI.Data;

public class DefaultAiWorkspaceSeeder : IDefaultAiWorkspaceSeeder
{
    protected IWorkspaceRepository WorkspaceRepository { get; }
    protected IGuidGenerator GuidGenerator { get; }
    protected ICurrentTenant CurrentTenant { get; }
    protected IStringEncryptionService StringEncryptor { get; }
    protected AIOptions AIOptions { get; }
    protected ILogger<DefaultAiWorkspaceSeeder> Logger { get; }

    public DefaultAiWorkspaceSeeder(
        IWorkspaceRepository workspaceRepository,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IStringEncryptionService stringEncryptor,
        IOptions<AIOptions> aiOptions,
        ILogger<DefaultAiWorkspaceSeeder> logger)
    {
        WorkspaceRepository = workspaceRepository;
        GuidGenerator = guidGenerator;
        CurrentTenant = currentTenant;
        StringEncryptor = stringEncryptor;
        AIOptions = aiOptions.Value;
        Logger = logger;
    }

    public virtual async Task<Guid?> EnsureDefaultWorkspaceAsync(CancellationToken cancellationToken = default)
    {
        if (!AIOptions.SeedDefaultWorkspace)
        {
            Logger.LogDebug("Default AI workspace seeding is disabled.");
            return null;
        }

        var seed = AIOptions.DefaultWorkspace;
        var workspaceName = string.IsNullOrWhiteSpace(seed.Name)
            ? AIWorkspaceNames.Default
            : seed.Name.Trim();

        var existing = await WorkspaceRepository.FindByNameAsync(workspaceName, cancellationToken);
        if (existing != null)
        {
            Logger.LogDebug(
                "Default AI workspace '{WorkspaceName}' already exists for tenant {TenantId}; administrator-owned configuration is unchanged.",
                workspaceName,
                CurrentTenant.Id);
            return existing.Id;
        }

        var workspace = new Workspace(
            GuidGenerator.Create(),
            workspaceName,
            seed.Provider,
            seed.Model,
            CurrentTenant.Id);

        workspace.UpdateConfiguration(
            seed.Model,
            EncryptApiKey(seed.ApiKey),
            seed.ApiBaseUrl,
            null,
            seed.Temperature,
            seed.MaxContextTokens,
            seed.OpenAIApiMode);

        EnsureDefaultModelConfigurations(workspace, seed);

        await WorkspaceRepository.InsertAsync(workspace, autoSave: true, cancellationToken);

        Logger.LogInformation(
            "Seeded default AI workspace '{WorkspaceName}' with model '{Model}' for tenant {TenantId}.",
            workspaceName,
            seed.Model,
            CurrentTenant.Id);

        return workspace.Id;
    }

    protected virtual void EnsureDefaultModelConfigurations(
        Workspace workspace,
        DefaultWorkspaceSeedOptions seed)
    {
        var configuredModels = new (AICapabilityType Capability, string? ModelId)[]
        {
            (AICapabilityType.ChatCompletion, seed.Model),
            (AICapabilityType.Embeddings, seed.EmbeddingModel),
            (AICapabilityType.AudioTranscription, seed.AudioModel),
            (AICapabilityType.TextToSpeech, seed.TtsModel),
            (AICapabilityType.VisionAnalysis, seed.VisionModel),
            (AICapabilityType.ImageGeneration, seed.ImageModel)
        };

        foreach (var (capability, configuredModelId) in configuredModels)
        {
            if (string.IsNullOrWhiteSpace(configuredModelId))
            {
                continue;
            }

            var modelId = configuredModelId.Trim();
            workspace.AddModelConfiguration(
                capability,
                modelId,
                apiEndpoint: seed.ApiBaseUrl,
                apiKey: null,
                priority: 0,
                openAIApiMode: seed.OpenAIApiMode);

            Logger.LogInformation(
                "Seeded AI model configuration {Capability}='{ModelId}' on workspace {WorkspaceId} for tenant {TenantId}.",
                capability,
                modelId,
                workspace.Id,
                CurrentTenant.Id);
        }
    }

    protected virtual string? EncryptApiKey(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return apiKey;
        }

        return StringEncryptor.Encrypt(apiKey);
    }
}
