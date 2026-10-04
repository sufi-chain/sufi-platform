using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Configuration;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Encryption;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.SufiAI.Data;

public class DefaultAiWorkspaceSeeder : IDefaultAiWorkspaceSeeder
{
    protected IWorkspaceRepository WorkspaceRepository { get; }
    protected IWorkspaceAssignmentRepository WorkspaceAssignmentRepository { get; }
    protected IInheritedWorkspaceProjectionSynchronizer InheritedWorkspaceProjectionSynchronizer { get; }
    protected IGuidGenerator GuidGenerator { get; }
    protected ICurrentTenant CurrentTenant { get; }
    protected IDataFilter DataFilter { get; }
    protected IUnitOfWorkManager UnitOfWorkManager { get; }
    protected IStringEncryptionService StringEncryptor { get; }
    protected AIOptions AIOptions { get; }
    protected ILogger<DefaultAiWorkspaceSeeder> Logger { get; }

    public DefaultAiWorkspaceSeeder(
        IWorkspaceRepository workspaceRepository,
        IWorkspaceAssignmentRepository workspaceAssignmentRepository,
        IInheritedWorkspaceProjectionSynchronizer inheritedWorkspaceProjectionSynchronizer,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IDataFilter dataFilter,
        IUnitOfWorkManager unitOfWorkManager,
        IStringEncryptionService stringEncryptor,
        IOptions<AIOptions> aiOptions,
        ILogger<DefaultAiWorkspaceSeeder> logger)
    {
        WorkspaceRepository = workspaceRepository;
        WorkspaceAssignmentRepository = workspaceAssignmentRepository;
        InheritedWorkspaceProjectionSynchronizer = inheritedWorkspaceProjectionSynchronizer;
        GuidGenerator = guidGenerator;
        CurrentTenant = currentTenant;
        DataFilter = dataFilter;
        UnitOfWorkManager = unitOfWorkManager;
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

        var existing = await FindDefaultWorkspaceAsync(workspaceName, cancellationToken);
        if (existing != null)
        {
            return await FinalizeExistingAsync(existing, seed, workspaceName, cancellationToken);
        }

        // Tenants never own a seeded default workspace. They inherit the host default
        // unless an administrator already converted or created a dedicated workspace.
        if (CurrentTenant.Id.HasValue)
        {
            return await EnsureTenantInheritedDefaultAsync(workspaceName, cancellationToken);
        }

        return await SeedHostDefaultWorkspaceAsync(seed, workspaceName, cancellationToken);
    }

