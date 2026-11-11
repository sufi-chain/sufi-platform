using System;

namespace SufiChain.SufiPlatform.Features;

/// <summary>
/// Published when a feature value is created, updated, or deleted so every host
/// drops its FeatureValue cache entry. Local entity events stay on the process
/// that saved the row; this event is what the other hosts receive.
/// </summary>
[Serializable]
public class FeatureValueChangedEto
{
    public string Name { get; set; } = default!;

    public string ProviderName { get; set; } = default!;

    public string? ProviderKey { get; set; }
}
