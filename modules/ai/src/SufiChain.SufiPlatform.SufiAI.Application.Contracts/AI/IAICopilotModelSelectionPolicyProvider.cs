using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI;

public interface IAICopilotModelSelectionPolicyProvider
{
    Task<AICopilotModelSelectionPolicy?> FindAsync(
        Guid copilotId,
        CancellationToken cancellationToken = default);
}

public class AICopilotModelSelectionPolicy
{
    public Guid CopilotId { get; set; }

    public Guid WorkspaceId { get; set; }

    public bool AllowUserModelSelection { get; set; }

    public IReadOnlyList<Guid> AllowedModelConfigurationIds { get; set; } = Array.Empty<Guid>();
}