    protected virtual async Task<Guid?> EnsureTenantInheritedDefaultAsync(
        string workspaceName,
        CancellationToken cancellationToken)
    {
        var tenantId = CurrentTenant.Id!.Value;

        await InheritedWorkspaceProjectionSynchronizer.EnsureCurrentTenantAsync(cancellationToken);

        var existing = await FindDefaultWorkspaceAsync(workspaceName, cancellationToken);
        if (existing != null)
        {
            Logger.LogDebug(
                "Tenant {TenantId} already has default AI workspace '{WorkspaceName}' ({WorkspaceId}); skipping dedicated seed.",
                tenantId,
                workspaceName,
                existing.Id);
            return existing.Id;
        }

        Workspace? hostWorkspace;
        using (CurrentTenant.Change(null))
        {
            // Change(null) does not switch a unit of work that already opened the
            // tenant SufiAI connection. A new unit of work reads the host database.
            using (var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
            {
                hostWorkspace = await WorkspaceRepository.FindByNameAsync(workspaceName, cancellationToken);
                await uow.CompleteAsync(cancellationToken);
            }
        }

        if (hostWorkspace == null)
        {
            Logger.LogWarning(
                "Host default AI workspace '{WorkspaceName}' is missing; tenant {TenantId} cannot inherit it.",
                workspaceName,
                tenantId);
            return null;
        }

        WorkspaceAssignment? assignment;
        using (DataFilter.Disable<IMultiTenant>())
        {
            assignment = await WorkspaceAssignmentRepository.FindAsync(
                tenantId,
                hostWorkspace.Id,
                cancellationToken);
        }

        var assignmentId = assignment?.Id ?? GuidGenerator.Create();
        var targetId = assignment is { IsActive: true }
            ? assignment.TargetWorkspaceId
            : GuidGenerator.Create();

        if (assignment == null)
        {
            await WorkspaceAssignmentRepository.InsertAsync(
                new WorkspaceAssignment(assignmentId, tenantId, hostWorkspace.Id, targetId),
                autoSave: true,
                cancellationToken);
        }
        else if (!assignment.IsActive || assignment.TargetWorkspaceId != targetId)
        {
            assignment.ReplaceTarget(targetId);
            assignment.Activate();
            using (DataFilter.Disable<IMultiTenant>())
            {
                await WorkspaceAssignmentRepository.UpdateAsync(assignment, autoSave: true, cancellationToken);
            }
        }

        try
        {
            var projection = await InheritedWorkspaceProjectionSynchronizer.CreateTenantProjectionAsync(
                hostWorkspace,
                tenantId,
                assignmentId,
                targetId,
                hostWorkspace.Id,
                workspaceName,
                cancellationToken);

            Logger.LogInformation(
                "Ensured inherited default AI workspace '{WorkspaceName}' ({WorkspaceId}) for tenant {TenantId} from host workspace {SourceWorkspaceId}.",
                projection.Name,
                projection.Id,
                tenantId,
                hostWorkspace.Id);

            return projection.Id;
        }
        catch (Exception ex) when (IsDuplicateKeyException(ex))
        {
            Logger.LogWarning(
                ex,
                "Inherited default AI workspace '{WorkspaceName}' already exists for tenant {TenantId}; reusing the existing row.",
                workspaceName,
                tenantId);

            var raced = await FindDefaultWorkspaceAsync(workspaceName, cancellationToken);
            if (raced != null)
            {
                return raced.Id;
            }

            throw;
        }
    }

    protected virtual async Task<Guid?> SeedHostDefaultWorkspaceAsync(
        DefaultWorkspaceSeedOptions seed,
        string workspaceName,
        CancellationToken cancellationToken)
    {
        var workspace = new Workspace(
            GuidGenerator.Create(),
            workspaceName,
            seed.Provider,
            seed.Model,
            CurrentTenant.Id);

        workspace.UpdateConfiguration(
            seed.Model,
            EncryptApiKey(seed.ApiKey),
            seed.ApiBaseUrl);

        EnsureDefaultModelConfigurations(workspace, seed);

        try
        {
            await WorkspaceRepository.InsertAsync(workspace, autoSave: true, cancellationToken);
        }
        catch (Exception ex) when (IsDuplicateKeyException(ex))
        {
            Logger.LogWarning(
                ex,
                "Default AI workspace '{WorkspaceName}' insert raced for tenant {TenantId}; reusing the existing row.",
                workspaceName,
                CurrentTenant.Id);

            var raced = await FindDefaultWorkspaceAsync(workspaceName, cancellationToken);
            if (raced != null)
            {
                return raced.Id;
            }

            throw;
        }

        Logger.LogInformation(
            "Seeded default AI workspace '{WorkspaceName}' with model '{Model}' for tenant {TenantId}.",
            workspaceName,
            seed.Model,
            CurrentTenant.Id);

        return workspace.Id;
    }

    protected virtual async Task<Guid> FinalizeExistingAsync(
        Workspace existing,
        DefaultWorkspaceSeedOptions seed,
        string workspaceName,
        CancellationToken cancellationToken)
    {
        if (existing.IsInherited)
        {
            Logger.LogDebug(
                "Default AI workspace '{WorkspaceName}' is inherited for tenant {TenantId}; seed will not mutate it.",
                workspaceName,
                CurrentTenant.Id);
            return existing.Id;
        }

        var preview = DefaultWorkspaceManagedFieldReconciler.Preview(existing, seed);
        if (preview.HasChanges)
        {
            DefaultWorkspaceManagedFieldReconciler.Apply(existing, seed, preview);
            await WorkspaceRepository.UpdateAsync(existing, autoSave: true, cancellationToken);
            Logger.LogInformation(
                "Reconciled managed default-workspace fields on '{WorkspaceName}' for tenant {TenantId}. Added capabilities: {Capabilities}.",
                workspaceName,
                CurrentTenant.Id,
                string.Join(", ", preview.MissingCapabilities));
        }
        else
        {
            Logger.LogDebug(
                "Default AI workspace '{WorkspaceName}' already exists for tenant {TenantId}; administrator-owned configuration is unchanged.",
                workspaceName,
                CurrentTenant.Id);
        }

        return existing.Id;
    }

    protected virtual async Task<Workspace?> FindDefaultWorkspaceAsync(
        string workspaceName,
        CancellationToken cancellationToken)
    {
        var existing = await WorkspaceRepository.FindByNameAsync(workspaceName, cancellationToken);
        if (existing != null)
        {
            return existing;
        }

        // Soft-deleted rows still occupy IX_SufiAI.Workspaces_TenantId_Name.
        using (DataFilter.Disable<ISoftDelete>())
        {
            existing = await WorkspaceRepository.FindByNameAsync(workspaceName, cancellationToken);
        }

        if (existing is not { IsDeleted: true })
        {
            return existing;
        }

        ObjectHelper.TrySetProperty(existing, x => x.IsDeleted, () => false);
        ObjectHelper.TrySetProperty(existing, x => x.DeleterId, () => (Guid?)null);
        ObjectHelper.TrySetProperty(existing, x => x.DeletionTime, () => (DateTime?)null);
        await WorkspaceRepository.UpdateAsync(existing, autoSave: true, cancellationToken);

        Logger.LogInformation(
            "Restored soft-deleted default AI workspace '{WorkspaceName}' ({WorkspaceId}) for tenant {TenantId}.",
            workspaceName,
            existing.Id,
            CurrentTenant.Id);

        return existing;
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
            var dimensions = capability == AICapabilityType.Embeddings
                ? (int?)EmbeddingModelDefaults.GetDimensions(modelId)
                : null;
            workspace.AddModelConfiguration(
                capability,
                modelId,
                apiEndpoint: seed.ApiBaseUrl,
                apiKey: null,
                priority: 0,
                openAIApiMode: OpenAIApiMode.ChatCompletions,
                dimensions: dimensions,
                maxContextTokens: AIModelConfiguration.DefaultMaxContextTokens);

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

    protected virtual bool IsDuplicateKeyException(Exception exception)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            if (current.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase)
                || current.Message.Contains("IX_SufiAI.Workspaces_TenantId_Name", StringComparison.OrdinalIgnoreCase)
                || current.Message.Contains("2601", StringComparison.OrdinalIgnoreCase)
                || current.Message.Contains("2627", StringComparison.OrdinalIgnoreCase)
                || current.Message.Contains("PK_", StringComparison.OrdinalIgnoreCase)
                || current.Message.Contains("IX_", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
