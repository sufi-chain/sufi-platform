using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI;

[Dependency(TryRegister = true)]
public class NullAICopilotModelSelectionPolicyProvider :
    IAICopilotModelSelectionPolicyProvider,
    ITransientDependency
{
    public Task<AICopilotModelSelectionPolicy?> FindAsync(
        Guid copilotId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<AICopilotModelSelectionPolicy?>(null);
    }
}
