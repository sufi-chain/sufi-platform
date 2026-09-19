using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI;

public interface IAIHooshvareModelSelectionPolicyProvider
{
    Task<AIHooshvareModelSelectionPolicy?> FindAsync(
        Guid hooshvareId,
        CancellationToken cancellationToken = default);
}

public class AIHooshvareModelSelectionPolicy
{
    public Guid HooshvareId { get; set; }

    public Guid WorkspaceId { get; set; }

    public bool AllowUserModelSelection { get; set; }

    public IReadOnlyList<Guid> AllowedModelConfigurationIds { get; set; } = Array.Empty<Guid>();
}
