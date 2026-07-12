using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Data;

public interface IDefaultAiWorkspaceSeeder : ITransientDependency
{
    Task<Guid?> EnsureDefaultWorkspaceAsync(CancellationToken cancellationToken = default);
}
