using System;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI;

public static class TestCopilotPolicy
{
    public static readonly AsyncLocal<AICopilotModelSelectionPolicy?> Current = new();
}

public class TestCopilotModelSelectionPolicyProvider : IAICopilotModelSelectionPolicyProvider
{
    public Task<AICopilotModelSelectionPolicy?> FindAsync(
        Guid copilotId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TestCopilotPolicy.Current.Value);
    }
}
