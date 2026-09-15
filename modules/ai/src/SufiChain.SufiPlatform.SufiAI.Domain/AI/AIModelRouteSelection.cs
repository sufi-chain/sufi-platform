using System;
using System.Collections.Generic;

namespace SufiChain.SufiPlatform.SufiAI;

public sealed class AIModelRouteSelection
{
    public Guid? ModelConfigurationId { get; init; }

    public IReadOnlyCollection<Guid>? AllowedModelConfigurationIds { get; init; }

    public bool RequiresToolCalling { get; init; }

    public static AIModelRouteSelection Implicit { get; } = new();
}
