using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI;

[Dependency(TryRegister = true)]
public class NullAIHooshvareModelSelectionPolicyProvider :
    IAIHooshvareModelSelectionPolicyProvider,
    ITransientDependency
{
    public Task<AIHooshvareModelSelectionPolicy?> FindAsync(
        Guid hooshvareId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<AIHooshvareModelSelectionPolicy?>(null);
    }
}
