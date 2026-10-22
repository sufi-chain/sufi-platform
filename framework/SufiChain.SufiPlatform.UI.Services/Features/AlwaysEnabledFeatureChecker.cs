using SufiChain.SufiPlatform.UI.Features;

namespace SufiChain.SufiPlatform.UI.Services.Features;

/// <summary>
/// Default feature checker that treats every feature as enabled.
/// Useful for apps and tests without feature management.
/// Replace with an ABP-backed implementation when feature management is active.
/// </summary>
public class AlwaysEnabledFeatureChecker : ISufiFeatureChecker
{
    public Task<bool> IsEnabledAsync(string featureName)
    {
        return Task.FromResult(true);
    }

    public Task<Dictionary<string, bool>> IsEnabledAsync(IEnumerable<string> featureNames)
    {
        var result = featureNames.Distinct(StringComparer.Ordinal).ToDictionary(name => name, _ => true);
        return Task.FromResult(result);
    }
}
