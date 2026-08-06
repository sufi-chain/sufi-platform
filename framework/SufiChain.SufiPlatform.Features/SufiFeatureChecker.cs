using Volo.Abp.DependencyInjection;
using AbpFeatureChecker = Volo.Abp.Features.IFeatureChecker;

namespace SufiChain.SufiPlatform.Features;

/// <summary>
/// Sufi feature checker wrapper over the ABP feature checker.
/// </summary>
[ExposeServices(typeof(IFeatureChecker))]
public class SufiFeatureChecker : IFeatureChecker, ITransientDependency
{
    private readonly AbpFeatureChecker _featureChecker;

    public SufiFeatureChecker(AbpFeatureChecker featureChecker)
    {
        _featureChecker = featureChecker;
    }

    public Task<string> GetOrNullAsync(string name)
    {
        return _featureChecker.GetOrNullAsync(name);
    }

    public Task<bool> IsEnabledAsync(string name)
    {
        return _featureChecker.IsEnabledAsync(name);
    }
}
