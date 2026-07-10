using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.AI.Configuration;
using SufiChain.SufiAbp.AI.RAG;
using SufiChain.SufiAbp.AI.Workspaces;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Encryption;

namespace SufiChain.SufiAbp.AI.Data;

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
            await EnsureEmbedderConfigAsync(existing, seed, cancellationToken);
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
            seed.SystemPrompt,
            seed.Temperature,
            seed.MaxContextTokens,
            seed.OpenAIApiMode);

        workspace.SetEmbedderConfig(BuildEmbedderConfigJson(seed));

        await WorkspaceRepository.InsertAsync(workspace, autoSave: true, cancellationToken);

        Logger.LogInformation(
            "Seeded default AI workspace '{WorkspaceName}' with model '{Model}' for tenant {TenantId}.",
            workspaceName,
            seed.Model,
            CurrentTenant.Id);

        return workspace.Id;
    }

    protected virtual async Task EnsureEmbedderConfigAsync(
        Workspace workspace,
        DefaultWorkspaceSeedOptions seed,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(workspace.EmbedderConfigJson))
        {
            return;
        }

        workspace.SetEmbedderConfig(BuildEmbedderConfigJson(seed, workspace));
        await WorkspaceRepository.UpdateAsync(workspace, autoSave: true, cancellationToken);

        Logger.LogInformation(
            "Backfilled embedder config on AI workspace '{WorkspaceName}' ({WorkspaceId}) for tenant {TenantId}.",
            workspace.Name,
            workspace.Id,
            CurrentTenant.Id);
    }

    protected virtual string BuildEmbedderConfigJson(
        DefaultWorkspaceSeedOptions seed,
        Workspace? workspace = null)
    {
        var embeddingModel = string.IsNullOrWhiteSpace(seed.EmbeddingModel)
            ? "text-embedding-3-small"
            : seed.EmbeddingModel.Trim();

        // Prefer a newly seeded plaintext key (encrypt it). Otherwise reuse the workspace's stored key.
        var apiKey = !string.IsNullOrWhiteSpace(seed.ApiKey)
            ? EncryptApiKey(seed.ApiKey)
            : workspace?.ApiKey;

        var config = new EmbedderConfiguration
        {
            Provider = workspace?.Provider ?? seed.Provider,
            Model = embeddingModel,
            ApiKey = apiKey,
            ApiBaseUrl = !string.IsNullOrWhiteSpace(seed.ApiBaseUrl)
                ? seed.ApiBaseUrl
                : workspace?.ApiBaseUrl
        };

        return JsonSerializer.Serialize(config);
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
