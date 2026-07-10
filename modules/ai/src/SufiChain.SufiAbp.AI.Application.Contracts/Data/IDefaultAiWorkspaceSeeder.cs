using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AI.Data;

public interface IDefaultAiWorkspaceSeeder : ITransientDependency
{
    Task<Guid?> EnsureDefaultWorkspaceAsync(CancellationToken cancellationToken = default);
}
