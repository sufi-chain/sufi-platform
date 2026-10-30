using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.SufiAI.Data;

/// <summary>
/// Seeds the host default AI workspace. For tenants, resolves an existing dedicated
/// workspace or ensures inheritance of the host default — tenants are never given a
/// separately seeded dedicated default workspace.
/// </summary>
public class DefaultWorkspaceDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    protected IDefaultAiWorkspaceSeeder DefaultAiWorkspaceSeeder { get; }
    protected ICurrentTenant CurrentTenant { get; }
    protected ILogger<DefaultWorkspaceDataSeedContributor> Logger { get; }

    public DefaultWorkspaceDataSeedContributor(
        IDefaultAiWorkspaceSeeder defaultAiWorkspaceSeeder,
        ICurrentTenant currentTenant,
        ILogger<DefaultWorkspaceDataSeedContributor> logger)
    {
        DefaultAiWorkspaceSeeder = defaultAiWorkspaceSeeder;
        CurrentTenant = currentTenant;
        Logger = logger;
    }

    public virtual async Task SeedAsync(DataSeedContext context)
    {
        // Host (TenantId null) or the tenant named on the seed context. A tenant that is
        // only ambient must not become a host seed or a tenant projection.
        if (context.TenantId == null && CurrentTenant.Id != null)
        {
            Logger.LogDebug(
                "Skipping default AI workspace seed. Tenant {TenantId} is ambient and the seed context did not name it.",
                CurrentTenant.Id);
            return;
        }

        var workspaceId = await DefaultAiWorkspaceSeeder.EnsureDefaultWorkspaceAsync(context.TenantId);
        if (workspaceId.HasValue)
        {
            Logger.LogDebug(
                "Default AI workspace is ready with ID {WorkspaceId} for tenant {TenantId}.",
                workspaceId.Value,
                context.TenantId);
        }
    }
}
