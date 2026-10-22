using SufiChain.SufiPlatform.UI.Features;
using Volo.Abp.Features;

namespace SufiChain.SufiPlatform.UI.Services.Features;

/// <summary>
/// Feature checker that delegates to ABP <see cref="IFeatureChecker"/>.
/// Use in hosts where feature/edition values decide what a user may open,
/// instead of <see cref="AlwaysEnabledFeatureChecker"/>.
/// </summary>
public class AbpFeatureCheckerAdapter : ISufiFeatureChecker
{
    private readonly IFeatureChecker _featureChecker;

    public AbpFeatureCheckerAdapter(IFeatureChecker featureChecker)
    {
        _featureChecker = featureChecker;
    }

    public Task<bool> IsEnabledAsync(string featureName)
    {
        return _featureChecker.IsEnabledAsync(featureName);
    }

    public async Task<Dictionary<string, bool>> IsEnabledAsync(IEnumerable<string> featureNames)
    {
        var result = new Dictionary<string, bool>(StringComparer.Ordinal);
        foreach (var featureName in featureNames.Distinct(StringComparer.Ordinal))
        {
            result[featureName] = await _featureChecker.IsEnabledAsync(featureName);
        }

        return result;
    }
}
