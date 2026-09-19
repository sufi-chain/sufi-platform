using System;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI;

public static class TestHooshvarePolicy
{
    public static readonly AsyncLocal<AIHooshvareModelSelectionPolicy?> Current = new();
}

public class TestHooshvareModelSelectionPolicyProvider : IAIHooshvareModelSelectionPolicyProvider
{
    public Task<AIHooshvareModelSelectionPolicy?> FindAsync(
        Guid hooshvareId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TestHooshvarePolicy.Current.Value);
    }
}
