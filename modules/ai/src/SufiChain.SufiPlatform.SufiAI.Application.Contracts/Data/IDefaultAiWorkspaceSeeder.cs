using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Data;

public interface IDefaultAiWorkspaceSeeder : ITransientDependency
{
    /// <summary>
    /// Seeds the host default workspace. An ambient tenant is not a seed request.
    /// </summary>
    Task<Guid?> EnsureDefaultWorkspaceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Seeds the host when <paramref name="tenantId"/> is null, or that tenant when it is set.
    /// The id is the seed context. The ambient tenant is not used.
    /// </summary>
    Task<Guid?> EnsureDefaultWorkspaceAsync(Guid? tenantId, CancellationToken cancellationToken = default);
}
