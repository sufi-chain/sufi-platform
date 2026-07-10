using Microsoft.Extensions.Logging;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.AI.Data;

/// <summary>
/// Seeds the default AI workspace for each host and tenant data seed scope.
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
        using (CurrentTenant.Change(context?.TenantId))
        {
            var workspaceId = await DefaultAiWorkspaceSeeder.EnsureDefaultWorkspaceAsync();
            if (workspaceId.HasValue)
            {
                Logger.LogDebug(
                    "Default AI workspace is ready with ID {WorkspaceId} for tenant {TenantId}.",
                    workspaceId.Value,
                    context?.TenantId);
            }
        }
    }
}
