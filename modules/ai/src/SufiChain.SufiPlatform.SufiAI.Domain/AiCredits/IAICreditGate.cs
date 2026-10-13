using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.AiCredits;

/// <summary>
/// Pre-request AI Credit check. Null implementation allows; SaaS replaces with wallet gate.
/// </summary>
public interface IAICreditGate : ITransientDependency
{
    Task EnsureAccessAsync(Workspace workspace, CancellationToken cancellationToken = default);
}

public class NullAICreditGate : IAICreditGate
{
    public Task EnsureAccessAsync(Workspace workspace, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
